# Automated Test Report - CaseGuard Backend API
Generated: 2026-01-14 01:07:53

## Summary
- **Total Tests**: 17
- **Passed**: 13
- **Failed**: 4
- **Pass Rate**: 76.47%

## Test Results
### âœ… PASS - Login as Admin
- **Endpoint**: POST /api/auth/login
- **Status Code**: 200
- **Time**: 2026-01-14 01:07:52

### âŒ FAIL - Get Claims
- **Endpoint**: GET /api/auth/claims
- **Status Code**: 401
- **Time**: 2026-01-14 01:07:52

### âœ… PASS - Login as Owner
- **Endpoint**: POST /api/auth/login
- **Status Code**: 200
- **Time**: 2026-01-14 01:07:52

### âœ… PASS - Login as Member
- **Endpoint**: POST /api/auth/login
- **Status Code**: 200
- **Time**: 2026-01-14 01:07:52

### âœ… PASS - Health Check
- **Endpoint**: GET /api/health
- **Status Code**: 200
- **Time**: 2026-01-14 01:07:52

### âŒ FAIL - Create License
- **Endpoint**: POST /api/License
- **Status Code**: 404
- **Time**: 2026-01-14 01:07:53

### âœ… PASS - Get All Licenses
- **Endpoint**: GET /api/License
- **Status Code**: 200
- **Time**: 2026-01-14 01:07:53

### âŒ FAIL - Get License by ID
- **Endpoint**: GET /api/License/{id}
- **Status Code**: 0
- **Time**: 2026-01-14 01:07:53
- **Message**: Skipped - no license ID
### âœ… PASS - Admin Only - Non-Admin Access
- **Endpoint**: POST /api/License
- **Status Code**: 403
- **Time**: 2026-01-14 01:07:53

### âŒ FAIL - Create Organization
- **Endpoint**: POST /api/Organization
- **Status Code**: 400
- **Time**: 2026-01-14 01:07:53

### âœ… PASS - Get All Organizations
- **Endpoint**: GET /api/organization
- **Status Code**: 200
- **Time**: 2026-01-14 01:07:53

### âœ… PASS - Get User Organizations
- **Endpoint**: GET /api/user/organizations
- **Status Code**: 200
- **Time**: 2026-01-14 01:07:53

### âœ… PASS - Pagination Support
- **Endpoint**: GET /api/License
- **Status Code**: 200
- **Time**: 2026-01-14 01:07:53

### âœ… PASS - Filtering Support
- **Endpoint**: GET /api/License
- **Status Code**: 200
- **Time**: 2026-01-14 01:07:53

### âœ… PASS - Sorting Support
- **Endpoint**: GET /api/License
- **Status Code**: 200
- **Time**: 2026-01-14 01:07:53

### âœ… PASS - Unauthorized Access
- **Endpoint**: GET /api/License
- **Status Code**: 401
- **Time**: 2026-01-14 01:07:53

### âœ… PASS - Resource Not Found
- **Endpoint**: GET /api/License/{id}
- **Status Code**: 404
- **Time**: 2026-01-14 01:07:53


## Endpoints Tested- POST /api/auth/login
- GET /api/auth/claims
- GET /api/health
- POST /api/License
- GET /api/License
- GET /api/License/{id}
- POST /api/Organization
- GET /api/organization
- GET /api/user/organizations

