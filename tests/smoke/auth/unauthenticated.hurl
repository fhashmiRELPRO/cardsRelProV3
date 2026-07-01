# Smoke test: no token returns 401
# Verifies: missing userToken/Bearer returns correct error

GET {{host}}/v3/user/me

HTTP 401
[Asserts]
jsonpath "$.success" == false
jsonpath "$.errorCode" == "MISSING_TOKEN"
