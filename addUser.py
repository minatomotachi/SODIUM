import requests
import json

url = "http://127.0.0.1:5000/users"
data = {
    "username": "john_doe",
    "email": "john@example.com",
    "password": "hashed_password",  # In a real application, make sure to hash the password
    "profile": {
        "bio": "Software developer with 10 years of experience.",
        "website": "https://johndoe.dev",
        "location": "San Francisco, CA"
    }
}

headers = {
    "Content-Type": "application/json"
}

response = requests.post(url, headers=headers, data=json.dumps(data))

print(response.status_code)
print(response.json())