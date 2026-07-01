# Demo Step 1: Login
# Verifies: valid credentials return 200 with a session token
# The token is captured and passed automatically to subsequent Hurl files

POST {{auth_host}}/v1/auth/login
Content-Type: application/json
{
    "email": "{{test_email}}",
    "password": "{{test_password}}"
}

HTTP 200
[Asserts]
jsonpath "$.success" == true
jsonpath "$.data.token" isString
jsonpath "$.data.expiresAt" isString

[Captures]
token: jsonpath "$.data.token"
