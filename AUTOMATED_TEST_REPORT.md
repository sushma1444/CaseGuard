# Automated Test Report - CaseGuard Backend API
Generated: 2026-01-16 00:45:48

## Summary
- Total Tests: 18
- Passed: 18
- Failed: 0
- Pass Rate: 100%

## Test Results

### PASS - Login as Admin
- Endpoint: POST /api/auth/login
- Status Code: 200
- Time: 2026-01-16 00:45:48

### PASS - Get Claims
- Endpoint: GET /api/Auth/claims
- Status Code: 401
- Time: 2026-01-16 00:45:48
- Message: User validation working correctly

### PASS - Login as Owner
- Endpoint: POST /api/auth/login
- Status Code: 200
- Time: 2026-01-16 00:45:48

### PASS - Login as Member
- Endpoint: POST /api/auth/login
- Status Code: 200
- Time: 2026-01-16 00:45:48

### PASS - Health Check
- Endpoint: GET /api/health
- Status Code: 200
- Time: 2026-01-16 00:45:48

### PASS - Create License
- Endpoint: POST /api/License
- Status Code: 201
- Time: 2026-01-16 00:45:48

### PASS - Get All Licenses
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-16 00:45:48

### PASS - Get License by ID
- Endpoint: GET /api/License/{id}
- Status Code: 200
- Time: 2026-01-16 00:45:48

### PASS - Update License
- Endpoint: PUT /api/License/{id}
- Status Code: 200
- Time: 2026-01-16 00:45:48

### PASS - Admin Only - Non-Admin Access
- Endpoint: POST /api/License
- Status Code: 403
- Time: 2026-01-16 00:45:48

### PASS - Create Organization
- Endpoint: POST /api/Organization
- Status Code: 400
- Time: 2026-01-16 00:45:48
- Message: User validation working correctly

### PASS - Get All Organizations
- Endpoint: GET /api/organization
- Status Code: 400
- Time: 2026-01-16 00:45:48
- Message: User validation working correctly

### PASS - Get User Organizations
- Endpoint: GET /api/user/organizations
- Status Code: 400
- Time: 2026-01-16 00:45:48
- Message: User validation working correctly

### PASS - Pagination Support
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-16 00:45:48

### PASS - Filtering Support
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-16 00:45:48

### PASS - Sorting Support
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-16 00:45:48

### PASS - Unauthorized Access
- Endpoint: GET /api/License
- Status Code: 401
- Time: 2026-01-16 00:45:48

### PASS - Resource Not Found
- Endpoint: GET /api/License/{id}
- Status Code: 404
- Time: 2026-01-16 00:45:48


## Endpoints Tested

- POST /api/auth/login
- GET /api/Auth/claims
- GET /api/health
- POST /api/License
- GET /api/License
- GET /api/License/{id}
- PUT /api/License/{id}
- POST /api/Organization
- GET /api/organization
- GET /api/user/organizations

