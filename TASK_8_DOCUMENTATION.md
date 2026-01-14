# Task 8: Admin Endpoints - License Management

## Overview

Task 8 implements the **LicenseController**, which provides Admin-only endpoints for managing licenses in the system. All endpoints require Admin role authorization and provide comprehensive license management capabilities including creation, viewing, updating, and cancellation.

## Implementation Details

### Controller Location
`CaseGuard.Backend.Assignment/Controllers/LicenseController.cs`

### Authorization
- **Policy**: `AdminOnly`
- **Required Role**: `Admin`
- All endpoints are protected with `[Authorize(Policy = "AdminOnly")]`

### Base Functionality
- Inherits from `BaseController` for common functionality
- Uses `ApplicationDbContext` for database operations
- Includes comprehensive logging for all operations
- Implements proper error handling with custom exceptions

---

## Endpoints

### 1. Create License

**Endpoint**: `POST /api/license`

**Description**: Creates a new license for an organization.

**Authorization**: Admin only

**Request Body** (`CreateLicenseRequest`):
```json
{
  "organizationId": "guid",
  "name": "string (required, max 200 chars)",
  "startDate": "datetime (optional, defaults to UTC now)",
  "expirationDate": "datetime (optional, defaults to startDate + 10 minutes)",
  "autoRenewalEnabled": "boolean (optional, defaults to false)"
}
```

**Response** (`LicenseResponse`):
- **Status Code**: `201 Created`
- **Body**: Created license information with assigned user count

**Example Request**:
```json
{
  "organizationId": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Premium License",
  "autoRenewalEnabled": true
}
```

**Example Response**:
```json
{
  "id": "789e4567-e89b-12d3-a456-426614174000",
  "organizationId": "123e4567-e89b-12d3-a456-426614174000",
  "organizationName": "Acme Corp",
  "name": "Premium License",
  "startDate": "2026-01-12T10:00:00Z",
  "expirationDate": "2026-01-12T10:10:00Z",
  "autoRenewalEnabled": true,
  "isActive": true,
  "isValid": true,
  "createdAt": "2026-01-12T10:00:00Z",
  "updatedAt": "2026-01-12T10:00:00Z",
  "cancelledAt": null,
  "assignedUserCount": 0
}
```

**Validation Rules**:
- Organization must exist
- Expiration date must be after start date
- Name is required (max 200 characters)

**Error Responses**:
- `400 Bad Request`: Invalid request data or validation failure
- `404 Not Found`: Organization not found
- `403 Forbidden`: User is not an admin

---

### 2. Get Licenses (List)

**Endpoint**: `GET /api/license`

**Description**: Retrieves a paginated list of licenses with optional filtering, searching, and sorting.

**Authorization**: Admin only

**Query Parameters** (`GetLicensesRequest`):
- `page` (int, default: 1): Page number
- `pageSize` (int, default: 10, max: 100): Number of items per page
- `sortBy` (string, optional): Field to sort by (`name`, `startDate`, `expirationDate`, `createdAt`, `organizationName`)
- `sortDirection` (string, optional): Sort direction (`asc` or `desc`, default: `asc`)
- `searchTerm` (string, optional): Search in license name or organization name
- `organizationId` (Guid, optional): Filter by organization ID
- `isActive` (bool, optional): Filter by active status
- `autoRenewalEnabled` (bool, optional): Filter by auto-renewal status
- `expirationStatus` (string, optional): Filter by expiration status (`expired`, `active`, `all`)

**Response** (`GetLicensesResponse`):
- **Status Code**: `200 OK`
- **Body**: Paginated list of licenses

**Example Request**:
```
GET /api/license?page=1&pageSize=20&sortBy=expirationDate&sortDirection=desc&isActive=true&expirationStatus=active
```

**Example Response**:
```json
{
  "items": [
    {
      "id": "789e4567-e89b-12d3-a456-426614174000",
      "organizationId": "123e4567-e89b-12d3-a456-426614174000",
      "organizationName": "Acme Corp",
      "name": "Premium License",
      "startDate": "2026-01-12T10:00:00Z",
      "expirationDate": "2026-01-12T10:10:00Z",
      "autoRenewalEnabled": true,
      "isActive": true,
      "isValid": true,
      "createdAt": "2026-01-12T10:00:00Z",
      "updatedAt": "2026-01-12T10:00:00Z",
      "cancelledAt": null,
      "assignedUserCount": 5
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

**Filtering Options**:
- **By Organization**: `organizationId=guid`
- **By Active Status**: `isActive=true` or `isActive=false`
- **By Auto-Renewal**: `autoRenewalEnabled=true` or `autoRenewalEnabled=false`
- **By Expiration Status**: 
  - `expirationStatus=expired` - Only expired licenses
  - `expirationStatus=active` - Only active (not expired) licenses
  - `expirationStatus=all` - All licenses regardless of expiration

**Sorting Options**:
- `name` - Sort by license name
- `startDate` - Sort by start date
- `expirationDate` - Sort by expiration date
- `createdAt` - Sort by creation date (default)
- `organizationName` - Sort by organization name

**Search**:
- Searches in license name and organization name (case-insensitive)

**Error Responses**:
- `400 Bad Request`: Invalid query parameters
- `403 Forbidden`: User is not an admin

---

### 3. Get License by ID

**Endpoint**: `GET /api/license/{id}`

**Description**: Retrieves a specific license by its ID.

**Authorization**: Admin only

**Path Parameters**:
- `id` (Guid): License ID

**Response** (`LicenseResponse`):
- **Status Code**: `200 OK`
- **Body**: License information

**Example Request**:
```
GET /api/license/789e4567-e89b-12d3-a456-426614174000
```

**Example Response**:
```json
{
  "id": "789e4567-e89b-12d3-a456-426614174000",
  "organizationId": "123e4567-e89b-12d3-a456-426614174000",
  "organizationName": "Acme Corp",
  "name": "Premium License",
  "startDate": "2026-01-12T10:00:00Z",
  "expirationDate": "2026-01-12T10:10:00Z",
  "autoRenewalEnabled": true,
  "isActive": true,
  "isValid": true,
  "createdAt": "2026-01-12T10:00:00Z",
  "updatedAt": "2026-01-12T10:00:00Z",
  "cancelledAt": null,
  "assignedUserCount": 5
}
```

**Error Responses**:
- `404 Not Found`: License not found
- `403 Forbidden`: User is not an admin

---

### 4. Update License

**Endpoint**: `PUT /api/license/{id}`

**Description**: Updates a license's properties. Only provided properties are updated.

**Authorization**: Admin only

**Path Parameters**:
- `id` (Guid): License ID

**Request Body** (`UpdateLicenseRequest`):
```json
{
  "name": "string (optional, max 200 chars)",
  "expirationDate": "datetime (optional)",
  "autoRenewalEnabled": "boolean (optional)",
  "isActive": "boolean (optional)"
}
```

**Response** (`LicenseResponse`):
- **Status Code**: `200 OK`
- **Body**: Updated license information

**Example Request**:
```json
{
  "name": "Updated Premium License",
  "expirationDate": "2026-01-12T11:00:00Z",
  "autoRenewalEnabled": false
}
```

**Example Response**:
```json
{
  "id": "789e4567-e89b-12d3-a456-426614174000",
  "organizationId": "123e4567-e89b-12d3-a456-426614174000",
  "organizationName": "Acme Corp",
  "name": "Updated Premium License",
  "startDate": "2026-01-12T10:00:00Z",
  "expirationDate": "2026-01-12T11:00:00Z",
  "autoRenewalEnabled": false,
  "isActive": true,
  "isValid": true,
  "createdAt": "2026-01-12T10:00:00Z",
  "updatedAt": "2026-01-12T10:05:00Z",
  "cancelledAt": null,
  "assignedUserCount": 5
}
```

**Validation Rules**:
- Expiration date must be after start date
- All fields are optional (partial updates supported)

**Error Responses**:
- `400 Bad Request`: Invalid request data or validation failure
- `404 Not Found`: License not found
- `403 Forbidden`: User is not an admin

---

### 5. Cancel License

**Endpoint**: `DELETE /api/license/{id}`

**Description**: Cancels (revokes) a license. This sets `IsActive` to `false`, sets `CancelledAt` timestamp, and automatically unassigns all active license assignments.

**Authorization**: Admin only

**Path Parameters**:
- `id` (Guid): License ID

**Response**:
- **Status Code**: `204 No Content`
- **Body**: None

**Example Request**:
```
DELETE /api/license/789e4567-e89b-12d3-a456-426614174000
```

**Behavior**:
1. Sets license `IsActive` to `false`
2. Sets `CancelledAt` to current UTC time
3. Sets `UpdatedAt` to current UTC time
4. Unassigns all active license assignments (sets `UnassignedAt` for all assignments)

**Error Responses**:
- `404 Not Found`: License not found
- `403 Forbidden`: User is not an admin

---

## Data Transfer Objects (DTOs)

### Request DTOs

#### CreateLicenseRequest
```csharp
public class CreateLicenseRequest
{
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool? AutoRenewalEnabled { get; set; }
}
```

#### UpdateLicenseRequest
```csharp
public class UpdateLicenseRequest
{
    [StringLength(200)]
    public string? Name { get; set; }
    
    public DateTime? ExpirationDate { get; set; }
    public bool? AutoRenewalEnabled { get; set; }
    public bool? IsActive { get; set; }
}
```

#### GetLicensesRequest
```csharp
public class GetLicensesRequest : PaginationRequest
{
    public Guid? OrganizationId { get; set; }
    public bool? IsActive { get; set; }
    public bool? AutoRenewalEnabled { get; set; }
    public string? ExpirationStatus { get; set; }
}
```

### Response DTOs

#### LicenseResponse
```csharp
public class LicenseResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool AutoRenewalEnabled { get; set; }
    public bool IsActive { get; set; }
    public bool IsValid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int AssignedUserCount { get; set; }
}
```

#### GetLicensesResponse
```csharp
public class GetLicensesResponse : PaginationResponse<LicenseResponse>
{
    // Inherits pagination properties from PaginationResponse<T>
}
```

---

## Business Logic

### License Creation
- **Default Expiration**: If `expirationDate` is not provided, it defaults to `startDate + 10 minutes` (for testing purposes)
- **Default Start Date**: If `startDate` is not provided, it defaults to current UTC time
- **Initial State**: New licenses are created with `IsActive = true`
- **Organization Validation**: The organization must exist before creating a license

### License Validation
- **Expiration Check**: `IsValid` property is computed based on:
  - License is active (`IsActive == true`)
  - Current time is before expiration date
  - License has not been cancelled

### License Cancellation
- **Cascade Effect**: When a license is cancelled, all active license assignments are automatically unassigned
- **Soft Delete**: License is not deleted from database, but marked as inactive for audit trail

### Pagination and Filtering
- **Default Page Size**: 10 items per page
- **Maximum Page Size**: 100 items per page
- **Default Sort**: By creation date (newest first)
- **Search**: Case-insensitive search in license name and organization name

---

## Error Handling

All endpoints use custom exception classes that are automatically converted to ProblemDetails responses by the global exception handler:

- **BadRequestException**: Invalid input or validation failure (400)
- **NotFoundException**: Resource not found (404)
- **ForbiddenException**: Insufficient permissions (403)
- **UnauthorizedException**: Authentication required (401)

---

## Logging

All operations are logged with appropriate log levels:

- **Information**: Successful operations (create, update, cancel)
- **Error**: Exceptions and failures

Log entries include:
- Operation type
- License ID
- Organization ID
- User ID (Admin performing the action)
- Error details (when applicable)

---

## Testing Examples

### Using Swagger UI

1. **Login as Admin**:
   ```
   POST /api/auth/login
   {
     "userId": "admin123",
     "email": "admin@example.com",
     "role": "Admin"
   }
   ```

2. **Copy the JWT token** from the response

3. **Authorize in Swagger**:
   - Click the "Authorize" button
   - Enter: `Bearer <your-token>`
   - Click "Authorize"

4. **Test Endpoints**:
   - Use the Swagger UI to test all endpoints interactively

### Using cURL

**Create License**:
```bash
curl -X POST "https://localhost:5000/api/license" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "organizationId": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Premium License",
    "autoRenewalEnabled": true
  }'
```

**Get Licenses**:
```bash
curl -X GET "https://localhost:5000/api/license?page=1&pageSize=20&isActive=true" \
  -H "Authorization: Bearer <token>"
```

**Update License**:
```bash
curl -X PUT "https://localhost:5000/api/license/789e4567-e89b-12d3-a456-426614174000" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated License Name",
    "expirationDate": "2026-01-12T12:00:00Z"
  }'
```

**Cancel License**:
```bash
curl -X DELETE "https://localhost:5000/api/license/789e4567-e89b-12d3-a456-426614174000" \
  -H "Authorization: Bearer <token>"
```

---

## Key Features

✅ **Complete CRUD Operations**: Create, Read, Update, Delete  
✅ **Pagination**: Efficient handling of large datasets  
✅ **Filtering**: Multiple filter options for precise queries  
✅ **Search**: Full-text search across license and organization names  
✅ **Sorting**: Flexible sorting by multiple fields  
✅ **Authorization**: Admin-only access with proper security  
✅ **Error Handling**: Comprehensive error handling with meaningful messages  
✅ **Logging**: Detailed logging for audit and debugging  
✅ **Validation**: Input validation and business rule enforcement  
✅ **Cascade Operations**: Automatic handling of related entities  

---

## Notes

- **Default Expiration**: Licenses default to 10 minutes expiration for testing purposes. In production, this would typically be longer (e.g., 1 year).
- **Auto-Renewal**: The auto-renewal feature is stored but the actual renewal logic is implemented in Task 15 (background job).
- **License Assignments**: The system tracks how many users have been assigned to each license, but assignment management is handled in Task 12.
- **Soft Delete**: Licenses are not physically deleted but marked as inactive to maintain audit trail.

---

## Related Tasks

- **Task 7**: Authorization system (provides AdminOnly policy)
- **Task 9**: Organization management (licenses are tied to organizations)
- **Task 12**: License assignment endpoints (assigning licenses to users)
- **Task 14**: License expiration logic (checking and enforcing expiration)
- **Task 15**: Auto-renewal background job (automatic license renewal)
