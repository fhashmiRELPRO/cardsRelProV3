# Demo Step 2: Get current user profile
# Requires: token captured from 01-login.hurl
# Verifies: session token is valid and returns user profile

GET {{user_host}}/v1/user/me
userToken: {{token}}

HTTP 200
[Asserts]
jsonpath "$.success" == true
jsonpath "$.data.userId" isInteger
jsonpath "$.data.email" isString
jsonpath "$.data.displayName" isString
jsonpath "$.data.isContractActive" == true
jsonpath "$.data.contractStatus" == "Active"
