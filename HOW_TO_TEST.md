# How to Test the API - Step-by-Step Guide

## Prerequisites

1. **Application Running**: Make sure the application is running
   ```bash
   dotnet run --project CaseGuard.Backend.Assignment
   ```

2. **Database Setup**: Ensure PostgreSQL is running and database is created
   ```bash
   dotnet ef database update
   ```

3. **Test Data**: Optionally load test data (see `test_data_setup.sql`)

---

## Step 1: Access Swagger UI

1. **Start the application** (if not already running):
   ```bash
   dotnet run --project CaseGuard.Backend.Assignment
   ```

2. **Open Swagger UI** in your browser:
   - URL: `http://localhost:5000/swagger`
   - You should see all available endpoints organized by controller

---

## Step 2: Test Authentication

### 2.1 Login as Admin

1. **Find the endpoint**: `POST /api/auth/login`
2. **Click "Try it out"**
3. **Enter request body**:
   ```json
   {
     "userId": "11111111-1111-1111-1111-111111111111",
     "email": "admin@example.com",
     "role": "Admin"
   }
   ```
4. **Click "Execute"**
5. **Copy the token** from the response (you'll need it for all other requests)

### 2.2 Authorize in Swagger

1. **Click the "Authorize" button** at the top right of Swagger UI
2. **Enter**: `Bearer {your-token-here}` (replace `{your-token-here}` with the actual token)
3. **Click "Authorize"** then **"Close"**
4. Now all requests will automatically include the authorization header

---

## Step 3: Test Admin Endpoints (LicenseController)

**Required Role**: Admin

### 3.1 Create a License

1. **Endpoint**: `POST /api/license`
2. **Request Body**:
   ```json
   {
     "organizationId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
     "name": "Premium License",
     "startDate": "2026-01-14T00:00:00Z",
     "expirationDate": "2026-12-31T23:59:59Z",
     "autoRenewalEnabled": true
   }
   ```
3. **Expected**: 201 Created with license details
4. **Note**: Save the `id` from the response for later tests

### 3.2 Get All Licenses

1. **Endpoint**: `GET /api/license`
2. **Query Parameters** (optional):
   - `page=1`
   - `pageSize=10`
   - `isActive=true`
   - `expirationStatus=active`
   - `sortBy=expirationdate`
   - `sortDirection=asc`
3. **Expected**: 200 OK with paginated list

### 3.3 Get License by ID

1. **Endpoint**: `GET /api/license/{id}`
2. **Replace `{id}`** with a license ID from step 3.1
3. **Expected**: 200 OK with license details

### 3.4 Update License

1. **Endpoint**: `PUT /api/license/{id}`
2. **Request Body**:
   ```json
   {
     "name": "Updated License Name",
     "autoRenewalEnabled": false
   }
   ```
3. **Expected**: 200 OK with updated license

### 3.5 Test Authorization (Non-Admin)

1. **Login as a regular user** (not Admin):
   ```json
   {
     "userId": "22222222-2222-2222-2222-222222222222",
     "email": "user@example.com",
     "role": "Member"
   }
   ```
2. **Try to create a license** (`POST /api/license`)
3. **Expected**: 403 Forbidden

---

## Step 4: Test Organization Endpoints

**Required Role**: Any authenticated user (for create), Member of organization (for view/update)

### 4.1 Create Organization

1. **Endpoint**: `POST /api/organization`
2. **Request Body**:
   ```json
   {
     "name": "My Test Organization",
     "description": "This is a test organization"
   }
   ```
3. **Expected**: 201 Created, you become the Owner
4. **Save the `id`** for later tests

### 4.2 Get All Organizations

1. **Endpoint**: `GET /api/organization`
2. **Query Parameters** (optional):
   - `page=1`
   - `pageSize=10`
   - `searchTerm=Test`
3. **Expected**: 200 OK with your organizations

### 4.3 Get Organization by ID

1. **Endpoint**: `GET /api/organization/{id}`
2. **Expected**: 200 OK with organization details

### 4.4 Update Organization

1. **Endpoint**: `PUT /api/organization/{id}`
2. **Request Body**:
   ```json
   {
     "name": "Updated Organization Name",
     "description": "Updated description"
   }
   ```
3. **Expected**: 200 OK

---

## Step 5: Test Member Endpoints

**Required Role**: Owner or OrganizationAdmin

### 5.1 Invite a Member

1. **Endpoint**: `POST /api/member/{organizationId}/invite`
2. **Replace `{organizationId}`** with your organization ID
3. **Request Body**:
   ```json
   {
     "email": "newmember@example.com",
     "role": "Member"
   }
   ```
4. **Expected**: 201 Created with invitation details
5. **Save the `invitationId`** for later

### 5.2 Get All Members

1. **Endpoint**: `GET /api/member/{organizationId}`
2. **Expected**: 200 OK with list of members

### 5.3 Get Member by ID

1. **Endpoint**: `GET /api/member/{organizationId}/{memberId}`
2. **Expected**: 200 OK with member details

### 5.4 Update Member Role

1. **Endpoint**: `PUT /api/member/{organizationId}/{memberId}/role`
2. **Request Body**:
   ```json
   {
     "role": "OrganizationAdmin"
   }
   ```
3. **Expected**: 200 OK with updated member

---

## Step 6: Test Invitation Endpoints

**Required Role**: Owner or OrganizationAdmin

### 6.1 Get All Invitations

1. **Endpoint**: `GET /api/invitation/{organizationId}`
2. **Expected**: 200 OK with list of invitations

### 6.2 Get Invitation by ID

1. **Endpoint**: `GET /api/invitation/{organizationId}/{invitationId}`
2. **Expected**: 200 OK with invitation details

### 6.3 Cancel Invitation

1. **Endpoint**: `DELETE /api/invitation/{organizationId}/{invitationId}`
2. **Expected**: 204 No Content

---

## Step 7: Test License Assignment Endpoints

**Required Role**: Owner or OrganizationAdmin (for assign), Member (for view)

### 7.1 Assign License to User

1. **Endpoint**: `POST /api/licenseassignment`
2. **Request Body**:
   ```json
   {
     "licenseId": "{license-id-from-step-3.1}",
     "userId": "{user-id}"
   }
   ```
3. **Expected**: 201 Created with assignment details

### 7.2 Get All Assignments

1. **Endpoint**: `GET /api/licenseassignment`
2. **Query Parameters** (optional):
   - `page=1`
   - `pageSize=10`
   - `licenseId={id}`
   - `userId={id}`
3. **Expected**: 200 OK with list of assignments

### 7.3 Get Assignment by ID

1. **Endpoint**: `GET /api/licenseassignment/{id}`
2. **Expected**: 200 OK with assignment details

### 7.4 Unassign License

1. **Endpoint**: `DELETE /api/licenseassignment/{id}`
2. **Expected**: 204 No Content

---

## Step 8: Test User Endpoints

**Required Role**: Any authenticated user

### 8.1 Get User Organizations

1. **Endpoint**: `GET /api/user/organizations`
2. **Expected**: 200 OK with list of organizations you belong to

### 8.2 Get Organization Details

1. **Endpoint**: `GET /api/user/organizations/{organizationId}`
2. **Expected**: 200 OK with organization details

### 8.3 Accept Invitation

1. **First, create an invitation** (see Step 5.1)
2. **Login as the invited user**
3. **Endpoint**: `POST /api/user/invitations/accept`
4. **Request Body**:
   ```json
   {
     "invitationId": "{invitation-id}"
   }
   ```
5. **Expected**: 200 OK, you're now a member

### 8.4 Leave Organization

1. **Endpoint**: `DELETE /api/user/organizations/{organizationId}`
2. **Expected**: 204 No Content

---

## Step 9: Test License Expiration

### 9.1 Create License with Short Expiration

1. **Login as Admin**
2. **Create a license** with expiration in 1 minute:
   ```json
   {
     "organizationId": "{org-id}",
     "name": "Short Expiration License",
     "startDate": "2026-01-14T00:00:00Z",
     "expirationDate": "2026-01-14T00:01:00Z",
     "autoRenewalEnabled": false
   }
   ```

### 9.2 Wait for Expiration

1. **Wait 1+ minute** after the expiration date

### 9.3 Check Expiration (Automatic)

1. **Get the license**: `GET /api/license/{id}`
2. **Expected**: License should show `isActive: false`, `isValid: false`

### 9.4 Try to Assign Expired License

1. **Try to assign the expired license**: `POST /api/licenseassignment`
2. **Expected**: 400 Bad Request - "Cannot assign an invalid or expired license"

---

## Step 10: Test Auto-Renewal

### 10.1 Create License with Auto-Renewal

1. **Create a license** with auto-renewal enabled and expiration in 6 days:
   ```json
   {
     "organizationId": "{org-id}",
     "name": "Auto-Renewal License",
     "startDate": "2026-01-14T00:00:00Z",
     "expirationDate": "2026-01-20T00:00:00Z",
     "autoRenewalEnabled": true
   }
   ```

### 10.2 Check Background Service

1. **Check application logs** for: "License Renewal Background Service started"
2. **The service runs every hour** and renews licenses expiring within 7 days

### 10.3 Verify Renewal (Manual Test)

1. **Note the expiration date** from step 10.1
2. **Wait for background service** to run (or manually trigger if you have access)
3. **Get the license**: `GET /api/license/{id}`
4. **Expected**: Expiration date should be extended

---

## Step 11: Test Pagination, Filtering, and Sorting

### 11.1 Test Pagination

1. **Get licenses** with pagination:
   ```
   GET /api/license?page=1&pageSize=5
   ```
2. **Expected**: Returns 5 items, includes pagination metadata

### 11.2 Test Filtering

1. **Filter active licenses**:
   ```
   GET /api/license?isActive=true&expirationStatus=active
   ```
2. **Expected**: Returns only active, non-expired licenses

### 11.3 Test Sorting

1. **Sort by expiration date**:
   ```
   GET /api/license?sortBy=expirationdate&sortDirection=asc
   ```
2. **Expected**: Licenses sorted by expiration date (ascending)

### 11.4 Test Search

1. **Search by name**:
   ```
   GET /api/license?searchTerm=Premium
   ```
2. **Expected**: Returns licenses matching "Premium" in name

---

## Step 12: Test Error Handling

### 12.1 Invalid Request

1. **Try to create license** with missing required fields
2. **Expected**: 400 Bad Request with validation errors

### 12.2 Resource Not Found

1. **Get license** with invalid ID: `GET /api/license/invalid-id`
2. **Expected**: 404 Not Found

### 12.3 Unauthorized Access

1. **Remove authorization** (click "Authorize" and remove token)
2. **Try any endpoint**
3. **Expected**: 401 Unauthorized

### 12.4 Forbidden Access

1. **Login as regular user** (not Admin)
2. **Try Admin-only endpoint**: `POST /api/license`
3. **Expected**: 403 Forbidden

---

## Quick Testing Checklist

### ✅ Authentication
- [ ] Login as Admin
- [ ] Login as regular user
- [ ] Get claims

### ✅ Admin Endpoints
- [ ] Create license
- [ ] Get all licenses
- [ ] Get license by ID
- [ ] Update license
- [ ] Cancel license
- [ ] Test non-admin access (should fail)

### ✅ Organization Endpoints
- [ ] Create organization
- [ ] Get all organizations
- [ ] Get organization by ID
- [ ] Update organization
- [ ] Delete organization

### ✅ Member Endpoints
- [ ] Invite member
- [ ] Get all members
- [ ] Get member by ID
- [ ] Update member role
- [ ] Remove member

### ✅ Invitation Endpoints
- [ ] Get all invitations
- [ ] Get invitation by ID
- [ ] Cancel invitation

### ✅ License Assignment
- [ ] Assign license
- [ ] Get all assignments
- [ ] Get assignment by ID
- [ ] Unassign license
- [ ] Try to assign expired license (should fail)

### ✅ User Endpoints
- [ ] Get user organizations
- [ ] Get organization details
- [ ] Accept invitation
- [ ] Leave organization

### ✅ Advanced Features
- [ ] License expiration
- [ ] Auto-renewal
- [ ] Pagination
- [ ] Filtering
- [ ] Sorting
- [ ] Search

### ✅ Error Handling
- [ ] Invalid requests
- [ ] Not found
- [ ] Unauthorized
- [ ] Forbidden

---

## Tips for Testing

1. **Use Swagger UI**: It's the easiest way to test all endpoints
2. **Save IDs**: Keep track of created resource IDs for later tests
3. **Test Different Roles**: Test as Admin, Owner, OrganizationAdmin, and Member
4. **Test Edge Cases**: Try invalid inputs, missing resources, etc.
5. **Check Logs**: Monitor application logs for errors
6. **Test Authorization**: Make sure users can only access what they should

---

## Common Issues

### Issue: 401 Unauthorized
- **Solution**: Make sure you've authorized in Swagger UI with a valid token

### Issue: 403 Forbidden
- **Solution**: Check that your user has the required role for the endpoint

### Issue: 404 Not Found
- **Solution**: Verify the resource ID exists and you have access to it

### Issue: 400 Bad Request
- **Solution**: Check request body format and required fields

---

## Next Steps

After testing:
1. Document any issues found
2. Verify all user stories work end-to-end
3. Test authorization thoroughly
4. Verify license expiration and auto-renewal
5. Test pagination, filtering, and sorting on all list endpoints

---

**Happy Testing! 🚀**
