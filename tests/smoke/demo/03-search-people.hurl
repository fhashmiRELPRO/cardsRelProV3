# Demo Step 3: Search for people
# Requires: token captured from 01-login.hurl
# Verifies: authenticated search returns results from real DB

GET {{search_host}}/v1/search/people?q={{search_query}}&limit=10
userToken: {{token}}

HTTP 200
[Asserts]
jsonpath "$.success" == true
jsonpath "$.data" isCollection

# --- Bonus: unauthenticated search returns 401 ---

GET {{search_host}}/v1/search/people?q={{search_query}}

HTTP 401
[Asserts]
jsonpath "$.success" == false
jsonpath "$.errorCode" == "MISSING_TOKEN"
