import requests

# Define the URL of the Flask endpoint
url = "http://localhost:5000/questions"

# Make the GET request to the Flask endpoint
response = requests.get(url)

# Check if the request was successful
if response.status_code == 200:
    # Parse the JSON response
    questions = response.json()
    # Print the list of questions
    print(questions)
else:
    # Print an error message if the request failed
    print(f"Failed to retrieve questions. Status code: {response.status_code}")