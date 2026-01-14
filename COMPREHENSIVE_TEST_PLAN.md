# Comprehensive Test Plan - Task 17

## Overview
This document outlines the complete testing strategy for the Organization and License Management System API.

## Test Objectives

1. ✅ Verify all endpoints work end-to-end
2. ✅ Test authorization and role-based access control
3. ✅ Test license expiration logic
4. ✅ Test auto-renewal background service
5. ✅ Test pagination, filtering, and sorting
6. ✅ Test edge cases and error handling

---

## Test Environment Setup

### Prerequisites
- Application running on `http://localhost:5000`
- PostgreSQL database connected
- Swagger UI accessible at `http://localhost:5000`

### Test Data Setup
Use the provided `test_data_setup.sql` or create test data via API endpoints.

---

## Test Scenarios

### 1. Authentication Tests

#### Test 1.1: Login as Admin
- **Endpoint**: `POST /api/auth/login`
- **Request**:
  ```json
  {
    "userId": "11111111-1111-1111-1111-111111111111",
    "email": "admin@example.com",
    "role": "Admin"
  }
  ```
- **Expected**: Returns JWT token
- **Status**: ✅

#### Test 1.2: Get Claims
- **Endpoint**: `GET /api/auth/claims`
- **Headers**: `Authorization: Bearer {token}`
- **Expected**: Returns user claims (userId, email, role)
- **Status**: ✅

---

### 2. Admin Endpoints (LicenseController)

#### Test 2.1: Create License
- **Endpoint**: `POST /api/license`
- **Role**: Admin
- **Request**:
  ```json
  {
    "organizationId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "name": "Premium License",
    "startDate": "2026-01-14T00:00:00Z",
    "expirationDate": "2026-01-14T00:10:00Z",
    "autoRenewalEnabled": true
  }
  ```
- **Expected**: 201 Created, returns license details
- **Status**: ⏳ To Test

#### Test 2.2: Get All Licenses
- **Endpoint**: `GET /api/license?page=1&pageSize=10`
- **Role**: Admin
- **Expected**: 200 OK, paginated list of licenses
- **Status**: ✅ Tested

#### Test 2.3: Get License by ID
- **Endpoint**: `GET /api/license/{id}`
- **Role**: Admin
- **Expected**: 200 OK, license details
- **Status**: ⏳ To Test

#### Test 2.4: Update License
- **Endpoint**: `PUT /api/license/{id}`
- **Role**: Admin
- **Request**:
  ```json
  {
    "name": "Updated License Name",
    "autoRenewalEnabled": false
  }
  ```
- **Expected**: 200 OK, updated license
- **Status**: ⏳ To Test

#### Test 2.5: Cancel License
- **Endpoint**: `DELETE /api/license/{id}`
- **Role**: Admin
- **Expected**: 204 No Content
- **Status**: ⏳ To Test

#### Test 2.6: Check Expiration (Manual)
- **Endpoint**: `POST /api/license/check-expiration`
- **Role**: Admin
- **Expected**: 200 OK, returns count of invalidated licenses
- **Status**: ⏳ To Test

#### Test 2.7: Authorization Test - Non-Admin
- **Endpoint**: `POST /api/license`
- **Role**: Member (non-admin)
- **Expected**: 403 Forbidden
- **Status**: ⏳ To Test

---

### 3. Organization Endpoints (OrganizationController)

#### Test 3.1: Create Organization
- **Endpoint**: `POST /api/organization`
- **Role**: Any authenticated user
- **Request**:
  ```json
  {
    "name": "Test Organization",
    "description": "Test Description"
  }
  ```
- **Expected**: 201 Created, returns organization with user as Owner
- **Status**: ⏳ To Test

#### Test 3.2: Get All Organizations
- **Endpoint**: `GET /api/organization?page=1&pageSize=10`
- **Role**: Any authenticated user
- **Expected**: 200 OK, paginated list (user sees only their organizations unless Admin)
- **Status**: ✅ Tested

#### Test 3.3: Get Organization by ID
- **Endpoint**: `GET /api/organization/{id}`
- **Role**: Member of organization or Admin
- **Expected**: 200 OK, organization details
- **Status**: ⏳ To Test

#### Test 3.4: Update Organization
- **Endpoint**: `PUT /api/organization/{id}`
- **Role**: Owner, OrganizationAdmin, or Admin
- **Request**:
  ```json
  {
    "name": "Updated Organization Name",
    "description": "Updated Description"
  }
  ```
- **Expected**: 200 OK, updated organization
- **Status**: ⏳ To Test

#### Test 3.5: Delete Organization
- **Endpoint**: `DELETE /api/organization/{id}`
- **Role**: Owner or Admin
- **Expected**: 204 No Content
- **Status**: ⏳ To Test

#### Test 3.6: Authorization Test - Non-Member
- **Endpoint**: `GET /api/organization/{id}` (other user's org)
- **Role**: Member (not part of organization)
- **Expected**: 403 Forbidden or 404 Not Found
- **Status**: ⏳ To Test

---

### 4. Member Endpoints (MemberController)

#### Test 4.1: Invite Member
- **Endpoint**: `POST /api/member/{orgId}/invite`
- **Role**: Owner or OrganizationAdmin
- **Request**:
  ```json
  {
    "email": "newuser@example.com",
    "role": "Member"
  }
  ```
- **Expected**: 201 Created, returns invitation details
- **Status**: ⏳ To Test

#### Test 4.2: Get All Members
- **Endpoint**: `GET /api/member/{orgId}?page=1&pageSize=10`
- **Role**: Member of organization or Admin
- **Expected**: 200 OK, paginated list of members
- **Status**: ⏳ To Test

#### Test 4.3: Get Member by ID
- **Endpoint**: `GET /api/member/{orgId}/{memberId}`
- **Role**: Member of organization or Admin
- **Expected**: 200 OK, member details
- **Status**: ⏳ To Test

#### Test 4.4: Update Member Role
- **Endpoint**: `PUT /api/member/{orgId}/{memberId}/role`
- **Role**: Owner or OrganizationAdmin
- **Request**:
  ```json
  {
    "role": "OrganizationAdmin"
  }
  ```
- **Expected**: 200 OK, updated member
- **Status**: ⏳ To Test

#### Test 4.5: Remove Member
- **Endpoint**: `DELETE /api/member/{orgId}/{memberId}`
- **Role**: Owner or OrganizationAdmin
- **Expected**: 204 No Content
- **Status**: ⏳ To Test

---

### 5. Invitation Endpoints (InvitationController)

#### Test 5.1: Get All Invitations
- **Endpoint**: `GET /api/invitation/{orgId}?page=1&pageSize=10`
- **Role**: Owner or OrganizationAdmin
- **Expected**: 200 OK, paginated list of invitations
- **Status**: ⏳ To Test

#### Test 5.2: Get Invitation by ID
- **Endpoint**: `GET /api/invitation/{orgId}/{invitationId}`
- **Role**: Owner or OrganizationAdmin
- **Expected**: 200 OK, invitation details
- **Status**: ⏳ To Test

#### Test 5.3: Cancel Invitation
- **Endpoint**: `DELETE /api/invitation/{orgId}/{invitationId}`
- **Role**: Owner or OrganizationAdmin
- **Expected**: 204 No Content
- **Status**: ⏳ To Test

---

### 6. License Assignment Endpoints (LicenseAssignmentController)

#### Test 6.1: Assign License to User
- **Endpoint**: `POST /api/licenseassignment`
- **Role**: Owner or OrganizationAdmin
- **Request**:
  ```json
  {
    "licenseId": "{license_id}",
    "userId": "{user_id}"
  }
  ```
- **Expected**: 201 Created, assignment details
- **Status**: ⏳ To Test

#### Test 6.2: Get All Assignments
- **Endpoint**: `GET /api/licenseassignment?page=1&pageSize=10`
- **Role**: Admin (sees all) or Member (sees their org's)
- **Expected**: 200 OK, paginated list
- **Status**: ✅ Tested

#### Test 6.3: Get Assignment by ID
- **Endpoint**: `GET /api/licenseassignment/{id}`
- **Role**: Admin or Member of organization
- **Expected**: 200 OK, assignment details
- **Status**: ⏳ To Test

#### Test 6.4: Unassign License
- **Endpoint**: `DELETE /api/licenseassignment/{id}`
- **Role**: Owner or OrganizationAdmin
- **Expected**: 204 No Content
- **Status**: ⏳ To Test

#### Test 6.5: Assign Expired License (Should Fail)
- **Endpoint**: `POST /api/licenseassignment`
- **Role**: Owner or OrganizationAdmin
- **Request**: Use expired license ID
- **Expected**: 400 Bad Request, "Cannot assign an invalid or expired license"
- **Status**: ⏳ To Test

---

### 7. User Endpoints (UserController)

#### Test 7.1: Get User Organizations
- **Endpoint**: `GET /api/user/organizations?page=1&pageSize=10`
- **Role**: Any authenticated user
- **Expected**: 200 OK, paginated list of user's organizations
- **Status**: ✅ Tested

#### Test 7.2: Get User Organization by ID
- **Endpoint**: `GET /api/user/organizations/{organizationId}`
- **Role**: Member of organization
- **Expected**: 200 OK, organization details
- **Status**: ⏳ To Test

#### Test 7.3: Accept Invitation
- **Endpoint**: `POST /api/user/invitations/accept`
- **Role**: Any authenticated user
- **Request**:
  ```json
  {
    "invitationId": "{invitation_id}"
  }
  ```
- **Expected**: 200 OK, organization details
- **Status**: ⏳ To Test

#### Test 7.4: Leave Organization
- **Endpoint**: `DELETE /api/user/organizations/{organizationId}`
- **Role**: Member of organization
- **Expected**: 204 No Content
- **Status**: ⏳ To Test

---

### 8. License Expiration Tests

#### Test 8.1: Create License with Short Expiration
- **Endpoint**: `POST /api/license`
- **Request**: Set expiration to 1 minute from now
- **Expected**: License created successfully
- **Status**: ⏳ To Test

#### Test 8.2: Wait for Expiration
- **Action**: Wait 1+ minute
- **Expected**: License should expire

#### Test 8.3: Check Expiration Manually
- **Endpoint**: `POST /api/license/check-expiration`
- **Expected**: Invalidates expired license, returns count
- **Status**: ⏳ To Test

#### Test 8.4: Verify Expired License is Inactive
- **Endpoint**: `GET /api/license/{id}`
- **Expected**: `isActive: false`, `isValid: false`
- **Status**: ⏳ To Test

#### Test 8.5: Automatic Expiration Check
- **Action**: Call `GET /api/license` (triggers automatic check)
- **Expected**: Expired licenses automatically invalidated
- **Status**: ⏳ To Test

---

### 9. Auto-Renewal Tests

#### Test 9.1: Create License with Auto-Renewal
- **Endpoint**: `POST /api/license`
- **Request**: `autoRenewalEnabled: true`, expiration in 5 days
- **Expected**: License created with auto-renewal enabled
- **Status**: ⏳ To Test

#### Test 9.2: Verify Background Service Running
- **Action**: Check application logs
- **Expected**: "License Renewal Background Service started"
- **Status**: ⏳ To Test

#### Test 9.3: Wait for Renewal Window
- **Action**: Set expiration to 6 days (within 7-day renewal window)
- **Expected**: License should be renewed by background service
- **Status**: ⏳ To Test (requires waiting or manual trigger)

#### Test 9.4: Verify License Renewed
- **Endpoint**: `GET /api/license/{id}`
- **Expected**: Expiration date extended by original duration
- **Status**: ⏳ To Test

---

### 10. Pagination, Filtering, and Sorting Tests

#### Test 10.1: Pagination
- **Endpoint**: `GET /api/license?page=1&pageSize=5`
- **Expected**: Returns 5 items, correct pagination metadata
- **Status**: ⏳ To Test

#### Test 10.2: Filtering
- **Endpoint**: `GET /api/license?isActive=true&expirationStatus=active`
- **Expected**: Returns only active, non-expired licenses
- **Status**: ⏳ To Test

#### Test 10.3: Sorting
- **Endpoint**: `GET /api/license?sortBy=expirationdate&sortDirection=asc`
- **Expected**: Licenses sorted by expiration date ascending
- **Status**: ⏳ To Test

#### Test 10.4: Search Term
- **Endpoint**: `GET /api/license?searchTerm=Premium`
- **Expected**: Returns licenses matching "Premium" in name or organization name
- **Status**: ⏳ To Test

---

### 11. Error Handling Tests

#### Test 11.1: Invalid Request Body
- **Endpoint**: `POST /api/license`
- **Request**: Missing required fields
- **Expected**: 400 Bad Request with validation errors
- **Status**: ⏳ To Test

#### Test 11.2: Resource Not Found
- **Endpoint**: `GET /api/license/{invalid_id}`
- **Expected**: 404 Not Found
- **Status**: ⏳ To Test

#### Test 11.3: Unauthorized Access
- **Endpoint**: `GET /api/license` (without token)
- **Expected**: 401 Unauthorized
- **Status**: ⏳ To Test

#### Test 11.4: Forbidden Access
- **Endpoint**: `POST /api/license` (as non-admin)
- **Expected**: 403 Forbidden
- **Status**: ⏳ To Test

---

## Test Execution Checklist

### Phase 1: Basic Functionality
- [ ] Authentication endpoints
- [ ] Health check endpoint
- [ ] All GET endpoints (list and by ID)
- [ ] All POST endpoints (create operations)

### Phase 2: Update and Delete
- [ ] All PUT endpoints (update operations)
- [ ] All DELETE endpoints (delete operations)

### Phase 3: Authorization
- [ ] Admin-only endpoints
- [ ] Organization-scoped endpoints
- [ ] Role-based access control

### Phase 4: Advanced Features
- [ ] License expiration logic
- [ ] Auto-renewal background service
- [ ] Pagination, filtering, sorting

### Phase 5: Edge Cases
- [ ] Invalid inputs
- [ ] Missing resources
- [ ] Unauthorized/forbidden access
- [ ] Business rule violations

---

## Test Results Summary

### Endpoints Tested: 0 / 26
### Test Scenarios Passed: 0 / 50+
### Status: ⏳ Ready to Execute

---

## Notes

- All tests should be performed using Swagger UI or API testing tools
- Use different user roles (Admin, Owner, OrganizationAdmin, Member) for authorization tests
- Create test data as needed for comprehensive testing
- Document any issues or unexpected behaviors

---

**Last Updated**: January 14, 2026
**Test Plan Version**: 1.0
