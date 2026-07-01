# Smoke test: login happy path
# Verifies: valid credentials return 200 with tokens

POST {{host}}/auth/login
Content-Type: application/json
{
    "email": "{{test_email}}",
    "password": "{{test_password}}"
}

HTTP 200
[Asserts]
jsonpath "$.success" == true
jsonpath "$.data.accessToken" isString
jsonpath "$.data.refreshToken" isString
