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

def get_question_and_answer():
    response = requests.get(f"{base_url}/questions", headers=headers)
    if response.status_code != 200:
        print(f"Failed to retrieve questions. Status code: {response.status_code}")
        return None, None
    questions = response.json()
    if not questions:
        print("No questions available. Create a question first.")
        return None, None

    question = questions[0]
    question_id = question["_id"]

    response = requests.get(f"{base_url}/questions/{question_id}/answers", headers=headers)
    if response.status_code != 200:
        print(f"Failed to retrieve answers. Status code: {response.status_code}")
        return None, None
    answers = response.json()
    if not answers:
        print("No answers available for this question. Create an answer first.")
        return None, None

    return question_id, answers[0]

token = login("john_doe", "hashed_password")
if not token:
    exit(1)

auth_headers = {**headers, "Authorization": f"Bearer {token}"}

question_id, answer = get_question_and_answer()
if question_id is None or answer is None:
    exit(1)

answer_id = answer["_id"]
print(f"Using question_id: {question_id}")
print(f"Using answer_id:   {answer_id}")

response = requests.post(f"{base_url}/answers/{answer_id}/vote", headers=auth_headers)

print(response.status_code)
print(response.json())
