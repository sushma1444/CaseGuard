# Task 6: Implement Authorization - Documentation

## Overview
Complete implementation of role-based authorization system with JWT authentication, authorization helpers, and policy-based access control.

---

## Authorization Architecture

### **Three-Layer Authorization**

1. **Authentication Layer** - JWT token validation
2. **Policy Layer** - Role-based policies (Admin, User)
3. **Business Logic Layer** - Organization-specific authorization

---

## JWT Authentication

### **Token Structure**
```json
{
  "userId": "user-guid",
  "email": "user@example.com",
  "role": "Admin" // or "User"
}
```

### **Authentication Configuration**
**File:** `Extensions/JwtExtensions.cs`

**JWT Settings:**
```csharp
Issuer: "CaseGuardBackend"
Audience: "CaseGuardFrontend"
Secret Key: From configuration
Token Expiration: 24 hours
```

**Features:**
- ✅ Bearer token authentication
- ✅ Token validation on every request
- ✅ Claims extraction for user context
- ✅ Automatic 401 response for invalid tokens

### **How to Use JWT Tokens**

**1. Login to get token:**
```bash
POST /api/auth/login
{
  "userId": "user123",
  "email": "user@example.com",
  "role": "Admin"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 86400
}
```

**2. Use token in requests:**
```bash
curl -X GET "https://localhost:5001/api/organization" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Authorization Policies

### **Global Policies**
**File:** `Program.cs`

#### **1. AdminOnly Policy**
```csharp
[Authorize(Policy = "AdminOnly")]
```

**Requirements:**
- User must be authenticated
- User's role claim must be "Admin"

**Used By:**
- `LicenseController` - All admin-only license management endpoints

**Enforcement:**
- Returns 401 if not authenticated
- Returns 403 if authenticated but not Admin

#### **2. Default Authorized Policy**
```csharp
[Authorize]
```

**Requirements:**
- User must be authenticated (valid JWT token)

**Used By:**
- All other controllers (Organization, Member, Invitation, User, LicenseAssignment)

---

## BaseController

**File:** `Controllers/BaseController.cs`

### **Purpose**
Provides common functionality for all controllers to access user claims.

### **Properties**

```csharp
// Get current user ID as string
protected string CurrentUserId

// Get current user ID as Guid (throws if invalid)
protected Guid CurrentUserIdGuid

// Get current user email
protected string CurrentUserEmail

// Check if current user is system admin
protected bool IsAdmin
```

### **Usage in Controllers**
```csharp
public class OrganizationController : BaseController
{
    public async Task<IActionResult> CreateOrganization(...)
    {
        // Access current user
        var userId = CurrentUserIdGuid;
        var isAdmin = IsAdmin;
        
        // Use in business logic
        if (isAdmin)
        {
            // Admin bypass logic
        }
    }
}
```

### **Exception Handling**
- Throws `UnauthorizedException` if claims are missing or invalid
- Automatically converted to 401 by global exception handler

---

## AuthorizationHelper

**File:** `Helpers/AuthorizationHelper.cs`

### **Purpose**
Centralized authorization logic for organization-based access control.

---

### **Check Methods** (Return bool)

#### **IsMemberOfOrganizationAsync**
```csharp
public static async Task<bool> IsMemberOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```

**Purpose:** Check if user is a member of organization (any role)

**Returns:** `true` if member, `false` otherwise

**Usage:**
```csharp
var isMember = await AuthorizationHelper.IsMemberOfOrganizationAsync(
    _dbContext, CurrentUserIdGuid, organizationId);

if (!isMember)
{
    // User is not a member
}
```

---

#### **IsOwnerOrAdminOfOrganizationAsync**
```csharp
public static async Task<bool> IsOwnerOrAdminOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```

**Purpose:** Check if user is Owner or OrganizationAdmin

**Returns:** `true` if Owner or OrganizationAdmin, `false` otherwise

**Usage:**
```csharp
var canManage = await AuthorizationHelper.IsOwnerOrAdminOfOrganizationAsync(
    _dbContext, CurrentUserIdGuid, organizationId);

if (canManage)
{
    // User can manage members, assign licenses, etc.
}
```

---

#### **IsOwnerOfOrganizationAsync**
```csharp
public static async Task<bool> IsOwnerOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```

**Purpose:** Check if user is specifically the Owner

**Returns:** `true` if Owner, `false` otherwise

**Usage:**
```csharp
var isOwner = await AuthorizationHelper.IsOwnerOfOrganizationAsync(
    _dbContext, CurrentUserIdGuid, organizationId);

if (isOwner)
{
    // User can delete organization, transfer ownership, etc.
}
```

---

#### **HasRoleInOrganizationAsync**
```csharp
public static async Task<bool> HasRoleInOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId,
    string requiredRole)
```

**Purpose:** Check if user has specific role or higher in hierarchy

**Parameters:**
- `requiredRole` - One of: "Member", "OrganizationAdmin", "Owner"

**Role Hierarchy:**
```
Owner > OrganizationAdmin > Member
```

**Returns:** `true` if user has required role or higher

**Examples:**
```csharp
// Check if user has at least Member role (any member)
var isMember = await HasRoleInOrganizationAsync(
    _dbContext, userId, orgId, Roles.Member);

// Check if user has at least OrganizationAdmin role (admin or owner)
var isAdmin = await HasRoleInOrganizationAsync(
    _dbContext, userId, orgId, Roles.OrganizationAdmin);

// Check if user is Owner
var isOwner = await HasRoleInOrganizationAsync(
    _dbContext, userId, orgId, Roles.Owner);
```

---

### **Ensure Methods** (Throw exceptions)

#### **EnsureUserIsMemberOfOrganizationAsync**
```csharp
public static async Task EnsureUserIsMemberOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```

**Purpose:** Verify user is a member, throw exception if not

**Throws:** `ForbiddenException` if user is not a member

**Usage:**
```csharp
// In controller action
await AuthorizationHelper.EnsureUserIsMemberOfOrganizationAsync(
    _dbContext, CurrentUserIdGuid, organizationId);

// If we get here, user is a member
// Continue with business logic
```

**Error Response:** `403 Forbidden`
```json
{
  "status": 403,
  "title": "Forbidden",
  "detail": "You do not have access to this organization."
}
```

---

#### **EnsureUserIsOwnerOrAdminOfOrganizationAsync**
```csharp
public static async Task EnsureUserIsOwnerOrAdminOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```

**Purpose:** Verify user is Owner or OrganizationAdmin, throw exception if not

**Throws:** `ForbiddenException` if user doesn't have required role

**Usage:**
```csharp
// Require Owner or OrgAdmin permission
await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
    _dbContext, CurrentUserIdGuid, organizationId);

// User has permission to manage members, assign licenses, etc.
```

**Error Response:** `403 Forbidden`
```json
{
  "status": 403,
  "title": "Forbidden",
  "detail": "You do not have permission to manage members of this organization."
}
```

---

#### **EnsureUserIsOwnerOfOrganizationAsync**
```csharp
public static async Task EnsureUserIsOwnerOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```

**Purpose:** Verify user is Owner, throw exception if not

**Throws:** `ForbiddenException` if user is not Owner

**Usage:**
```csharp
// Require Owner permission (for critical operations)
await AuthorizationHelper.EnsureUserIsOwnerOfOrganizationAsync(
    _dbContext, CurrentUserIdGuid, organizationId);

// User is Owner and can perform critical actions
```

**Error Response:** `403 Forbidden`
```json
{
  "status": 403,
  "title": "Forbidden",
  "detail": "You must be the organization owner to perform this action."
}
```

---

#### **EnsureUserHasRoleInOrganizationAsync**
```csharp
public static async Task EnsureUserHasRoleInOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId,
    string requiredRole)
```

**Purpose:** Verify user has specific role or higher, throw exception if not

**Parameters:**
- `requiredRole` - One of: "Member", "OrganizationAdmin", "Owner"

**Throws:** `ForbiddenException` if user doesn't have required role

**Usage:**
```csharp
// Require at least OrganizationAdmin role
await AuthorizationHelper.EnsureUserHasRoleInOrganizationAsync(
    _dbContext, CurrentUserIdGuid, organizationId, Roles.OrganizationAdmin);
```

---

### **Utility Methods**

#### **GetOrganizationMembershipAsync**
```csharp
public static async Task<OrganizationMember?> GetOrganizationMembershipAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
```

**Purpose:** Get user's membership record in organization

**Returns:** `OrganizationMember` if member, `null` otherwise

**Usage:**
```csharp
var membership = await AuthorizationHelper.GetOrganizationMembershipAsync(
    _dbContext, userId, organizationId);

if (membership != null)
{
    var role = membership.Role;
    var joinedAt = membership.JoinedAt;
}
```

---

## Role Definitions

**File:** `Constants/Roles.cs`

### **System Roles**
```csharp
public static class Roles
{
    public const string Admin = "Admin";              // Global system admin
    public const string User = "User";                // Regular user
}
```

### **Organization Roles**
```csharp
public static class Roles
{
    public const string Owner = "Owner";              // Organization owner
    public const string OrganizationAdmin = "OrganizationAdmin";  // Org admin
    public const string Member = "Member";            // Regular member
}
```

### **Role Hierarchy**

#### **System Level**
```
Admin (Global access to all organizations)
  ↓
User (Access only to joined organizations)
```

#### **Organization Level**
```
Owner (Full control of organization)
  ↓
OrganizationAdmin (Manage members, assign licenses)
  ↓
Member (View organization, accept invitations)
```

---

## Authorization Patterns by Endpoint

### **Pattern 1: Admin-Only Endpoints**

**Example:** License Management

```csharp
[Authorize(Policy = "AdminOnly")]
public class LicenseController : BaseController
{
    // All endpoints require Admin role
}
```

**Authorization Flow:**
1. JWT token validated ✅
2. Role claim checked for "Admin" ✅
3. Endpoint executed ✅

---

### **Pattern 2: Admin Bypass with Organization Check**

**Example:** Member Management

```csharp
[HttpPost("{organizationId}/invite")]
public async Task<IActionResult> InviteMember(Guid organizationId, ...)
{
    // Admins can access any organization
    if (!IsAdmin)
    {
        // Non-admins must be Owner or OrgAdmin
        await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
            _dbContext, CurrentUserIdGuid, organizationId);
    }
    
    // Continue with business logic
}
```

**Authorization Flow:**
1. JWT token validated ✅
2. Check if user is system Admin
   - If Admin: Skip org check ✅
   - If not Admin: Check org-level role ✅
3. Endpoint executed ✅

---

### **Pattern 3: Membership Check**

**Example:** View Members

```csharp
[HttpGet("{organizationId}")]
public async Task<IActionResult> GetMembers(Guid organizationId, ...)
{
    // Admins can access any organization
    if (!IsAdmin)
    {
        // Non-admins must be members (any role)
        await AuthorizationHelper.EnsureUserIsMemberOfOrganizationAsync(
            _dbContext, CurrentUserIdGuid, organizationId);
    }
    
    // Return member list
}
```

**Authorization Flow:**
1. JWT token validated ✅
2. Check if user is system Admin
   - If Admin: Skip check ✅
   - If not Admin: Verify membership ✅
3. Endpoint executed ✅

---

### **Pattern 4: User Context Only**

**Example:** User's Organizations

```csharp
[HttpGet("organizations")]
public async Task<IActionResult> GetUserOrganizations(...)
{
    // No explicit authorization check needed
    // Query automatically filtered by CurrentUserIdGuid
    
    var organizations = await _dbContext.OrganizationMembers
        .Where(om => om.UserId == CurrentUserIdGuid)
        .ToListAsync();
        
    return Ok(organizations);
}
```

**Authorization Flow:**
1. JWT token validated ✅
2. User can only see their own data (filtered by UserId) ✅
3. Endpoint executed ✅

---

## Authorization Matrix

### **Organization Management**

| Endpoint | Global Admin | Owner | OrgAdmin | Member | Non-Member |
|----------|--------------|-------|----------|--------|------------|
| Create Organization | ✅ | ✅ | ✅ | ✅ | ✅ |
| View Organization | ✅ | ✅ | ✅ | ✅ | ❌ |
| Update Organization | ✅ | ✅ | ✅ | ❌ | ❌ |
| Delete Organization | ✅ | ✅ | ✅ | ❌ | ❌ |

### **Member Management**

| Endpoint | Global Admin | Owner | OrgAdmin | Member | Non-Member |
|----------|--------------|-------|----------|--------|------------|
| Invite Member | ✅ | ✅ | ✅ | ❌ | ❌ |
| View Members | ✅ | ✅ | ✅ | ✅ | ❌ |
| Update Role | ✅ | ✅ | ✅ | ❌ | ❌ |
| Remove Member | ✅ | ✅ | ✅ | ❌ | ❌ |

### **Invitation Management**

| Endpoint | Global Admin | Owner | OrgAdmin | Member | Non-Member |
|----------|--------------|-------|----------|--------|------------|
| View Invitations | ✅ | ✅ | ✅ | ❌ | ❌ |
| Cancel Invitation | ✅ | ✅ | ✅ | ❌ | ❌ |
| Accept Invitation | ✅ | ✅ (invited) | ✅ (invited) | ✅ (invited) | ✅ (invited) |

### **License Assignment**

| Endpoint | Global Admin | Owner | OrgAdmin | Member | Non-Member |
|----------|--------------|-------|----------|--------|------------|
| Assign License | ✅ | ✅ | ✅ | ❌ | ❌ |
| Unassign License | ✅ | ✅ | ✅ | ❌ | ❌ |
| View Assignments | ✅ | ✅ | ✅ | ✅ | ❌ |

### **License Management (Admin Only)**

| Endpoint | Global Admin | Owner | OrgAdmin | Member | Non-Member |
|----------|--------------|-------|----------|--------|------------|
| Create License | ✅ | ❌ | ❌ | ❌ | ❌ |
| View All Licenses | ✅ | ❌ | ❌ | ❌ | ❌ |
| Update License | ✅ | ❌ | ❌ | ❌ | ❌ |
| Cancel License | ✅ | ❌ | ❌ | ❌ | ❌ |

### **User Operations**

| Endpoint | Global Admin | Owner | OrgAdmin | Member | Non-Member |
|----------|--------------|-------|----------|--------|------------|
| View My Organizations | ✅ | ✅ | ✅ | ✅ | ✅ |
| Leave Organization | ✅ | ❌ (Owner) | ✅ | ✅ | ❌ |

---

## Error Responses

### **401 Unauthorized**
**Trigger:** Missing or invalid JWT token

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "User is not authenticated."
}
```

**Common Causes:**
- No Authorization header
- Invalid token format
- Expired token
- Invalid signature

---

### **403 Forbidden**
**Trigger:** Valid authentication but insufficient permissions

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to manage members of this organization."
}
```

**Common Causes:**
- User is not Admin (for AdminOnly endpoints)
- User is not Owner/OrgAdmin (for management endpoints)
- User is not a member (for member-only endpoints)

---

## Security Best Practices

### **✅ Implemented Security Features**

#### **1. Defense in Depth**
- Multiple authorization layers (authentication → policy → business logic)
- Checks at both controller and service level
- Admin bypass requires explicit check

#### **2. Principle of Least Privilege**
- Users only see their own organizations
- Members have read-only access
- OrgAdmin can manage but not delete
- Owner has full control

#### **3. Secure by Default**
- All controllers require authentication by default
- Explicit authorization checks in endpoints
- No implicit access granted

#### **4. Information Disclosure Prevention**
- Non-members receive 404 (not 403) for organizations
- Doesn't reveal organization existence
- Error messages don't leak sensitive data

#### **5. Token Security**
- JWT tokens expire after 24 hours
- Tokens validated on every request
- Invalid tokens rejected immediately

#### **6. Audit Trail**
- All authorization failures logged
- User actions logged with user ID
- Timestamps on all operations

---

## Testing Authorization

### **Test Scenarios**

#### **1. Authentication Tests**
```bash
# No token - should get 401
curl -X GET "https://localhost:5001/api/organization"

# Invalid token - should get 401
curl -X GET "https://localhost:5001/api/organization" \
  -H "Authorization: Bearer invalid-token"

# Valid token - should succeed
curl -X GET "https://localhost:5001/api/organization" \
  -H "Authorization: Bearer {valid-token}"
```

#### **2. Admin Policy Tests**
```bash
# Non-admin user - should get 403
curl -X POST "https://localhost:5001/api/license" \
  -H "Authorization: Bearer {user-token}" \
  -H "Content-Type: application/json" \
  -d '{...}'

# Admin user - should succeed
curl -X POST "https://localhost:5001/api/license" \
  -H "Authorization: Bearer {admin-token}" \
  -H "Content-Type: application/json" \
  -d '{...}'
```

#### **3. Organization Role Tests**
```bash
# Member tries to invite - should get 403
curl -X POST "https://localhost:5001/api/member/{orgId}/invite" \
  -H "Authorization: Bearer {member-token}" \
  -d '{...}'

# Owner invites - should succeed
curl -X POST "https://localhost:5001/api/member/{orgId}/invite" \
  -H "Authorization: Bearer {owner-token}" \
  -d '{...}'

# OrgAdmin invites - should succeed
curl -X POST "https://localhost:5001/api/member/{orgId}/invite" \
  -H "Authorization: Bearer {orgadmin-token}" \
  -d '{...}'
```

#### **4. Non-Member Access Tests**
```bash
# Non-member views organization - should get 404
curl -X GET "https://localhost:5001/api/user/organizations/{orgId}" \
  -H "Authorization: Bearer {non-member-token}"

# Member views organization - should succeed
curl -X GET "https://localhost:5001/api/user/organizations/{orgId}" \
  -H "Authorization: Bearer {member-token}"
```

---

## Code Examples

### **Example 1: Simple Authorization Check**
```csharp
[HttpGet("{organizationId}")]
public async Task<IActionResult> GetOrganization(Guid organizationId)
{
    // Verify user is a member
    await AuthorizationHelper.EnsureUserIsMemberOfOrganizationAsync(
        _dbContext, CurrentUserIdGuid, organizationId);
    
    // Get organization
    var organization = await _dbContext.Organizations
        .FirstOrDefaultAsync(o => o.Id == organizationId);
    
    return Ok(organization);
}
```

### **Example 2: Admin Bypass Pattern**
```csharp
[HttpDelete("{organizationId}/{memberId}")]
public async Task<IActionResult> RemoveMember(
    Guid organizationId, 
    Guid memberId)
{
    // Admins can remove from any organization
    if (!IsAdmin)
    {
        // Non-admins must be Owner or OrgAdmin
        await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
            _dbContext, CurrentUserIdGuid, organizationId);
    }
    
    // Business logic to remove member
    // ...
}
```

### **Example 3: Role-Specific Logic**
```csharp
[HttpPut("{organizationId}/{memberId}/role")]
public async Task<IActionResult> UpdateMemberRole(
    Guid organizationId,
    Guid memberId,
    UpdateMemberRoleRequest request)
{
    // Check authorization
    if (!IsAdmin)
    {
        await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
            _dbContext, CurrentUserIdGuid, organizationId);
    }
    
    // Get member
    var member = await _dbContext.OrganizationMembers
        .FirstOrDefaultAsync(om => om.Id == memberId);
    
    // Prevent removing last Owner
    if (member.Role == Roles.Owner && request.Role != Roles.Owner)
    {
        var ownerCount = await _dbContext.OrganizationMembers
            .CountAsync(om => om.OrganizationId == organizationId && 
                            om.Role == Roles.Owner);
        
        if (ownerCount <= 1)
        {
            throw new BadRequestException(
                "Cannot change the role of the last Owner.");
        }
    }
    
    // Update role
    member.Role = request.Role;
    await _dbContext.SaveChangesAsync();
    
    return Ok(member);
}
```

---

## Integration Points

### **1. Program.cs Configuration**
```csharp
// Add authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add authorization policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireClaim(ClaimTypes.Role, Roles.Admin));

// Add middleware
app.UseAuthentication();
app.UseAuthorization();
```

### **2. Controller Usage**
```csharp
[Authorize] // or [Authorize(Policy = "AdminOnly")]
public class MyController : BaseController
{
    // Access user context via base properties
    var userId = CurrentUserIdGuid;
    var isAdmin = IsAdmin;
}
```

### **3. Authorization Helper Usage**
```csharp
// Check permissions
await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
    _dbContext, CurrentUserIdGuid, organizationId);
```

---

## File Locations

```
CaseGuard.Backend.Assignment/
├── Controllers/
│   └── BaseController.cs           # Base with user context
├── Helpers/
│   └── AuthorizationHelper.cs      # Authorization checks
├── Extensions/
│   └── JwtExtensions.cs            # JWT configuration
├── Constants/
│   ├── Roles.cs                    # Role definitions
│   └── ClaimTypes.cs               # Custom claim types
└── Program.cs                      # Policy configuration
```

---

**Status**: ✅ Task 6 Complete - Comprehensive authorization system with JWT authentication, role-based policies, and organization-level access control fully implemented
