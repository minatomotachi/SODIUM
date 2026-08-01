import requests
import json

base_url = "http://127.0.0.1:5000"
headers = {
    "Content-Type": "application/json"
}

def login(username, password):
    response = requests.post(f"{base_url}/auth/login", headers=headers,
                             data=json.dumps({"username": username, "password": password}))
    print(f"Login status: {response.status_code}")
    if response.status_code != 200:
        print(response.json())
        return None
    return response.json()["token"]

def get_real_question():
    response = requests.get(f"{base_url}/questions", headers=headers)
    if response.status_code != 200:
        print(f"Failed to retrieve questions. Status code: {response.status_code}")
        return None
    questions = response.json()
    if not questions:
        print("No questions available. Create a question first.")
        return None
    return questions[0]

token = login("john_doe", "hashed_password")
if not token:
    exit(1)

auth_headers = {**headers, "Authorization": f"Bearer {token}"}

question = get_real_question()
if question is None:
    exit(1)

question_id = question["_id"]
print(f"Using question_id: {question_id}")

data = {
    "question_id": question_id,
    "body": "You can use Passport.js for authentication in Node.js."
}

response = requests.post(f"{base_url}/answers", headers=auth_headers, data=json.dumps(data))

print(response.status_code)
print(response.json())
