# Automated Test Report - CaseGuard Backend API
Generated: 2026-01-15 11:33:41

## Summary
- Total Tests: 21
- Passed: 17
- Failed: 4
- Pass Rate: 80.95%

## Test Results

### PASS - Login as Admin
- Endpoint: POST /api/auth/login
- Status Code: 200
- Time: 2026-01-15 11:33:41

### FAIL - Get Claims - Success
- Endpoint: GET /api/Auth/claims
- Status Code: 401
- Time: 2026-01-15 11:33:41

### PASS - Get Claims - User Validation
- Endpoint: GET /api/Auth/claims
- Status Code: 401
- Time: 2026-01-15 11:33:41

### PASS - Login as Owner
- Endpoint: POST /api/auth/login
- Status Code: 200
- Time: 2026-01-15 11:33:41

### PASS - Login as Member
- Endpoint: POST /api/auth/login
- Status Code: 200
- Time: 2026-01-15 11:33:41

### PASS - Health Check
- Endpoint: GET /api/health
- Status Code: 200
- Time: 2026-01-15 11:33:41

### PASS - Create License
- Endpoint: POST /api/License
- Status Code: 201
- Time: 2026-01-15 11:33:41

### PASS - Get All Licenses
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-15 11:33:41

### PASS - Get License by ID
- Endpoint: GET /api/License/{id}
- Status Code: 200
- Time: 2026-01-15 11:33:41

### PASS - Update License
- Endpoint: PUT /api/License/{id}
- Status Code: 200
- Time: 2026-01-15 11:33:41

### PASS - Admin Only - Non-Admin Access
- Endpoint: POST /api/License
- Status Code: 403
- Time: 2026-01-15 11:33:41

### FAIL - Create Organization - Success
- Endpoint: POST /api/Organization
- Status Code: 400
- Time: 2026-01-15 11:33:41

### FAIL - Get All Organizations - Success
- Endpoint: GET /api/organization
- Status Code: 400
- Time: 2026-01-15 11:33:41

### PASS - Get All Organizations - User Validation
- Endpoint: GET /api/organization
- Status Code: 400
- Time: 2026-01-15 11:33:41

### FAIL - Get User Organizations - Success
- Endpoint: GET /api/user/organizations
- Status Code: 400
- Time: 2026-01-15 11:33:41

### PASS - Get User Organizations - User Validation
- Endpoint: GET /api/user/organizations
- Status Code: 400
- Time: 2026-01-15 11:33:41

### PASS - Pagination Support
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-15 11:33:41

### PASS - Filtering Support
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-15 11:33:41

### PASS - Sorting Support
- Endpoint: GET /api/License
- Status Code: 200
- Time: 2026-01-15 11:33:41

### PASS - Unauthorized Access
- Endpoint: GET /api/License
- Status Code: 401
- Time: 2026-01-15 11:33:41

### PASS - Resource Not Found
- Endpoint: GET /api/License/{id}
- Status Code: 404
- Time: 2026-01-15 11:33:41


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

