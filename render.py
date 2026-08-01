from flask import Flask, request, jsonify, abort
from flask_pymongo import PyMongo
from config import Config
from bson.objectid import ObjectId
from datetime import datetime
import bcrypt
import os

app = Flask(__name__)
app.config.from_object(Config)
mongo = PyMongo(app)

def bcrypt_password(password):
    return bcrypt.hashpw(password.encode('utf-8'), bcrypt.gensalt()).decode('utf-8')

# Helper function to convert ObjectId to string
def convert_objectid(data):
    if isinstance(data, dict):
        for key, value in data.items():
            if isinstance(value, ObjectId):
                data[key] = str(value)
            elif isinstance(value, dict):
                data[key] = convert_objectid(value)
            elif isinstance(value, list):
                data[key] = [convert_objectid(item) if isinstance(item, (dict, ObjectId)) else item for item in value]
    elif isinstance(data, ObjectId):
        return str(data)
    return data

@app.route('/users/<user_id>', methods=['DELETE'])
def delete_user(user_id):
    result = mongo.db.users.delete_one({"_id": ObjectId(user_id)})
    if result.deleted_count == 0:
        return "User not found", 404
    else:
        return jsonify({"message": "User deleted successfully"}), 200

@app.route('/users', methods=['POST'])
def create_user():
    data = request.json
    user = {
        "username": data["username"],
        "email": data["email"],
        "password": bcrypt_password(data["password"]),  # Note: Hash the password before storing in a real application
        "reputation": 0,
        "created_at": datetime.utcnow(),
        "profile": data.get("profile", {}),
        "badges": []
    }
    result = mongo.db.users.insert_one(user)
    user["_id"] = str(result.inserted_id)
    return jsonify(user), 201

@app.route('/users', methods=['GET'])
def list_usernames():
    users = mongo.db.users.find({}, {"_id": 0, "username": 1})
    usernames = [user["username"] for user in users]
    return jsonify(usernames), 200


@app.route('/questions/<question_id>', methods=['DELETE'])
def delete_question(question_id):
    result = mongo.db.questions.delete_one({"_id": ObjectId(question_id)})
    if result.deleted_count == 0:
        return "Question not found", 404
    else:
        return jsonify({"message": "Question deleted successfully"}), 200

@app.route('/questions', methods=['POST'])
def create_question():
    data = request.json

    user_id = ObjectId(data["user_id"])
    user = mongo.db.users.find_one({"_id": user_id})
    if not user:
        return jsonify({"error": "User not found"}), 404

    question = {
        "title": data["title"],
        "body": data["body"],
        "tags": data["tags"],
        "user_id": str(user_id),
        "created_at": datetime.utcnow(),
        "updated_at": datetime.utcnow(),
        "views": 0,
        "votes": 0,
        "answers": []
    }
    result = mongo.db.questions.insert_one(question)
    question["_id"] = str(result.inserted_id)
    return jsonify(question), 201

@app.route('/questions', methods=['GET'])
def list_questions():
    questions = mongo.db.questions.find()
    question_list = [convert_objectid(question) for question in questions]
    return jsonify(question_list), 200

@app.route('/questions/<question_id>', methods=['GET'])
def get_question(question_id):
    question = mongo.db.questions.find_one_or_404({"_id": ObjectId(question_id)})
    return jsonify(convert_objectid(question)), 200

@app.route('/answers', methods=['POST'])
def create_answer():
    data = request.json

    user_id = data["user_id"]
    user = mongo.db.questions.find_one({"user_id": user_id})
    if not user:
        return jsonify({"error": "User not found"}), 404
    
    # Check if question_id exists
    question_id = ObjectId(data["question_id"])
    question = mongo.db.questions.find_one({"_id": question_id})
    if not question:
        return jsonify({"error": "Question not found"}), 404

    answer = {
        "question_id": str(question_id),
        "user_id": str(user_id),
        "body": data["body"],
        "created_at": datetime.utcnow(),
        "updated_at": datetime.utcnow(),
        "votes": 0,
        "comments": []
    }
    result = mongo.db.answers.insert_one(answer)
    answer["_id"] = str(result.inserted_id)
    return jsonify(answer), 201

@app.route('/questions/<question_id>/answers', methods=['GET'])
def get_answers(question_id):
    answers = mongo.db.answers.find({"question_id": str(ObjectId(question_id))})
    return jsonify([convert_objectid(answer) for answer in answers]), 200

if __name__ == '__main__':
    app.run(debug=True)