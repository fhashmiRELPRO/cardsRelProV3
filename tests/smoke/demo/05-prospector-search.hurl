# Demo Step 5: GET /v1/search  (wire-compatible with CARDS GET /prospector/v1/search)
# Verifies: per-tenant MongoDB config, Atlas Search pipeline, CARDS-compatible response shape
# Frontend can point here without any response-format changes.

GET {{search_host}}/v1/search?query={{search_query}}&pageSize=10&page=1&showPastRole=true&includeInActiveCompanies=true
userToken: {{token}}

HTTP 200
[Asserts]
jsonpath "$.totalIndividuals" isInteger
jsonpath "$.individuals" isCollection
jsonpath "$.searchId" isInteger
