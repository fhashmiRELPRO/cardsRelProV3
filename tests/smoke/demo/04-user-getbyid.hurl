# Demo Step 4: GET /v1/user/{id}
# Verifies: UserManagement entitlement gate, BaseMySqlRepository → rcp2_read_user stored proc, ApiResponse<T> shape

GET {{user_host}}/v1/user/{{test_user_id}}
userToken: {{token}}

HTTP 200
[Asserts]
jsonpath "$.success" == true
jsonpath "$.data.userId" isInteger
jsonpath "$.data.name" isString
jsonpath "$.data.email" isString
jsonpath "$.data.orgId" isInteger
