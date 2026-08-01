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

def get_my_question(token):
    auth_headers = {**headers, "Authorization": f"Bearer {token}"}
    response = requests.get(f"{base_url}/questions", headers=auth_headers)
    if response.status_code != 200:
        print(f"Failed to retrieve questions. Status code: {response.status_code}")
        return None
    questions = response.json()
    if not questions:
        print("No questions available.")
        return None
    return questions[0]

token = login("john_doe", "hashed_password")
if not token:
    exit(1)

auth_headers = {**headers, "Authorization": f"Bearer {token}"}

question = get_my_question(token)
if question is None:
    exit(1)

question_id = question["_id"]
print(f"Using question_id: {question_id}")

url = f"{base_url}/questions/{question_id}"

response = requests.delete(url, headers=auth_headers)

print(response.status_code)
print(response.json())
