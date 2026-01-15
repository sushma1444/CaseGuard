# Task 4: Implement Organization Owner/Admin Endpoints - Documentation

## Overview
Complete implementation of Organization Owner and Admin endpoints across 4 controllers for managing organizations, members, invitations, and license assignments.

---

## Controllers Overview

| Controller | Base Route | Authorization | Purpose |
|------------|------------|---------------|---------|
| OrganizationController | `/api/organization` | Authenticated users | Organization CRUD operations |
| MemberController | `/api/member` | Owner/OrgAdmin | Member management |
| InvitationController | `/api/invitation` | Owner/OrgAdmin | Invitation management |
| LicenseAssignmentController | `/api/licenseassignment` | Owner/OrgAdmin | License assignment operations |

---

## OrganizationController

### ✅ **1. Create a New Organization**

**Endpoint:** `POST /api/organization`

**Authorization:** Any authenticated user

**Request Body:**
```json
{
  "name": "Acme Corporation",
  "description": "Leading software company"
}
```

**Response:** `201 Created`
```json
{
  "id": "guid",
  "name": "Acme Corporation",
  "description": "Leading software company",
  "createdAt": "2026-01-15T10:00:00Z",
  "updatedAt": "2026-01-15T10:00:00Z",
  "memberCount": 1,
  "activeLicenseCount": 0,
  "currentUserRole": "Owner"
}
```

**Business Logic:**
1. Validates user is authenticated
2. Checks organization name is unique
3. Creates organization entity
4. Auto-creates OrganizationMember with Role = "Owner" for creator
5. Returns organization with creator as Owner

**Validation:**
- Name is required (max 200 characters)
- Description is optional (max 1000 characters)
- Organization name must be unique (case-insensitive)

**Error Responses:**
- `400 Bad Request` - Invalid input or duplicate name
- `401 Unauthorized` - User not authenticated

---

### ✅ **2. Update Organization Details**

**Endpoint:** `PUT /api/organization/{id}`

**Authorization:** Owner or OrganizationAdmin of the organization

**Request Body:** (All fields optional)
```json
{
  "name": "Acme Corp Ltd",
  "description": "Updated description"
}
```

**Response:** `200 OK`
```json
{
  "id": "guid",
  "name": "Acme Corp Ltd",
  "description": "Updated description",
  "createdAt": "2026-01-15T10:00:00Z",
  "updatedAt": "2026-01-15T10:05:00Z",
  "memberCount": 5,
  "activeLicenseCount": 2,
  "currentUserRole": "Owner"
}
```

**Business Logic:**
1. Verifies organization exists
2. Checks user is Owner or OrganizationAdmin
3. Updates provided fields only
4. Updates UpdatedAt timestamp
5. Returns updated organization

**Error Responses:**
- `400 Bad Request` - Invalid input
- `403 Forbidden` - User not authorized
- `404 Not Found` - Organization not found

---

### ✅ **3. Delete My Organization**

**Endpoint:** `DELETE /api/organization/{id}`

**Authorization:** Owner or OrganizationAdmin of the organization

**Response:** `204 No Content`

**Business Logic:**
1. Verifies organization exists
2. Checks user is Owner or OrganizationAdmin
3. Deletes organization (cascade deletes members, licenses, invitations)
4. Removes all related data

**Cascade Effects:**
- All OrganizationMembers deleted
- All Licenses deleted
- All LicenseAssignments deleted
- All Invitations deleted

**Error Responses:**
- `403 Forbidden` - User not authorized
- `404 Not Found` - Organization not found

---

## MemberController

### ✅ **4. Invite Users to Organization (via Email)**

**Endpoint:** `POST /api/member/{organizationId}/invite`

**Authorization:** Owner or OrganizationAdmin of the organization

**Request Body:**
```json
{
  "email": "user@example.com",
  "role": "Member"
}
```

**Response:** `201 Created`
```json
{
  "invitationId": "guid",
  "email": "user@example.com",
  "organizationId": "guid",
  "organizationName": "Acme Corp",
  "role": "Member",
  "status": "Pending",
  "expiresAt": "2026-01-22T10:00:00Z"
}
```

**Business Logic:**
1. Verifies organization exists
2. Checks user authorization
3. Validates email not already a member
4. Checks for existing pending invitations
5. Creates invitation with 7-day expiration
6. Returns invitation details

**Validation:**
- Email is required and must be valid format
- Role must be: Owner, OrganizationAdmin, or Member
- User cannot already be a member
- No pending invitation for same email

**Error Responses:**
- `400 Bad Request` - User already member or duplicate invitation
- `403 Forbidden` - User not authorized
- `404 Not Found` - Organization not found
- `409 Conflict` - Duplicate invitation

---

### ✅ **5. Remove Users from Organization**

**Endpoint:** `DELETE /api/member/{organizationId}/{memberId}`

**Authorization:** Owner or OrganizationAdmin of the organization

**Response:** `204 No Content`

**Business Logic:**
1. Verifies organization and member exist
2. Checks user authorization
3. Prevents removing the last Owner
4. Unassigns all active licenses
5. Deletes OrganizationMember record

**Validation:**
- Cannot remove last Owner from organization
- Member must exist in organization
- Authorization required

**Side Effects:**
- All license assignments unassigned
- Member loses all access to organization
- Historical data preserved in LicenseAssignments

**Error Responses:**
- `400 Bad Request` - Attempting to remove last Owner
- `403 Forbidden` - User not authorized
- `404 Not Found` - Organization or member not found

---

### ✅ **6. Assign License to User**

**Endpoint:** `POST /api/licenseassignment`

**Authorization:** Owner or OrganizationAdmin of the license's organization

**Request Body:**
```json
{
  "licenseId": "guid",
  "userId": "guid"
}
```

**Response:** `201 Created`
```json
{
  "id": "guid",
  "licenseId": "guid",
  "licenseName": "Premium License",
  "userId": "guid",
  "userEmail": "user@example.com",
  "userName": "John Doe",
  "organizationId": "guid",
  "assignedAt": "2026-01-15T10:00:00Z",
  "unassignedAt": null,
  "isActive": true
}
```

**Business Logic:**
1. Verifies license exists and is valid
2. Verifies user exists
3. Checks user is member of license's organization
4. Validates no duplicate active assignment
5. Creates LicenseAssignment record

**Validation:**
- License must be valid (not expired, IsActive = true)
- User must be organization member
- No duplicate active assignments
- Authorization required

**Error Responses:**
- `400 Bad Request` - Invalid license or user not member
- `403 Forbidden` - User not authorized
- `404 Not Found` - License or user not found
- `409 Conflict` - License already assigned

---

### ✅ **7. Unassign License from User**

**Endpoint:** `DELETE /api/licenseassignment/{id}`

**Authorization:** Owner or OrganizationAdmin of the license's organization

**Response:** `204 No Content`

**Business Logic:**
1. Verifies assignment exists
2. Checks user authorization
3. Sets UnassignedAt timestamp (soft delete)
4. Preserves historical record

**Side Effects:**
- Assignment becomes inactive
- User loses license access
- Historical data preserved for audit

**Error Responses:**
- `403 Forbidden` - User not authorized
- `404 Not Found` - Assignment not found

---

### ✅ **8. View All Members in Organization**

**Endpoint:** `GET /api/member/{organizationId}`

**Authorization:** Member of the organization (any role)

**Query Parameters:**
```
Pagination:
- page: int (default: 1)
- pageSize: int (default: 10, max: 100)

Filtering:
- role: string (Owner | OrganizationAdmin | Member)
- emailFilter: string (partial match)
- searchTerm: string (searches name and email)

Sorting:
- sortBy: string (email | name | role | joinedat)
- sortDirection: string (asc | desc, default: asc)
```

**Example Request:**
```
GET /api/member/org-guid?page=1&pageSize=10&role=Member&sortBy=joinedat&sortDirection=desc
```

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "guid",
      "userId": "guid",
      "email": "user@example.com",
      "name": "John Doe",
      "role": "Member",
      "joinedAt": "2026-01-10T10:00:00Z",
      "assignedLicenseCount": 2,
      "hasActiveLicense": true
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

**Features:**
- ✅ Pagination
- ✅ Filter by role
- ✅ Filter by email (partial match)
- ✅ Search by name or email
- ✅ Sort by multiple fields
- ✅ Shows license counts per member
- ✅ Shows active license status

**Error Responses:**
- `400 Bad Request` - Invalid query parameters
- `403 Forbidden` - User not member of organization
- `404 Not Found` - Organization not found

---

### ✅ **9. View Details of Specific Member**

**Endpoint:** `GET /api/member/{organizationId}/{memberId}`

**Authorization:** Member of the organization

**Response:** `200 OK`
```json
{
  "id": "guid",
  "userId": "guid",
  "email": "user@example.com",
  "name": "John Doe",
  "role": "Member",
  "joinedAt": "2026-01-10T10:00:00Z",
  "assignedLicenseCount": 2,
  "hasActiveLicense": true
}
```

**Business Logic:**
1. Verifies organization exists
2. Checks user is member
3. Gets member details with license info
4. Returns member data

**Error Responses:**
- `403 Forbidden` - User not member
- `404 Not Found` - Organization or member not found

---

### ✅ **10. Update Member's Role**

**Endpoint:** `PUT /api/member/{organizationId}/{memberId}/role`

**Authorization:** Owner or OrganizationAdmin of the organization

**Request Body:**
```json
{
  "role": "OrganizationAdmin"
}
```

**Response:** `200 OK`
```json
{
  "id": "guid",
  "userId": "guid",
  "email": "user@example.com",
  "name": "John Doe",
  "role": "OrganizationAdmin",
  "joinedAt": "2026-01-10T10:00:00Z",
  "assignedLicenseCount": 2,
  "hasActiveLicense": true
}
```

**Business Logic:**
1. Verifies organization and member exist
2. Checks user authorization
3. Validates new role value
4. Prevents removing last Owner
5. Updates member role

**Validation:**
- Role must be: Owner, OrganizationAdmin, or Member
- Cannot change last Owner's role
- Authorization required

**Error Responses:**
- `400 Bad Request` - Invalid role or removing last Owner
- `403 Forbidden` - User not authorized
- `404 Not Found` - Organization or member not found

---

## InvitationController

### ✅ **11. View All Pending Invitations**

**Endpoint:** `GET /api/invitation/{organizationId}`

**Authorization:** Owner or OrganizationAdmin of the organization

**Query Parameters:**
```
Pagination:
- page: int (default: 1)
- pageSize: int (default: 10, max: 100)

Filtering:
- status: string (Pending | Accepted | Cancelled | Expired)
- emailFilter: string (partial match)

Sorting:
- sortBy: string (email | createdat | expiresat | status)
- sortDirection: string (asc | desc, default: asc)
```

**Example Request:**
```
GET /api/invitation/org-guid?status=Pending&sortBy=expiresat&sortDirection=asc
```

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "guid",
      "organizationId": "guid",
      "organizationName": "Acme Corp",
      "email": "user@example.com",
      "role": "Member",
      "status": "Pending",
      "expiresAt": "2026-01-22T10:00:00Z",
      "createdAt": "2026-01-15T10:00:00Z",
      "acceptedAt": null,
      "cancelledAt": null
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 5,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

**Features:**
- ✅ Pagination
- ✅ Filter by status
- ✅ Filter by email
- ✅ Sort by multiple fields
- ✅ Shows expiration info

**Error Responses:**
- `400 Bad Request` - Invalid query parameters
- `403 Forbidden` - User not authorized
- `404 Not Found` - Organization not found

---

### ✅ **12. View Details of Specific Invitation**

**Endpoint:** `GET /api/invitation/{organizationId}/{invitationId}`

**Authorization:** Owner or OrganizationAdmin of the organization

**Response:** `200 OK`
```json
{
  "id": "guid",
  "organizationId": "guid",
  "organizationName": "Acme Corp",
  "email": "user@example.com",
  "role": "Member",
  "status": "Pending",
  "expiresAt": "2026-01-22T10:00:00Z",
  "createdAt": "2026-01-15T10:00:00Z",
  "acceptedAt": null,
  "cancelledAt": null
}
```

**Error Responses:**
- `403 Forbidden` - User not authorized
- `404 Not Found` - Organization or invitation not found

---

### ✅ **13. Cancel Pending Invitation**

**Endpoint:** `DELETE /api/invitation/{organizationId}/{invitationId}`

**Authorization:** Owner or OrganizationAdmin of the organization

**Response:** `204 No Content`

**Business Logic:**
1. Verifies organization and invitation exist
2. Checks user authorization
3. Validates invitation is still pending
4. Sets Status to Cancelled
5. Records CancelledAt timestamp

**Validation:**
- Invitation must exist
- Invitation must be Pending (cannot cancel Accepted/Expired)
- Authorization required

**Error Responses:**
- `400 Bad Request` - Invitation not pending
- `403 Forbidden` - User not authorized
- `404 Not Found` - Organization or invitation not found

---

## Authorization System

### **Authorization Helper Methods**

#### **EnsureUserIsOwnerOrAdminOfOrganizationAsync**
```csharp
// Checks if user is Owner or OrganizationAdmin
// Throws ForbiddenException if not authorized
// Used for: Organization management, member management, license assignment
```

#### **EnsureUserIsMemberOfOrganizationAsync**
```csharp
// Checks if user is any member of organization (any role)
// Throws ForbiddenException if not a member
// Used for: Viewing members, viewing organization details
```

### **Admin Bypass**
System admins (role = "Admin") can bypass organization-level authorization checks and access any organization.

### **Role Hierarchy**
```
Admin (Global)
├── Can access all organizations
└── Can perform all operations

Owner (Organization-specific)
├── Can manage organization
├── Can manage members
├── Can assign/unassign licenses
└── Can invite and remove members

OrganizationAdmin (Organization-specific)
├── Can manage members
├── Can assign/unassign licenses
└── Can invite and remove members

Member (Organization-specific)
└── Can view organization and members
```

---

## Common Features Across Endpoints

### **Pagination**
All list endpoints support:
- `page` - Page number (1-based, default: 1)
- `pageSize` - Items per page (1-100, default: 10)

Response includes:
- `totalCount` - Total items matching query
- `totalPages` - Calculated page count
- `hasNextPage` - Boolean flag
- `hasPreviousPage` - Boolean flag

### **Filtering**
Context-specific filters available per endpoint:
- Members: role, emailFilter
- Invitations: status, emailFilter
- Organizations: by user membership

### **Sorting**
All list endpoints support:
- `sortBy` - Field name to sort by
- `sortDirection` - "asc" or "desc" (default: "asc")
- Default sorting varies by endpoint (usually newest first)

### **Search**
Text search available on:
- Members: searches name and email
- Invitations: searches email
- Organizations: searches name and description

---

## Error Handling

### **Custom Exceptions**
```csharp
BadRequestException - Validation errors, business rule violations (400)
NotFoundException - Resource not found (404)
ForbiddenException - Authorization failure (403)
UnauthorizedException - Authentication failure (401)
```

### **Global Exception Handler**
All exceptions converted to ProblemDetails format:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to manage members of this organization.",
  "traceId": "trace-id"
}
```

---

## Validation Rules

### **Organization**
- Name: Required, max 200 characters, unique
- Description: Optional, max 1000 characters

### **Invitation**
- Email: Required, valid email format, max 256 characters
- Role: Required, one of: Owner, OrganizationAdmin, Member
- Expiration: Default 7 days from creation

### **Member**
- Role: One of: Owner, OrganizationAdmin, Member
- Cannot remove last Owner
- Cannot change role of last Owner

### **License Assignment**
- License must be valid and active
- User must be organization member
- No duplicate active assignments

---

## Business Rules

### **Organization Creation**
- Creator automatically becomes Owner
- OrganizationMember record auto-created
- Organization starts with 1 member

### **Member Removal**
- Cannot remove last Owner
- All licenses automatically unassigned
- Historical data preserved

### **Role Changes**
- Cannot change last Owner's role
- Must be Owner or OrgAdmin to change roles
- Validates role values

### **Invitations**
- 7-day expiration by default
- Email must not be existing member
- No duplicate pending invitations
- Can only cancel Pending invitations

### **License Assignment**
- License must be valid (not expired, active)
- User must be organization member
- No duplicate active assignments
- Unassignment preserves history

---

## Logging

### **Audit Trail Events**
```csharp
// Organization operations
LogInformation("Organization {OrganizationId} created by user {UserId}")
LogInformation("Organization {OrganizationId} updated by user {UserId}")
LogInformation("Organization {OrganizationId} deleted by user {UserId}")

// Member operations
LogInformation("User {UserId} invited to organization {OrganizationId}")
LogInformation("Member {MemberId} removed from organization {OrganizationId}")
LogInformation("Member {MemberId} role updated to {Role}")

// Invitation operations
LogInformation("Invitation {InvitationId} cancelled by user {UserId}")

// License assignment operations
LogInformation("License {LicenseId} assigned to user {UserId}")
LogInformation("License assignment {AssignmentId} removed")

// Error logging
LogError(ex, "Error creating organization")
LogError(ex, "Error inviting member to organization {OrganizationId}")
```

---

## Testing Scenarios

### **Happy Path**
1. ✅ User creates organization (becomes Owner)
2. ✅ Owner invites new member via email
3. ✅ Owner views all members with pagination
4. ✅ Owner assigns license to member
5. ✅ Owner updates member role to OrganizationAdmin
6. ✅ Owner views pending invitations
7. ✅ Owner cancels invitation
8. ✅ Owner removes member (unassigns licenses)
9. ✅ Owner updates organization details
10. ✅ Owner deletes organization

### **Authorization Tests**
1. ✅ Non-member cannot view organization members → 403
2. ✅ Member cannot invite users (must be Owner/OrgAdmin) → 403
3. ✅ Member cannot assign licenses → 403
4. ✅ Member cannot update roles → 403

### **Validation Tests**
1. ✅ Cannot create org with duplicate name → 400
2. ✅ Cannot invite existing member → 400
3. ✅ Cannot remove last Owner → 400
4. ✅ Cannot change last Owner's role → 400
5. ✅ Cannot assign invalid license → 400
6. ✅ Cannot assign license to non-member → 400

### **Edge Cases**
1. ✅ Multiple invitations for different emails
2. ✅ Expired invitations filtered out
3. ✅ License assignment with expired license blocked
4. ✅ Pagination with large datasets
5. ✅ Filtering with multiple criteria

---

## Performance Considerations

### **Query Optimization**
- Eager loading with `.Include()` to avoid N+1
- Batch queries for license counts
- Indexes on foreign keys and filters

### **Pagination**
- Max page size enforced (100)
- Skip/Take for efficient pagination
- Total count query optimized

### **Caching Opportunities**
- Organization member counts
- Active license counts
- Role lookups (constants)

---

## API Examples

### **Create Organization and Invite Member**
```bash
# 1. Create organization
curl -X POST "https://localhost:5001/api/organization" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"name": "My Company", "description": "Company description"}'

# 2. Invite member
curl -X POST "https://localhost:5001/api/member/{orgId}/invite" \
  -H "Authorization: Bearer {owner-token}" \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "role": "Member"}'
```

### **Assign License to Member**
```bash
curl -X POST "https://localhost:5001/api/licenseassignment" \
  -H "Authorization: Bearer {owner-token}" \
  -H "Content-Type: application/json" \
  -d '{"licenseId": "license-guid", "userId": "user-guid"}'
```

### **View Members with Filtering**
```bash
curl -X GET "https://localhost:5001/api/member/{orgId}?role=Member&sortBy=joinedat&sortDirection=desc" \
  -H "Authorization: Bearer {token}"
```

---

## File Locations

```
CaseGuard.Backend.Assignment/
├── Controllers/
│   ├── OrganizationController.cs
│   ├── MemberController.cs
│   ├── InvitationController.cs
│   └── LicenseAssignmentController.cs
└── Helpers/
    └── AuthorizationHelper.cs
```

---

**Status**: ✅ Task 4 Complete - All 13 Organization Owner/Admin user stories fully implemented with authorization, pagination, filtering, and comprehensive error handling
