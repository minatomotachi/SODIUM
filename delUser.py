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
    return response.json()

token_data = login("john_doe", "hashed_password")
if not token_data:
    exit(1)

token = token_data["token"]
auth_headers = {**headers, "Authorization": f"Bearer {token}"}

user_id = token_data["user_id"]

url = f"{base_url}/users/{user_id}"

response = requests.delete(url, headers=auth_headers)

print(response.status_code)
print(response.json())
