package main

import (
	"context"
	"time"

	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/bson/primitive"
	"go.mongodb.org/mongo-driver/mongo/options"
)

const (
	questionsCol = "questions"
	answersCol   = "answers"
	usersCol     = "users"
)

func newID() string {
	return primitive.NewObjectID().Hex()
}

func nowStr() string {
	return time.Now().UTC().Format(time.RFC3339Nano)
}

// ---------- Questions ----------

func mongoInsertQuestion(title, body string, tags interface{}, userID string) (string, error) {
	id := newID()
	question := bson.M{
		"_id":        id,
		"title":      title,
		"body":       body,
		"tags":       tags,
		"user_id":    userID,
		"created_at": nowStr(),
		"updated_at": nowStr(),
		"views":      0,
		"votes":      0,
		"answers":    []interface{}{},
	}
	_, err := db.Collection(questionsCol).InsertOne(context.Background(), question)
	if err != nil {
		return "", err
	}
	return id, nil
}

func mongoGetQuestion(id string) (bson.M, error) {
	var q bson.M
	err := db.Collection(questionsCol).FindOne(context.Background(), bson.M{"_id": id}).Decode(&q)
	if err != nil {
		return nil, err
	}
	return q, nil
}

func mongoListQuestions() ([]bson.M, error) {
	cursor, err := db.Collection(questionsCol).Find(
		context.Background(),
		bson.M{},
		options.Find().SetSort(bson.D{{Key: "created_at", Value: -1}}),
	)
	if err != nil {
		return nil, err
	}
	defer cursor.Close(context.Background())

	var questions []bson.M
	if err := cursor.All(context.Background(), &questions); err != nil {
		return nil, err
	}
	return questions, nil
}

func mongoDeleteQuestion(id string) (bool, error) {
	res, err := db.Collection(questionsCol).DeleteOne(context.Background(), bson.M{"_id": id})
	if err != nil {
		return false, err
	}
	return res.DeletedCount > 0, nil
}

// ---------- Answers ----------

func mongoInsertAnswer(questionID, userID, body string) (string, error) {
	id := newID()
	answer := bson.M{
		"_id":         id,
		"question_id": questionID,
		"user_id":     userID,
		"body":        body,
		"created_at":  nowStr(),
		"updated_at":  nowStr(),
		"votes":       0,
		"comments":    []interface{}{},
	}
	_, err := db.Collection(answersCol).InsertOne(context.Background(), answer)
	if err != nil {
		return "", err
	}
	return id, nil
}

func mongoGetAnswer(id string) (bson.M, error) {
	var a bson.M
	err := db.Collection(answersCol).FindOne(context.Background(), bson.M{"_id": id}).Decode(&a)
	if err != nil {
		return nil, err
	}
	return a, nil
}

func mongoListAnswers(questionID string) ([]bson.M, error) {
	cursor, err := db.Collection(answersCol).Find(
		context.Background(),
		bson.M{"question_id": questionID},
		options.Find().SetSort(bson.D{{Key: "created_at", Value: -1}}),
	)
	if err != nil {
		return nil, err
	}
	defer cursor.Close(context.Background())

	var answers []bson.M
	if err := cursor.All(context.Background(), &answers); err != nil {
		return nil, err
	}
	return answers, nil
}

func mongoListAllAnswers() ([]bson.M, error) {
	cursor, err := db.Collection(answersCol).Find(
		context.Background(),
		bson.M{},
		options.Find().SetSort(bson.D{{Key: "created_at", Value: -1}}),
	)
	if err != nil {
		return nil, err
	}
	defer cursor.Close(context.Background())

	var answers []bson.M
	if err := cursor.All(context.Background(), &answers); err != nil {
		return nil, err
	}
	return answers, nil
}

func mongoVoteAnswer(id string) (int64, error) {
	var a bson.M
	err := db.Collection(answersCol).FindOneAndUpdate(
		context.Background(),
		bson.M{"_id": id},
		bson.M{"$inc": bson.M{"votes": 1}},
		options.FindOneAndUpdate().SetReturnDocument(options.After),
	).Decode(&a)
	if err != nil {
		return 0, err
	}

	votes := int64(0)
	switch v := a["votes"].(type) {
	case int64:
		votes = v
	case int32:
		votes = int64(v)
	}
	return votes, nil
}
