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

token = login("john_doe", "hashed_password")
if not token:
    exit(1)

auth_headers = {**headers, "Authorization": f"Bearer {token}"}

data = {
    "title": "How to implement authentication in Node.js?",
    "body": "I am trying to implement user authentication in a Node.js application. What are the best practices?",
    "tags": ["node.js", "authentication", "best-practices"]
}

response = requests.post(f"{base_url}/questions", headers=auth_headers, data=json.dumps(data))

print(response.status_code)
print(response.json())
