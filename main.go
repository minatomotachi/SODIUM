package main

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"os"
	"strings"
	"time"

	"github.com/golang-jwt/jwt/v5"
	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/bson/primitive"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
	"golang.org/x/crypto/bcrypt"
)

var client *mongo.Client
var db *mongo.Database

func getMongoURI() string {
	uri := os.Getenv("MONGO_URI")
	if uri == "" {
		uri = "mongodb://localhost:27017/forum"
	}
	return uri
}

func getJWTSecret() []byte {
	secret := os.Getenv("JWT_SECRET")
	if secret == "" {
		secret = "sodium-dev-secret"
	}
	return []byte(secret)
}

func generateToken(userID string) (string, error) {
	claims := jwt.MapClaims{
		"user_id": userID,
		"exp":     time.Now().Add(24 * time.Hour).Unix(),
		"iat":     time.Now().Unix(),
	}
	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
	return token.SignedString(getJWTSecret())
}

func userIDFromToken(r *http.Request) (string, error) {
	auth := r.Header.Get("Authorization")
	if auth == "" || !strings.HasPrefix(auth, "Bearer ") {
		return "", fmt.Errorf("missing bearer token")
	}
	tokenString := strings.TrimPrefix(auth, "Bearer ")

	token, err := jwt.Parse(tokenString, func(t *jwt.Token) (interface{}, error) {
		if _, ok := t.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, fmt.Errorf("unexpected signing method: %v", t.Header["alg"])
		}
		return getJWTSecret(), nil
	})
	if err != nil {
		return "", err
	}

	claims, ok := token.Claims.(jwt.MapClaims)
	if !ok || !token.Valid {
		return "", fmt.Errorf("invalid token")
	}
	userID, ok := claims["user_id"].(string)
	if !ok || userID == "" {
		return "", fmt.Errorf("token missing user_id")
	}
	return userID, nil
}

func handleLogin(w http.ResponseWriter, r *http.Request) {
	var data bson.M
	if err := json.NewDecoder(r.Body).Decode(&data); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	username, _ := data["username"].(string)
	password, _ := data["password"].(string)
	if username == "" || password == "" {
		http.Error(w, "username and password required", http.StatusBadRequest)
		return
	}

	var user bson.M
	err := db.Collection("users").FindOne(context.Background(), bson.M{"username": username}).Decode(&user)
	if err == mongo.ErrNoDocuments {
		writeJSON(w, http.StatusUnauthorized, bson.M{"error": "Invalid credentials"})
		return
	} else if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	storedHash, _ := user["password"].(string)
	if bcrypt.CompareHashAndPassword([]byte(storedHash), []byte(password)) != nil {
		writeJSON(w, http.StatusUnauthorized, bson.M{"error": "Invalid credentials"})
		return
	}

	userID := user["_id"].(primitive.ObjectID).Hex()
	token, err := generateToken(userID)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	writeJSON(w, http.StatusOK, bson.M{"token": token, "user_id": userID})
}

func bcryptPassword(password string) (string, error) {
	hash, err := bcrypt.GenerateFromPassword([]byte(password), bcrypt.DefaultCost)
	if err != nil {
		return "", err
	}
	return string(hash), nil
}

func writeJSON(w http.ResponseWriter, status int, data interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	json.NewEncoder(w).Encode(data)
}

func handleDeleteUser(w http.ResponseWriter, r *http.Request) {
	userID, err := primitive.ObjectIDFromHex(r.PathValue("user_id"))
	if err != nil {
		http.Error(w, "Invalid user ID", http.StatusBadRequest)
		return
	}

	tokenUserID, err := userIDFromToken(r)
	if err != nil {
		writeJSON(w, http.StatusUnauthorized, bson.M{"error": err.Error()})
		return
	}
	if tokenUserID != userID.Hex() {
		writeJSON(w, http.StatusForbidden, bson.M{"error": "You can only delete your own account"})
		return
	}

	result, err := db.Collection("users").DeleteOne(context.Background(), bson.M{"_id": userID})
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	if result.DeletedCount == 0 {
		http.Error(w, "User not found", http.StatusNotFound)
		return
	}
	writeJSON(w, http.StatusOK, bson.M{"message": "User deleted successfully"})
}

func handleCreateUser(w http.ResponseWriter, r *http.Request) {
	var data bson.M
	if err := json.NewDecoder(r.Body).Decode(&data); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	password, _ := data["password"].(string)
	hash, err := bcryptPassword(password)
	if err != nil {
		http.Error(w, "Failed to hash password", http.StatusInternalServerError)
		return
	}

	userID := primitive.NewObjectID()
	user := bson.M{
		"_id":        userID,
		"username":   data["username"],
		"email":      data["email"],
		"password":   hash,
		"reputation": 0,
		"created_at": time.Now().UTC(),
		"profile":    data["profile"],
		"badges":     []interface{}{},
	}
	if _, ok := data["profile"]; !ok {
		user["profile"] = bson.M{}
	}

	_, err = db.Collection("users").InsertOne(context.Background(), user)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	user["_id"] = userID.Hex()
	writeJSON(w, http.StatusCreated, user)
}

func handleListUsernames(w http.ResponseWriter, r *http.Request) {
	cursor, err := db.Collection("users").Find(context.Background(), bson.M{}, options.Find().SetProjection(bson.M{"_id": 0, "username": 1}))
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	defer cursor.Close(context.Background())

	var usernames []string
	for cursor.Next(context.Background()) {
		var user bson.M
		if err := cursor.Decode(&user); err != nil {
			continue
		}
		if username, ok := user["username"].(string); ok {
			usernames = append(usernames, username)
		}
	}
	writeJSON(w, http.StatusOK, usernames)
}

func handleDeleteQuestion(w http.ResponseWriter, r *http.Request) {
	questionID := r.PathValue("question_id")

	tokenUserID, err := userIDFromToken(r)
	if err != nil {
		writeJSON(w, http.StatusUnauthorized, bson.M{"error": err.Error()})
		return
	}

	question, err := mongoGetQuestion(questionID)
	if err != nil {
		http.Error(w, "Question not found", http.StatusNotFound)
		return
	}

	ownerID, _ := question["user_id"].(string)
	if tokenUserID != ownerID {
		writeJSON(w, http.StatusForbidden, bson.M{"error": "You can only delete your own question"})
		return
	}

	deleted, err := mongoDeleteQuestion(questionID)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	if !deleted {
		http.Error(w, "Question not found", http.StatusNotFound)
		return
	}
	writeJSON(w, http.StatusOK, bson.M{"message": "Question deleted successfully"})
}

func handleCreateQuestion(w http.ResponseWriter, r *http.Request) {
	var data bson.M
	if err := json.NewDecoder(r.Body).Decode(&data); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	tokenUserID, err := userIDFromToken(r)
	if err != nil {
		writeJSON(w, http.StatusUnauthorized, bson.M{"error": err.Error()})
		return
	}
	userID, err := primitive.ObjectIDFromHex(tokenUserID)
	if err != nil {
		writeJSON(w, http.StatusUnauthorized, bson.M{"error": "valid bearer token required"})
		return
	}
	var user bson.M
	err = db.Collection("users").FindOne(context.Background(), bson.M{"_id": userID}).Decode(&user)
	if err == mongo.ErrNoDocuments {
		writeJSON(w, http.StatusNotFound, bson.M{"error": "User not found"})
		return
	} else if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	id, err := mongoInsertQuestion(
		data["title"].(string),
		data["body"].(string),
		data["tags"],
		userID.Hex(),
	)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	question := bson.M{
		"_id":        id,
		"title":      data["title"],
		"body":       data["body"],
		"tags":       data["tags"],
		"user_id":    userID.Hex(),
		"created_at": nowStr(),
		"updated_at": nowStr(),
		"views":      0,
		"votes":      0,
		"answers":    []interface{}{},
	}
	writeJSON(w, http.StatusCreated, question)
}

func handleListQuestions(w http.ResponseWriter, r *http.Request) {
	questions, err := mongoListQuestions()
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	writeJSON(w, http.StatusOK, questions)
}

func handleGetQuestion(w http.ResponseWriter, r *http.Request) {
	question, err := mongoGetQuestion(r.PathValue("question_id"))
	if err != nil {
		http.Error(w, "Question not found", http.StatusNotFound)
		return
	}
	writeJSON(w, http.StatusOK, question)
}

func handleCreateAnswer(w http.ResponseWriter, r *http.Request) {
	var data bson.M
	if err := json.NewDecoder(r.Body).Decode(&data); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	tokenUserID, err := userIDFromToken(r)
	if err != nil {
		writeJSON(w, http.StatusUnauthorized, bson.M{"error": err.Error()})
		return
	}
	userID, err := primitive.ObjectIDFromHex(tokenUserID)
	if err != nil {
		writeJSON(w, http.StatusUnauthorized, bson.M{"error": "valid bearer token required"})
		return
	}
	var user bson.M
	err = db.Collection("users").FindOne(context.Background(), bson.M{"_id": userID}).Decode(&user)
	if err == mongo.ErrNoDocuments {
		writeJSON(w, http.StatusNotFound, bson.M{"error": "User not found"})
		return
	} else if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	questionID, ok := data["question_id"].(string)
	if !ok {
		http.Error(w, "Invalid question ID", http.StatusBadRequest)
		return
	}
	if _, err := mongoGetQuestion(questionID); err != nil {
		writeJSON(w, http.StatusNotFound, bson.M{"error": "Question not found"})
		return
	}

	id, err := mongoInsertAnswer(questionID, userID.Hex(), data["body"].(string))
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	answer := bson.M{
		"_id":         id,
		"question_id": questionID,
		"user_id":     userID.Hex(),
		"body":        data["body"],
		"created_at":  nowStr(),
		"updated_at":  nowStr(),
		"votes":       0,
		"comments":    []interface{}{},
	}
	writeJSON(w, http.StatusCreated, answer)
}

func handleVoteAnswer(w http.ResponseWriter, r *http.Request) {
	answerID := r.PathValue("answer_id")

	_, err := userIDFromToken(r)
	if err != nil {
		writeJSON(w, http.StatusUnauthorized, bson.M{"error": err.Error()})
		return
	}

	if _, err := mongoGetAnswer(answerID); err != nil {
		http.Error(w, "Answer not found", http.StatusNotFound)
		return
	}

	votes, err := mongoVoteAnswer(answerID)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	writeJSON(w, http.StatusOK, bson.M{"message": "Vote added", "votes": votes})
}

func handleGetAnswers(w http.ResponseWriter, r *http.Request) {
	answers, err := mongoListAnswers(r.PathValue("question_id"))
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	writeJSON(w, http.StatusOK, answers)
}

func handleAdVote(w http.ResponseWriter, r *http.Request) {
	_, err := userIDFromToken(r)
	if err != nil {
		writeJSON(w, http.StatusUnauthorized, bson.M{"error": err.Error()})
		return
	}

	questions, err := mongoListQuestions()
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	if len(questions) == 0 {
		writeJSON(w, http.StatusNotFound, bson.M{"error": "No questions available. Create a question first."})
		return
	}
	questionID, _ := questions[0]["_id"].(string)

	answers, err := mongoListAnswers(questionID)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	if len(answers) == 0 {
		writeJSON(w, http.StatusNotFound, bson.M{"error": "No answers available for this question. Create an answer first."})
		return
	}
	answerID, _ := answers[0]["_id"].(string)

	votes, err := mongoVoteAnswer(answerID)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	writeJSON(w, http.StatusOK, bson.M{
		"question_id": questionID,
		"answer_id":   answerID,
		"message":     "Vote added",
		"votes":       votes,
	})
}

func main() {
	ctx := context.Background()

	clientOptions := options.Client().ApplyURI(getMongoURI())
	var err error
	client, err = mongo.Connect(ctx, clientOptions)
	if err != nil {
		fmt.Println("Failed to connect to MongoDB:", err)
		os.Exit(1)
	}
	defer client.Disconnect(ctx)

	db = client.Database(dbNameFromURI(getMongoURI()))

	mux := http.NewServeMux()

	mux.HandleFunc("POST /auth/login", handleLogin)
	mux.HandleFunc("DELETE /users/{user_id}", handleDeleteUser)
	mux.HandleFunc("POST /users", handleCreateUser)
	mux.HandleFunc("GET /users", handleListUsernames)

	mux.HandleFunc("DELETE /questions/{question_id}", handleDeleteQuestion)
	mux.HandleFunc("POST /questions", handleCreateQuestion)
	mux.HandleFunc("GET /questions", handleListQuestions)
	mux.HandleFunc("GET /questions/{question_id}", handleGetQuestion)
	mux.HandleFunc("GET /questions/{question_id}/answers", handleGetAnswers)

	mux.HandleFunc("POST /answers", handleCreateAnswer)
	mux.HandleFunc("POST /answers/{answer_id}/vote", handleVoteAnswer)

	mux.HandleFunc("POST /adVote", handleAdVote)

	mux.HandleFunc("GET /download/db", handleDownloadDB)

	addr := ":" + os.Getenv("PORT")
	if addr == ":" {
		addr = ":5000"
	}
	fmt.Printf("Server running on http://127.0.0.1%s\n", addr)
	http.ListenAndServe(addr, mux)
}

func dbNameFromURI(uri string) string {
	idx := strings.LastIndex(uri, "/")
	if idx == -1 || idx == len(uri)-1 {
		return "forum"
	}
	return uri[idx+1:]
}
