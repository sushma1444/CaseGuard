# Automated Test Report - CaseGuard Backend API
Generated: 2026-01-14 15:04:30

## Summary
- Total Tests: 18
- Passed: 14
- Failed: 4
- Pass Rate: 77.78%

## Test Results

### PASS - Login as Admin
- Endpoint: POST /api/auth/login
- Status Code: 200
- Time: 2026-01-14 15:04:29

### FAIL - Get Claims
- Endpoint: GET /api/Auth/claims
- Status Code: 401
- Time: 2026-01-14 15:04:29

### PASS - Login as Owner
- Endpoint: POST /api/auth/login
- Status Code: 200
- Time: 2026-01-14 15:04:29

### PASS - Login as Member
- Endpoint: POST /api/auth/login
- Status Code: 200
- Time: 2026-01-14 15:04:29

### PASS - Health Check
- Endpoint: GET /api/health
- Status Code: 200
- Time: 2026-01-14 15:04:29

### PASS - Create License
- Endpoint: POST /api/License
- Status Code: 201
- Time: 2026-01-14 15:04:30

### PASS - Get All Licenses
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-14 15:04:30

### PASS - Get License by ID
- Endpoint: GET /api/License/{id}
- Status Code: 200
- Time: 2026-01-14 15:04:30

### PASS - Update License
- Endpoint: PUT /api/License/{id}
- Status Code: 200
- Time: 2026-01-14 15:04:30

### PASS - Admin Only - Non-Admin Access
- Endpoint: POST /api/License
- Status Code: 403
- Time: 2026-01-14 15:04:30

### FAIL - Create Organization
- Endpoint: POST /api/Organization
- Status Code: 400
- Time: 2026-01-14 15:04:30

### FAIL - Get All Organizations
- Endpoint: GET /api/organization
- Status Code: 400
- Time: 2026-01-14 15:04:30

### FAIL - Get User Organizations
- Endpoint: GET /api/user/organizations
- Status Code: 400
- Time: 2026-01-14 15:04:30

### PASS - Pagination Support
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-14 15:04:30

### PASS - Filtering Support
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-14 15:04:30

### PASS - Sorting Support
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-14 15:04:30

### PASS - Unauthorized Access
- Endpoint: GET /api/License
- Status Code: 401
- Time: 2026-01-14 15:04:30

### PASS - Resource Not Found
- Endpoint: GET /api/License/{id}
- Status Code: 404
- Time: 2026-01-14 15:04:30


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

