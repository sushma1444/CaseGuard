# Task 5: Implement Regular User Endpoints - Documentation

## Overview
Complete implementation of Regular User endpoints in the `UserController` for managing user organizations, accepting invitations, and leaving organizations.

---

## Controller Information

**Controller:** `UserController`  
**Base Route:** `/api/user`  
**Authorization:** `[Authorize]` - All endpoints require authenticated users  
**Dependencies:**
- `ApplicationDbContext` - Database access
- `ILogger<UserController>` - Logging

---

## Regular User Stories Implementation

### ✅ **1. View All Organizations I'm a Member Of**

**Endpoint:** `GET /api/user/organizations`

**Authorization:** Any authenticated user

**Query Parameters:**
```
Pagination:
- page: int (default: 1, min: 1)
- pageSize: int (default: 10, min: 1, max: 100)

Filtering:
- role: string (optional: Owner | OrganizationAdmin | Member)
- searchTerm: string (optional - searches organization name and description)

Sorting:
- sortBy: string (optional: name | joinedat | role)
- sortDirection: string (optional: asc | desc, default: asc)
```

**Example Request:**
```
GET /api/user/organizations?page=1&pageSize=10&role=Member&sortBy=joinedat&sortDirection=desc
```

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "guid",
      "name": "Acme Corporation",
      "description": "Leading software company",
      "role": "Member",
      "joinedAt": "2026-01-10T10:00:00Z",
      "createdAt": "2026-01-01T10:00:00Z",
      "memberCount": 25,
      "activeLicenseCount": 5,
      "userAssignedLicenseCount": 2
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 3,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

**Response Fields:**
- `id` - Organization ID
- `name` - Organization name
- `description` - Organization description (optional)
- `role` - User's role in this organization
- `joinedAt` - When user joined the organization
- `createdAt` - When organization was created
- `memberCount` - Total members in organization
- `activeLicenseCount` - Total active licenses for organization
- `userAssignedLicenseCount` - Number of licenses assigned to this user in this organization

**Business Logic:**
1. Gets current user ID from JWT claims
2. Queries user's organization memberships
3. Applies role filter if provided
4. Applies search term to organization name/description
5. Sorts by specified field and direction
6. Paginates results
7. Calculates aggregate data (member counts, license counts)
8. Returns paginated list with metadata

**Features:**
- ✅ Pagination support
- ✅ Filter by user's role in organization
- ✅ Search by organization name or description
- ✅ Sort by name, joinedAt, or role
- ✅ Shows member count per organization
- ✅ Shows active license count per organization
- ✅ Shows user's assigned license count per organization
- ✅ Default sort: newest membership first (joinedAt desc)

**Error Responses:**
- `400 Bad Request` - Invalid query parameters
- `401 Unauthorized` - User not authenticated

**Use Cases:**
- User wants to see all organizations they belong to
- User wants to find organizations by role
- User wants to search for specific organization
- User wants to see where they have licenses assigned

---

### ✅ **2. View Details of a Specific Organization I Belong To**

**Endpoint:** `GET /api/user/organizations/{organizationId}`

**Authorization:** User must be a member of the organization

**Response:** `200 OK`
```json
{
  "id": "guid",
  "name": "Acme Corporation",
  "description": "Leading software company",
  "role": "Member",
  "joinedAt": "2026-01-10T10:00:00Z",
  "createdAt": "2026-01-01T10:00:00Z",
  "memberCount": 25,
  "activeLicenseCount": 5,
  "userAssignedLicenseCount": 2
}
```

**Business Logic:**
1. Gets current user ID from JWT claims
2. Queries user's membership in specified organization
3. Returns 404 if user is not a member
4. Gets organization details
5. Calculates member count
6. Calculates active license count
7. Calculates user's assigned license count
8. Returns organization details with user context

**Use Cases:**
- User wants to see details of a specific organization
- User wants to check their role in organization
- User wants to see how many licenses they have
- User wants to see organization member count

**Error Responses:**
- `404 Not Found` - Organization not found or user not a member
- `401 Unauthorized` - User not authenticated

---

### ✅ **3. Accept an Invitation to Join an Organization**

**Endpoint:** `POST /api/user/invitations/accept`

**Authorization:** Any authenticated user

**Request Body:**
```json
{
  "invitationId": "guid"
}
```

**Response:** `200 OK`
```json
{
  "organizationId": "guid",
  "organizationName": "Acme Corporation",
  "role": "Member",
  "acceptedAt": "2026-01-15T10:00:00Z"
}
```

**Business Logic:**
1. Gets invitation by ID
2. Validates invitation exists
3. **Validates invitation is for user's email** (case-insensitive)
4. Validates invitation is Pending (not Accepted, Cancelled, or Expired)
5. Validates invitation has not expired
6. Checks user is not already a member
7. Gets or creates User record in database
8. Creates OrganizationMember record with invitation role
9. Updates invitation status to Accepted
10. Records AcceptedAt timestamp
11. Links invitation to user
12. Returns acceptance confirmation

**Validation Rules:**
- Invitation must exist
- Invitation email must match user's JWT email claim
- Invitation must be in Pending status
- Invitation must not be expired (ExpiresAt > now)
- User cannot already be a member of the organization
- InvitationId is required

**User Creation:**
If user doesn't exist in database:
- Creates new User record
- Uses JWT userId as ID
- Uses JWT email as email
- Generates default name from email (part before @)
- Sets CreatedAt and UpdatedAt timestamps

If user exists:
- Updates email if different from JWT claim
- Updates UpdatedAt timestamp

**Side Effects:**
- User record created or updated
- OrganizationMember record created
- Invitation status changed to Accepted
- AcceptedAt timestamp recorded
- Invitation.UserId linked to user

**Error Responses:**
- `400 Bad Request` - Invalid invitation, wrong email, already member, expired
- `404 Not Found` - Invitation not found
- `401 Unauthorized` - User not authenticated

**Use Cases:**
- User receives invitation email
- User clicks accept link
- User joins organization with specified role
- User gains access to organization resources

---

### ✅ **4. Leave an Organization I'm Part Of**

**Endpoint:** `DELETE /api/user/organizations/{organizationId}`

**Authorization:** User must be a member of the organization

**Response:** `204 No Content`

**Business Logic:**
1. Gets current user ID from JWT claims
2. Queries user's membership in organization
3. Returns 404 if not a member
4. **Prevents Owner from leaving** (must transfer ownership or delete org)
5. Finds all active license assignments for user in organization
6. Unassigns all licenses (sets UnassignedAt timestamp)
7. Deletes OrganizationMember record
8. Returns 204 No Content

**Validation Rules:**
- User must be a member of the organization
- **User cannot be Owner** - Owners cannot leave
- Organization must exist

**Owner Restriction:**
Owners cannot leave organizations because:
- Organization needs at least one Owner
- Owner has full control and should explicitly transfer ownership
- Owner should delete organization if they want to remove it
- Prevents orphaned organizations

**Side Effects:**
- All active license assignments unassigned
- OrganizationMember record deleted
- User loses all access to organization
- Historical license assignment data preserved (via UnassignedAt)

**Error Responses:**
- `400 Bad Request` - User is Owner and cannot leave
- `404 Not Found` - Organization not found or user not a member
- `401 Unauthorized` - User not authenticated

**Use Cases:**
- User no longer wants to be part of organization
- User was added by mistake
- User moving to different organization
- Cleanup of old memberships

**Alternative Actions for Owners:**
- Transfer ownership to another member first, then leave
- Delete the entire organization
- Promote another member to Owner, demote self to Member, then leave

---

## Authorization & Security

### **Authentication Required**
All endpoints require valid JWT token with:
```json
{
  "userId": "user-guid",
  "email": "user@example.com",
  "role": "User" // or "Admin"
}
```

### **Authorization Checks**

#### **View Organizations**
- No additional authorization needed
- User can only see their own organizations
- Filter automatically applied by UserId

#### **View Specific Organization**
- User must be a member of the organization
- Returns 404 if not a member (doesn't reveal existence)

#### **Accept Invitation**
- Invitation email must match user's JWT email
- Prevents users from accepting invitations meant for others
- Case-insensitive email comparison

#### **Leave Organization**
- User must be a member
- User cannot be Owner
- Soft check prevents accidental orphaning

### **Security Features**
- ✅ Users can only access their own data
- ✅ Email validation on invitation acceptance
- ✅ Organization existence hidden from non-members
- ✅ Owner protection prevents orphaned organizations
- ✅ Historical data preserved on license unassignment

---

## Error Handling

### **Custom Exceptions Used**
```csharp
BadRequestException - Validation errors, business rules (400)
NotFoundException - Organization or invitation not found (404)
UnauthorizedException - Authentication failure (401)
```

### **Global Exception Handler**
All exceptions converted to ProblemDetails format:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "This invitation is not for your email address.",
  "traceId": "trace-id"
}
```

### **Common Error Scenarios**

**Accept Invitation Errors:**
```json
// Wrong email
{
  "status": 400,
  "detail": "This invitation is not for your email address."
}

// Already member
{
  "status": 400,
  "detail": "You are already a member of this organization."
}

// Expired invitation
{
  "status": 400,
  "detail": "This invitation has expired."
}

// Not pending
{
  "status": 400,
  "detail": "Invitation is not pending. Current status: Accepted."
}
```

**Leave Organization Errors:**
```json
// Owner cannot leave
{
  "status": 400,
  "detail": "Organization Owner cannot leave the organization. Please transfer ownership or delete the organization instead."
}

// Not a member
{
  "status": 404,
  "detail": "Organization with ID 'guid' was not found."
}
```

---

## Database Operations

### **Queries Used**

#### **Get User Organizations**
```csharp
_dbContext.OrganizationMembers
  .Include(om => om.Organization)
  .Where(om => om.UserId == currentUserId)
  .OrderByDescending(om => om.JoinedAt)
  .Skip(skip)
  .Take(pageSize)
```

#### **Accept Invitation**
```csharp
// Get invitation
_dbContext.Invitations
  .Include(i => i.Organization)
  .FirstOrDefaultAsync(i => i.Id == invitationId)

// Check existing membership
_dbContext.OrganizationMembers
  .FirstOrDefaultAsync(om => om.UserId == userId && om.OrganizationId == orgId)

// Get or create user
_dbContext.Users
  .FirstOrDefaultAsync(u => u.Id == userId)
```

#### **Leave Organization**
```csharp
// Get membership
_dbContext.OrganizationMembers
  .Include(om => om.Organization)
  .FirstOrDefaultAsync(om => om.UserId == userId && om.OrganizationId == orgId)

// Get license assignments
_dbContext.LicenseAssignments
  .Include(la => la.License)
  .Where(la => la.UserId == userId && 
              la.License.OrganizationId == orgId &&
              la.UnassignedAt == null)
```

### **Aggregate Queries**
Batch queries used for performance:
```csharp
// Member counts per organization
_dbContext.OrganizationMembers
  .GroupBy(om => om.OrganizationId)
  .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
  .ToDictionaryAsync()

// Active license counts per organization
_dbContext.Licenses
  .Where(l => l.IsActive && l.IsValid)
  .GroupBy(l => l.OrganizationId)
  .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
  .ToDictionaryAsync()

// User's license counts per organization
_dbContext.LicenseAssignments
  .Where(la => la.UserId == userId && la.UnassignedAt == null)
  .GroupBy(la => la.License.OrganizationId)
  .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
  .ToDictionaryAsync()
```

---

## Business Rules

### **Invitation Acceptance**
1. Invitation must be addressed to user's email (case-insensitive match)
2. Invitation must be in Pending status
3. Invitation must not be expired (ExpiresAt > now)
4. User cannot already be a member of the organization
5. User record created if doesn't exist in database
6. User gains role specified in invitation

### **Leaving Organization**
1. Only Members and OrganizationAdmins can leave
2. Owners cannot leave (must transfer ownership or delete org)
3. All active licenses automatically unassigned
4. Membership deleted (not soft-deleted)
5. Historical license assignment data preserved

### **Organization Visibility**
1. Users can only see organizations they belong to
2. Non-members receive 404 (not 403) to hide existence
3. Organization details include user's role and context
4. Aggregate counts calculated per-organization

---

## Logging

### **Audit Trail Events**
```csharp
// Accept invitation
LogInformation("User {UserId} accepted invitation {InvitationId} to join organization {OrganizationId}")

// Leave organization
LogInformation("User {UserId} left organization {OrganizationId}")

// Error logging
LogError(ex, "Error retrieving user organizations")
LogError(ex, "Error retrieving organization {OrganizationId} for user {UserId}")
LogError(ex, "Error accepting invitation {InvitationId} for user {UserId}")
LogError(ex, "Error leaving organization {OrganizationId} for user {UserId}")
```

**Logged Information:**
- User ID (from JWT claims)
- Organization ID
- Invitation ID
- Action performed
- Timestamp (automatic)
- Error details (if applicable)

---

## Performance Considerations

### **Query Optimization**
- ✅ Eager loading with `.Include()` avoids N+1 queries
- ✅ Batch aggregate queries for counts
- ✅ Single query per operation where possible
- ✅ Indexes on UserId and OrganizationId foreign keys

### **Pagination**
- ✅ Max page size enforced (100 items)
- ✅ Total count query before pagination
- ✅ Skip/Take for efficient pagination
- ✅ Default page size: 10 items

### **Aggregate Data**
- ✅ Member counts calculated in batch (dictionary lookup)
- ✅ License counts calculated in batch
- ✅ User license counts calculated separately
- ✅ All counts done in single database round-trip

---

## Testing Scenarios

### **Happy Path**
1. ✅ User views all their organizations
2. ✅ User views specific organization details
3. ✅ User accepts invitation and joins organization
4. ✅ User leaves organization (as Member or OrgAdmin)

### **Authorization Tests**
1. ✅ Non-member cannot view organization details → 404
2. ✅ User cannot accept invitation for different email → 400
3. ✅ Owner cannot leave organization → 400

### **Validation Tests**
1. ✅ Cannot accept expired invitation → 400
2. ✅ Cannot accept already-accepted invitation → 400
3. ✅ Cannot accept invitation if already member → 400
4. ✅ Cannot leave organization not a member of → 404

### **Edge Cases**
1. ✅ New user accepting first invitation (auto-creates user)
2. ✅ User with no organizations returns empty list
3. ✅ Leaving organization unassigns all licenses
4. ✅ Expired invitation marked as Expired on access attempt
5. ✅ Pagination with single organization
6. ✅ Filtering by role with no matches

---

## API Examples

### **View User's Organizations**
```bash
curl -X GET "https://localhost:5001/api/user/organizations?page=1&pageSize=10" \
  -H "Authorization: Bearer {user-token}"
```

### **View Specific Organization**
```bash
curl -X GET "https://localhost:5001/api/user/organizations/{org-guid}" \
  -H "Authorization: Bearer {user-token}"
```

### **Accept Invitation**
```bash
curl -X POST "https://localhost:5001/api/user/invitations/accept" \
  -H "Authorization: Bearer {user-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "invitationId": "invitation-guid"
  }'
```

### **Leave Organization**
```bash
curl -X DELETE "https://localhost:5001/api/user/organizations/{org-guid}" \
  -H "Authorization: Bearer {user-token}"
```

### **Search Organizations by Name**
```bash
curl -X GET "https://localhost:5001/api/user/organizations?searchTerm=acme&sortBy=name" \
  -H "Authorization: Bearer {user-token}"
```

### **Filter by Role**
```bash
curl -X GET "https://localhost:5001/api/user/organizations?role=Member&sortBy=joinedat&sortDirection=desc" \
  -H "Authorization: Bearer {user-token}"
```

---

## Integration with Other Components

### **Invitation Flow**
1. Organization Owner sends invitation (MemberController)
2. User receives email notification (external system)
3. **User accepts invitation** (UserController) ← This endpoint
4. User becomes organization member
5. User can view organization (UserController)

### **License Assignment Flow**
1. Organization Owner assigns license (LicenseAssignmentController)
2. **User sees license count in organization list** (UserController)
3. User leaves organization
4. **Licenses automatically unassigned** (UserController)

### **Organization Lifecycle**
1. User creates organization (OrganizationController) - becomes Owner
2. **User views organization in their list** (UserController)
3. Owner invites members (MemberController)
4. Members accept and join (UserController)
5. **Members can leave** (UserController)
6. Owner deletes organization (OrganizationController)

---

## Data Flow Diagrams

### **Accept Invitation Flow**
```
User (JWT) → POST /api/user/invitations/accept
    ↓
Validate invitation exists
    ↓
Validate email matches user
    ↓
Validate invitation pending & not expired
    ↓
Check not already member
    ↓
Get or create User record
    ↓
Create OrganizationMember
    ↓
Update Invitation (Accepted, AcceptedAt)
    ↓
Return success response
```

### **Leave Organization Flow**
```
User (JWT) → DELETE /api/user/organizations/{id}
    ↓
Validate user is member
    ↓
Prevent if user is Owner
    ↓
Find active license assignments
    ↓
Set UnassignedAt on all licenses
    ↓
Delete OrganizationMember
    ↓
Return 204 No Content
```

---

## File Location

```
CaseGuard.Backend.Assignment/
└── Controllers/
    └── UserController.cs
```

---

## Related Documentation

- [Task 1: Database Schema](Task1_Database_Schema_Design.md) - Entity definitions
- [Task 2: DTOs](Task2_Request_Response_DTOs.md) - Request/Response contracts
- [Task 4: Organization Endpoints](Task4_Organization_Owner_Admin_Endpoints.md) - Invitation creation
- [Task 6: Authorization](Task6_Authorization_Implementation.md) - Authorization helpers

---

**Status**: ✅ Task 5 Complete - All 4 Regular User user stories fully implemented with pagination, filtering, comprehensive validation, and proper authorization
