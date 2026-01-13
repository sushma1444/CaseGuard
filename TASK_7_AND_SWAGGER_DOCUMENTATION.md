# Task 7: Authorization System & Swagger UI Configuration - Documentation

## Table of Contents
1. [Swagger UI Configuration](#swagger-ui-configuration)
2. [Task 7: Authorization System Implementation](#task-7-authorization-system-implementation)
3. [Usage Examples](#usage-examples)
4. [Testing Authorization](#testing-authorization)

---

## Swagger UI Configuration

### Overview
Swagger UI provides an interactive interface to test and explore the API endpoints. This section documents the changes made to enable Swagger UI in all environments.

### Changes Made

#### 1. Program.cs Modification
**File:** `CaseGuard.Backend.Assignment/Program.cs`

**Before:**
```csharp
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CaseGuard API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}
```

**After:**
```csharp
// Configure the HTTP request pipeline
// Enable Swagger in all environments for development/testing
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CaseGuard API v1");
    options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});
```

### What Changed?
- **Removed Environment Check**: Previously, Swagger UI was only available in Development mode. Now it's enabled in all environments (Development, Production, etc.).
- **Why?**: This allows easy API testing and documentation access regardless of the environment setting.

### Swagger UI Features

#### Accessing Swagger UI
1. **Start the application:**
   ```bash
   dotnet run --project CaseGuard.Backend.Assignment
   ```

2. **Open in browser:**
   - Navigate to: `http://localhost:5000`
   - Swagger UI will be displayed at the root URL

#### Swagger UI Components

1. **API Information Header**
   - **Title**: "CaseGuard Backend API"
   - **Version**: "v1"
   - **Specification**: OAS3 (OpenAPI 3.0)
   - **Description**: "Organization and License Management System API"

2. **Authorize Button**
   - Located in the top-right corner
   - Used to configure JWT token authentication
   - Click to open the authorization dialog
   - Enter your JWT token (obtained from `/api/Auth/login`)
   - Format: `Bearer <your-token>` or just `<your-token>`

3. **Endpoint Sections**
   - **Auth**: Authentication endpoints (login, claims)
   - **Health**: Health check endpoint
   - Each endpoint shows:
     - HTTP method (GET, POST, PUT, DELETE)
     - Endpoint path
     - Description
     - Lock icon (🔒) if authentication is required

4. **Schemas Section**
   - Lists all data models (DTOs)
   - Expandable to view structure
   - Shows property types and descriptions

### Using Swagger UI

#### Step 1: Test Public Endpoints
1. Click on `GET /api/Health`
2. Click "Try it out"
3. Click "Execute"
4. View the response (should return "healthy")

#### Step 2: Get Authentication Token
1. Click on `POST /api/Auth/login`
2. Click "Try it out"
3. Enter request body:
   ```json
   {
     "userId": "550e8400-e29b-41d4-a716-446655440000",
     "email": "admin@example.com",
     "role": "Admin"
   }
   ```
4. Click "Execute"
5. Copy the `token` from the response

#### Step 3: Authorize with JWT Token
1. Click the "Authorize" button (top-right)
2. In the "Value" field, enter: `Bearer <your-token>`
   - Example: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
3. Click "Authorize"
4. Click "Close"

#### Step 4: Test Protected Endpoints
1. Click on `GET /api/Auth/claims`
2. Click "Try it out"
3. Click "Execute"
4. View your JWT claims in the response

### Swagger Configuration Details

#### SwaggerEndpoint
- **Path**: `/swagger/v1/swagger.json`
- **Purpose**: Points to the OpenAPI JSON specification
- **Usage**: Swagger UI reads this JSON to generate the interactive documentation

#### RoutePrefix
- **Value**: `string.Empty` (empty string)
- **Purpose**: Makes Swagger UI available at the root URL (`/`)
- **Alternative**: If set to `"swagger"`, Swagger UI would be at `/swagger`

### Security Note
⚠️ **Important**: In production environments, consider:
- Restricting Swagger UI access to specific IPs or networks
- Using environment-based configuration to disable Swagger in production
- Implementing authentication for Swagger UI itself

---

## Task 7: Authorization System Implementation

### Overview
Task 7 implements a comprehensive authorization system that provides:
- Role-based access control (RBAC)
- Organization-scoped authorization
- Resource-level access checks
- Helper methods for common authorization scenarios

### Components Implemented

#### 1. AuthorizationHelper Class
**File:** `CaseGuard.Backend.Assignment/Helpers/AuthorizationHelper.cs`

A static helper class providing async methods for authorization checks.

##### Organization Membership Checks

**`IsMemberOfOrganizationAsync()`**
```csharp
Task<bool> IsMemberOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```
- **Purpose**: Checks if a user is a member of a specific organization
- **Returns**: `true` if user is a member, `false` otherwise
- **Usage**: Verify basic organization membership

**`GetOrganizationMembershipAsync()`**
```csharp
Task<OrganizationMember?> GetOrganizationMembershipAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```
- **Purpose**: Retrieves the organization membership record
- **Returns**: `OrganizationMember` object if found, `null` otherwise
- **Usage**: Get membership details including role

**`HasRoleInOrganizationAsync()`**
```csharp
Task<bool> HasRoleInOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId,
    string requiredRole)
```
- **Purpose**: Checks if a user has a specific role (or higher) in an organization
- **Role Hierarchy**:
  - `Member` → Can access if role is Member, OrganizationAdmin, or Owner
  - `OrganizationAdmin` → Can access if role is OrganizationAdmin or Owner
  - `Owner` → Can access only if role is Owner
- **Returns**: `true` if user has required role or higher, `false` otherwise

**`IsOwnerOrAdminOfOrganizationAsync()`**
```csharp
Task<bool> IsOwnerOrAdminOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```
- **Purpose**: Checks if user is Owner or OrganizationAdmin
- **Returns**: `true` if user is Owner or OrganizationAdmin, `false` otherwise

**`IsOwnerOfOrganizationAsync()`**
```csharp
Task<bool> IsOwnerOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```
- **Purpose**: Checks if user is the Owner of the organization
- **Returns**: `true` if user is Owner, `false` otherwise

##### Resource Access Checks

**`CanAccessLicenseAsync()`**
```csharp
Task<bool> CanAccessLicenseAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid licenseId)
```
- **Purpose**: Verifies if a user can access a specific license
- **Logic**: User must be a member of the license's organization
- **Returns**: `true` if user can access, `false` otherwise

**`CanAccessLicenseAssignmentAsync()`**
```csharp
Task<bool> CanAccessLicenseAssignmentAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid assignmentId)
```
- **Purpose**: Verifies if a user can access a license assignment
- **Logic**: User must be a member of the assignment's license organization
- **Returns**: `true` if user can access, `false` otherwise

**`CanAccessInvitationAsync()`**
```csharp
Task<bool> CanAccessInvitationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid invitationId)
```
- **Purpose**: Verifies if a user can access an invitation
- **Logic**: User must be a member of the invitation's organization
- **Returns**: `true` if user can access, `false` otherwise

##### Ensure Methods (Throw Exceptions)

These methods throw `ForbiddenException` if the check fails:

**`EnsureUserIsMemberOfOrganizationAsync()`**
```csharp
Task EnsureUserIsMemberOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```
- **Throws**: `ForbiddenException` with message "You do not have access to this organization."

**`EnsureUserHasRoleInOrganizationAsync()`**
```csharp
Task EnsureUserHasRoleInOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId,
    string requiredRole)
```
- **Throws**: `ForbiddenException` with message indicating required role

**`EnsureUserIsOwnerOrAdminOfOrganizationAsync()`**
```csharp
Task EnsureUserIsOwnerOrAdminOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```
- **Throws**: `ForbiddenException` with message "You must be an owner or admin of this organization to perform this action."

**`EnsureUserIsOwnerOfOrganizationAsync()`**
```csharp
Task EnsureUserIsOwnerOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```
- **Throws**: `ForbiddenException` with message "You must be the owner of this organization to perform this action."

##### Utility Methods

**`GetOrganizationIdFromClaims()`**
```csharp
Guid? GetOrganizationIdFromClaims(ClaimsPrincipal user)
```
- **Purpose**: Extracts organization ID from JWT claims (if present)
- **Returns**: `Guid?` - organization ID or `null` if not in claims

#### 2. Enhanced ClaimsHelper
**File:** `CaseGuard.Backend.Assignment/Helpers/ClaimsHelper.cs`

##### New Methods Added

**`IsOwnerOrOrganizationAdmin()`**
```csharp
bool IsOwnerOrOrganizationAdmin(ClaimsPrincipal user)
```
- **Purpose**: Checks if user has Owner, OrganizationAdmin, or Admin role in JWT claims
- **Returns**: `true` if user has one of these roles

**`GetOrganizationId()`**
```csharp
Guid? GetOrganizationId(ClaimsPrincipal user)
```
- **Purpose**: Gets organization ID from JWT claims
- **Returns**: `Guid?` - organization ID or `null`

**`GetUserIdAsGuid()`**
```csharp
Guid GetUserIdAsGuid(ClaimsPrincipal user)
```
- **Purpose**: Converts user ID string from claims to `Guid`
- **Throws**: `UnauthorizedException` if user ID is invalid or missing
- **Returns**: `Guid` representation of user ID

#### 3. Enhanced BaseController
**File:** `CaseGuard.Backend.Assignment/Controllers/BaseController.cs`

##### New Properties Added

**`CurrentUserIdGuid`**
```csharp
protected Guid CurrentUserIdGuid => ClaimsHelper.GetUserIdAsGuid(User);
```
- **Purpose**: Provides current user ID as `Guid` (useful for database queries)
- **Usage**: Use instead of `CurrentUserId` when you need a `Guid`

**`IsOwnerOrOrganizationAdmin`**
```csharp
protected bool IsOwnerOrOrganizationAdmin => ClaimsHelper.IsOwnerOrOrganizationAdmin(User);
```
- **Purpose**: Quick check if user is Owner, OrganizationAdmin, or Admin
- **Usage**: Conditional logic in controllers

**`CurrentOrganizationId`**
```csharp
protected Guid? CurrentOrganizationId => ClaimsHelper.GetOrganizationId(User);
```
- **Purpose**: Gets organization ID from JWT claims (if present)
- **Returns**: `Guid?` - may be `null` if not in claims

#### 4. Authorization Attributes
**File:** `CaseGuard.Backend.Assignment/Attributes/RequireOrganizationMembershipAttribute.cs`

##### Custom Attributes

**`RequireOrganizationMembershipAttribute`**
```csharp
[RequireOrganizationMembership]
public IActionResult SomeAction() { ... }
```
- **Purpose**: Requires authenticated user (uses "Member" policy)
- **Note**: Actual organization membership must be verified in controller using `AuthorizationHelper`

**`RequireOrganizationOwnerOrAdminAttribute`**
```csharp
[RequireOrganizationOwnerOrAdmin]
public IActionResult SomeAction() { ... }
```
- **Purpose**: Requires Owner, OrganizationAdmin, or Admin role (uses "OrganizationOwnerOrAdmin" policy)
- **Note**: Organization membership must still be verified in controller

#### 5. Authorization Policies
**File:** `CaseGuard.Backend.Assignment/Extensions/ServiceCollectionExtensions.cs`

##### Existing Policies

**`AdminOnly` Policy**
```csharp
[Authorize(Policy = "AdminOnly")]
```
- **Requirement**: User must have "Admin" role
- **Usage**: System-level admin operations

**`OrganizationOwnerOrAdmin` Policy**
```csharp
[Authorize(Policy = "OrganizationOwnerOrAdmin")]
```
- **Requirement**: User must have "Admin", "Owner", or "OrganizationAdmin" role
- **Usage**: Organization management operations

**`Member` Policy**
```csharp
[Authorize(Policy = "Member")]
// or simply
[Authorize]
```
- **Requirement**: User must be authenticated
- **Usage**: Any authenticated user operations

### Authorization Flow

```
1. User sends request with JWT token
   ↓
2. JWT Authentication Middleware validates token
   ↓
3. Authorization Middleware checks policies
   ↓
4. Controller action executes
   ↓
5. Controller uses AuthorizationHelper for organization checks
   ↓
6. AuthorizationHelper queries database for membership
   ↓
7. If check fails → ForbiddenException (403)
   If check passes → Action continues
```

### Role Hierarchy

```
Admin (System Level)
  ├── Full access to all organizations
  └── Can manage licenses globally

Owner (Organization Level)
  ├── Full control of their organization
  ├── Can manage members
  └── Can manage licenses in their organization

OrganizationAdmin (Organization Level)
  ├── Can manage members (except Owner)
  └── Can manage licenses in their organization

Member (Organization Level)
  └── Can view organization resources
```

---

## Usage Examples

### Example 1: Check Organization Membership

```csharp
[HttpGet("{organizationId}")]
[Authorize(Policy = "Member")]
public async Task<IActionResult> GetOrganization(Guid organizationId)
{
    // Ensure user is a member of the organization
    await AuthorizationHelper.EnsureUserIsMemberOfOrganizationAsync(
        _dbContext, 
        CurrentUserIdGuid, 
        organizationId);

    // Proceed with fetching organization
    var organization = await _dbContext.Organizations
        .FirstOrDefaultAsync(o => o.Id == organizationId);
    
    return Ok(organization);
}
```

### Example 2: Require Owner or Admin Role

```csharp
[HttpDelete("{organizationId}")]
[Authorize(Policy = "OrganizationOwnerOrAdmin")]
public async Task<IActionResult> DeleteOrganization(Guid organizationId)
{
    // Ensure user is owner or admin of the organization
    await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
        _dbContext,
        CurrentUserIdGuid,
        organizationId);

    // Proceed with deletion
    var organization = await _dbContext.Organizations
        .FirstOrDefaultAsync(o => o.Id == organizationId);
    
    _dbContext.Organizations.Remove(organization);
    await _dbContext.SaveChangesAsync();
    
    return NoContent();
}
```

### Example 3: Check License Access

```csharp
[HttpGet("licenses/{licenseId}")]
[Authorize]
public async Task<IActionResult> GetLicense(Guid licenseId)
{
    // Check if user can access this license
    var canAccess = await AuthorizationHelper.CanAccessLicenseAsync(
        _dbContext,
        CurrentUserIdGuid,
        licenseId);

    if (!canAccess)
    {
        throw new ForbiddenException("You do not have access to this license.");
    }

    var license = await _dbContext.Licenses
        .FirstOrDefaultAsync(l => l.Id == licenseId);
    
    return Ok(license);
}
```

### Example 4: Admin-Only Endpoint

```csharp
[HttpPost("licenses")]
[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> CreateLicense([FromBody] CreateLicenseRequest request)
{
    // Only system admins can create licenses
    // No additional organization checks needed
    
    var license = new License
    {
        // ... map from request
    };
    
    _dbContext.Licenses.Add(license);
    await _dbContext.SaveChangesAsync();
    
    return CreatedAtAction(nameof(GetLicense), new { id = license.Id }, license);
}
```

### Example 5: Role-Based Access with Hierarchy

```csharp
[HttpPut("members/{memberId}/role")]
[Authorize(Policy = "OrganizationOwnerOrAdmin")]
public async Task<IActionResult> UpdateMemberRole(
    Guid organizationId, 
    Guid memberId, 
    [FromBody] UpdateMemberRoleRequest request)
{
    // Ensure user is owner or admin
    await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
        _dbContext,
        CurrentUserIdGuid,
        organizationId);

    // If promoting to Owner, ensure current user is Owner
    if (request.Role == Roles.Owner)
    {
        await AuthorizationHelper.EnsureUserIsOwnerOfOrganizationAsync(
            _dbContext,
            CurrentUserIdGuid,
            organizationId);
    }

    // Update member role
    var member = await _dbContext.OrganizationMembers
        .FirstOrDefaultAsync(m => m.Id == memberId && m.OrganizationId == organizationId);
    
    member.Role = request.Role;
    await _dbContext.SaveChangesAsync();
    
    return Ok(member);
}
```

---

## Testing Authorization

### Test Scenarios

#### 1. Test Organization Membership
```bash
# 1. Login as a user
POST /api/Auth/login
{
  "userId": "user-guid",
  "email": "user@example.com",
  "role": "Member"
}

# 2. Try to access organization (should succeed if member)
GET /api/Organizations/{organizationId}
Authorization: Bearer <token>

# 3. Try to access different organization (should fail with 403)
GET /api/Organizations/{other-organization-id}
Authorization: Bearer <token>
```

#### 2. Test Role-Based Access
```bash
# 1. Login as Member
POST /api/Auth/login
{
  "userId": "member-guid",
  "email": "member@example.com",
  "role": "Member"
}

# 2. Try to delete organization (should fail with 403)
DELETE /api/Organizations/{organizationId}
Authorization: Bearer <member-token>

# 3. Login as Owner
POST /api/Auth/login
{
  "userId": "owner-guid",
  "email": "owner@example.com",
  "role": "Owner"
}

# 4. Try to delete organization (should succeed)
DELETE /api/Organizations/{organizationId}
Authorization: Bearer <owner-token>
```

#### 3. Test Admin Access
```bash
# 1. Login as Admin
POST /api/Auth/login
{
  "userId": "admin-guid",
  "email": "admin@example.com",
  "role": "Admin"
}

# 2. Create license (should succeed)
POST /api/Licenses
Authorization: Bearer <admin-token>
{
  "name": "Premium License",
  "maxUsers": 100
}

# 3. Login as Member
POST /api/Auth/login
{
  "userId": "member-guid",
  "email": "member@example.com",
  "role": "Member"
}

# 4. Try to create license (should fail with 403)
POST /api/Licenses
Authorization: Bearer <member-token>
```

### Expected HTTP Status Codes

- **200 OK**: Request successful, user has access
- **401 Unauthorized**: Missing or invalid JWT token
- **403 Forbidden**: User is authenticated but lacks required permissions
- **404 Not Found**: Resource doesn't exist or user doesn't have access
- **400 Bad Request**: Invalid request data

### Common Authorization Errors

#### Error: "You do not have access to this organization."
- **Cause**: User is not a member of the organization
- **Solution**: Verify user's organization membership in database

#### Error: "You must be an owner or admin of this organization to perform this action."
- **Cause**: User is a member but not Owner or OrganizationAdmin
- **Solution**: Check user's role in `OrganizationMembers` table

#### Error: "You must be the owner of this organization to perform this action."
- **Cause**: Action requires Owner role, but user is OrganizationAdmin or Member
- **Solution**: Only Owner can perform this action

---

## Summary

### Swagger UI Changes
- ✅ Enabled Swagger UI in all environments
- ✅ Swagger UI accessible at root URL (`http://localhost:5000`)
- ✅ JWT authentication support via "Authorize" button
- ✅ Interactive API testing interface

### Authorization System Features
- ✅ Comprehensive organization membership checks
- ✅ Role-based access control (RBAC)
- ✅ Resource-level authorization
- ✅ Helper methods for common scenarios
- ✅ Exception-based authorization failures
- ✅ Async/await support for database queries
- ✅ Custom authorization attributes
- ✅ Enhanced BaseController with authorization helpers

### Files Created/Modified
1. **Created:**
   - `Helpers/AuthorizationHelper.cs` - Main authorization logic
   - `Attributes/RequireOrganizationMembershipAttribute.cs` - Custom attributes

2. **Modified:**
   - `Program.cs` - Swagger UI configuration
   - `Helpers/ClaimsHelper.cs` - Added organization-related methods
   - `Controllers/BaseController.cs` - Added authorization properties

### Next Steps
The authorization system is now ready to be used in all controllers. When implementing:
- **Task 8**: LicenseController (Admin endpoints)
- **Task 9**: OrganizationController
- **Task 10**: MemberController
- **Task 11**: InvitationController
- **Task 12**: LicenseAssignmentController
- **Task 13**: UserController

Use `AuthorizationHelper` methods to enforce proper access control based on user roles and organization membership.

---

**Document Version**: 1.0  
**Last Updated**: January 12, 2026  
**Author**: CaseGuard Development Team
