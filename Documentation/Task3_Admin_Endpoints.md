# Task 3: Implement Admin Endpoints - Documentation

## Overview
Complete implementation of Admin-only endpoints for license management in the `LicenseController`.

---

## Controller Information

**Controller:** `LicenseController`  
**Base Route:** `/api/license`  
**Authorization:** `[Authorize(Policy = "AdminOnly")]` - All endpoints require Admin role  
**Dependencies:**
- `ApplicationDbContext` - Database access
- `ILogger<LicenseController>` - Logging
- `ILicenseExpirationService` - License expiration logic

---

## Admin User Stories Implementation

### ✅ **1. Create a License for an Organization**

**Endpoint:** `POST /api/license`

**Request Body:**
```json
{
  "organizationId": "guid",
  "name": "Premium License",
  "startDate": "2026-01-15T10:00:00Z",  // Optional, defaults to now
  "expirationDate": "2026-01-15T10:10:00Z",  // Optional, defaults to startDate + 10 minutes
  "autoRenewalEnabled": false  // Optional, defaults to false
}
```

**Response:** `201 Created`
```json
{
  "id": "guid",
  "organizationId": "guid",
  "organizationName": "Acme Corp",
  "name": "Premium License",
  "startDate": "2026-01-15T10:00:00Z",
  "expirationDate": "2026-01-15T10:10:00Z",
  "autoRenewalEnabled": false,
  "isActive": true,
  "isValid": true,
  "createdAt": "2026-01-15T10:00:00Z",
  "updatedAt": "2026-01-15T10:00:00Z",
  "cancelledAt": null,
  "assignedUserCount": 0
}
```

**Validation Rules:**
- OrganizationId must exist
- Name is required (max 200 characters)
- ExpirationDate must be after StartDate
- Default expiration: 10 minutes from start date

**Error Responses:**
- `400 Bad Request` - Invalid input or validation failure
- `404 Not Found` - Organization not found
- `403 Forbidden` - User is not an admin

**Business Logic:**
1. Validates organization exists
2. Sets default dates if not provided
3. Validates expiration after start date
4. Creates license entity
5. Saves to database
6. Returns created license with details

---

### ✅ **2. View All Licenses in the System**

#### **Get Paginated List**
**Endpoint:** `GET /api/license`

**Query Parameters:**
```
Pagination:
- page: int (default: 1, min: 1)
- pageSize: int (default: 10, min: 1, max: 100)

Filtering:
- organizationId: guid (optional)
- isActive: bool (optional)
- autoRenewalEnabled: bool (optional)
- expirationStatus: string (optional: "expired" | "active" | "all")
- searchTerm: string (optional - searches name and organization name)

Sorting:
- sortBy: string (optional: "name" | "startdate" | "expirationdate" | "createdat" | "organizationname")
- sortDirection: string (optional: "asc" | "desc", default: "asc")
```

**Example Request:**
```
GET /api/license?page=1&pageSize=10&isActive=true&sortBy=expirationdate&sortDirection=asc
```

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "guid",
      "organizationId": "guid",
      "organizationName": "Acme Corp",
      "name": "Premium License",
      "startDate": "2026-01-15T10:00:00Z",
      "expirationDate": "2026-01-15T10:10:00Z",
      "autoRenewalEnabled": true,
      "isActive": true,
      "isValid": true,
      "createdAt": "2026-01-15T10:00:00Z",
      "updatedAt": "2026-01-15T10:00:00Z",
      "cancelledAt": null,
      "assignedUserCount": 5
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

**Features:**
- ✅ Pagination with page and pageSize
- ✅ Filter by organization, active status, auto-renewal
- ✅ Filter by expiration status (expired/active/all)
- ✅ Search by license name or organization name
- ✅ Sort by multiple fields with direction
- ✅ Default sort: newest first (CreatedAt desc)
- ✅ Includes assigned user counts per license
- ✅ Checks and invalidates expired licenses before querying

**Error Responses:**
- `400 Bad Request` - Invalid query parameters
- `403 Forbidden` - User is not an admin

#### **Get Single License**
**Endpoint:** `GET /api/license/{id}`

**Response:** `200 OK`
```json
{
  "id": "guid",
  "organizationId": "guid",
  "organizationName": "Acme Corp",
  "name": "Premium License",
  "startDate": "2026-01-15T10:00:00Z",
  "expirationDate": "2026-01-15T10:10:00Z",
  "autoRenewalEnabled": true,
  "isActive": true,
  "isValid": true,
  "createdAt": "2026-01-15T10:00:00Z",
  "updatedAt": "2026-01-15T10:00:00Z",
  "cancelledAt": null,
  "assignedUserCount": 5
}
```

**Error Responses:**
- `404 Not Found` - License not found
- `403 Forbidden` - User is not an admin

---

### ✅ **3. Update License Properties**

**Endpoint:** `PUT /api/license/{id}`

**Request Body:** (All fields optional)
```json
{
  "name": "Enterprise License",
  "expirationDate": "2026-01-15T12:00:00Z",
  "autoRenewalEnabled": true,
  "isActive": true
}
```

**Response:** `200 OK`
```json
{
  "id": "guid",
  "organizationId": "guid",
  "organizationName": "Acme Corp",
  "name": "Enterprise License",
  "startDate": "2026-01-15T10:00:00Z",
  "expirationDate": "2026-01-15T12:00:00Z",
  "autoRenewalEnabled": true,
  "isActive": true,
  "isValid": true,
  "createdAt": "2026-01-15T10:00:00Z",
  "updatedAt": "2026-01-15T10:05:00Z",
  "cancelledAt": null,
  "assignedUserCount": 5
}
```

**Updatable Properties:**
- `name` - Rename the license
- `expirationDate` - Extend or shorten expiration (must be after start date)
- `autoRenewalEnabled` - Enable/disable auto-renewal
- `isActive` - Activate/deactivate license

**Validation Rules:**
- ExpirationDate must be after StartDate
- At least one property should be provided

**Error Responses:**
- `400 Bad Request` - Invalid input or validation failure
- `404 Not Found` - License not found
- `403 Forbidden` - User is not an admin

**Use Cases:**
- Extend expiration before license expires
- Enable auto-renewal for active licenses
- Temporarily deactivate a license
- Rename license for clarity

---

### ✅ **4. Cancel or Revoke a License**

**Endpoint:** `DELETE /api/license/{id}`

**Response:** `204 No Content`

**Business Logic:**
1. Finds the license by ID
2. Sets `IsActive = false`
3. Records `CancelledAt` timestamp
4. Automatically unassigns all active license assignments
5. Updates all affected records in database

**Side Effects:**
- License becomes inactive
- All users lose access (assignments unassigned)
- Timestamp recorded for audit trail
- Cannot be reversed (must create new license)

**Error Responses:**
- `404 Not Found` - License not found
- `403 Forbidden` - User is not an admin

**Audit Trail:**
- Logs admin user ID who cancelled
- Records cancellation timestamp
- Preserves historical data

---

## Bonus Endpoint

### **Check and Invalidate Expired Licenses**

**Endpoint:** `POST /api/license/check-expiration`

**Response:** `200 OK`
```json
{
  "invalidatedCount": 3,
  "message": "Invalidated 3 expired license(s)."
}
```

**Purpose:**
- Manually trigger expiration check
- Useful for testing or administrative tasks
- Background service runs automatically, but this allows immediate check

**Business Logic:**
1. Queries all active licenses
2. Checks if ExpirationDate < CurrentTime
3. Sets expired licenses to IsActive = false
4. Returns count of invalidated licenses

---

## Authorization & Security

### **Admin-Only Policy**
```csharp
[Authorize(Policy = "AdminOnly")]
```

**Requirements:**
- User must be authenticated (valid JWT token)
- User's role claim must be "Admin"
- Configured in Program.cs authorization policies

**Enforcement:**
- Applied at controller level (all endpoints)
- Returns 401 Unauthorized if not authenticated
- Returns 403 Forbidden if not an admin

### **JWT Claims Required:**
```json
{
  "userId": "user-guid",
  "role": "Admin",
  "email": "admin@example.com"
}
```

---

## Error Handling

### **Custom Exceptions Used:**
- `NotFoundException` - When license or organization not found
- `BadRequestException` - For validation errors or invalid input

### **Global Exception Handler:**
All exceptions converted to ProblemDetails format:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Not Found",
  "status": 404,
  "detail": "License with ID 'guid' was not found.",
  "traceId": "trace-id"
}
```

---

## Database Operations

### **Entity Relationships Used:**
```csharp
License
├── Organization (one-to-many)
└── LicenseAssignments (one-to-many)
```

### **Queries:**
- `.Include(l => l.Organization)` - Eager load organization
- `.Where()` - Filtering
- `.OrderBy()` / `.OrderByDescending()` - Sorting
- `.Skip()` / `.Take()` - Pagination
- `.GroupBy()` - Count aggregations

### **Optimizations:**
- Batch loading assigned user counts
- Indexes used for filtering (OrganizationId, IsActive, AutoRenewalEnabled)
- Efficient pagination with Skip/Take

---

## Logging

### **Log Events:**
```csharp
// Success logs
LogInformation("License {LicenseId} created for organization {OrganizationId} by admin {AdminId}")
LogInformation("License {LicenseId} updated by admin {AdminId}")
LogInformation("License {LicenseId} cancelled by admin {AdminId}")
LogInformation("Expiration check completed by admin {AdminId}. Invalidated {Count} license(s).")

// Error logs
LogError(ex, "Error creating license for organization {OrganizationId}")
LogError(ex, "Error retrieving licenses")
LogError(ex, "Error updating license {LicenseId}")
LogError(ex, "Error cancelling license {LicenseId}")
```

**Audit Trail:**
- All admin actions logged with admin user ID
- Timestamps included automatically
- Error context preserved

---

## Testing Scenarios

### **Happy Path:**
1. ✅ Admin creates license for organization
2. ✅ Admin views all licenses with filtering
3. ✅ Admin gets specific license details
4. ✅ Admin extends expiration date
5. ✅ Admin enables auto-renewal
6. ✅ Admin cancels license

### **Error Cases:**
1. ✅ Create license for non-existent organization → 404
2. ✅ Create license with expiration before start → 400
3. ✅ Update non-existent license → 404
4. ✅ Get non-existent license → 404
5. ✅ Non-admin user attempts access → 403

### **Edge Cases:**
1. ✅ License expires during operation
2. ✅ Multiple licenses for same organization
3. ✅ Cancel license with active assignments
4. ✅ Filter by multiple criteria
5. ✅ Pagination with large datasets

---

## Performance Considerations

### **Query Optimization:**
- Eager loading with `.Include()` avoids N+1 queries
- Batch count queries for assigned users
- Indexes on frequently filtered columns

### **Pagination:**
- Limits result set size (max 100 per page)
- Total count query before pagination
- Efficient Skip/Take implementation

### **Caching Opportunities:**
- Organization lookups could be cached
- License counts for dashboard views
- Not implemented in current version

---

## API Examples

### **Create License with Auto-Renewal**
```bash
curl -X POST "https://localhost:5001/api/license" \
  -H "Authorization: Bearer {admin-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "organizationId": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Premium License",
    "autoRenewalEnabled": true
  }'
```

### **Get Active Licenses Expiring Soon**
```bash
curl -X GET "https://localhost:5001/api/license?isActive=true&expirationStatus=active&sortBy=expirationdate&sortDirection=asc" \
  -H "Authorization: Bearer {admin-token}"
```

### **Extend License Expiration**
```bash
curl -X PUT "https://localhost:5001/api/license/123e4567-e89b-12d3-a456-426614174000" \
  -H "Authorization: Bearer {admin-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "expirationDate": "2026-01-20T10:00:00Z"
  }'
```

### **Cancel License**
```bash
curl -X DELETE "https://localhost:5001/api/license/123e4567-e89b-12d3-a456-426614174000" \
  -H "Authorization: Bearer {admin-token}"
```

---

## File Location

```
CaseGuard.Backend.Assignment/
└── Controllers/
    └── LicenseController.cs
```

---

**Status**: ✅ Task 3 Complete - All 4 Admin user stories fully implemented with pagination, filtering, sorting, and comprehensive error handling
