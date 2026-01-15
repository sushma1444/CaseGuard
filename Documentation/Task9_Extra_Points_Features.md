# Task 9: Extra Points Features - Documentation

## Overview
Implementation of advanced query features including pagination, filtering, and sorting across all list endpoints for improved performance and user experience.

---

## Features Implemented

### **Core Features**
1. ✅ **Pagination** - All list endpoints support page-based navigation
2. ✅ **Filtering** - Dynamic filtering by entity properties
3. ✅ **Sorting** - Multi-column sorting with direction control
4. ✅ **Search** - Text search across relevant fields

### **Supported Endpoints**
- `/api/license` - List licenses
- `/api/organization` - List organizations
- `/api/member` - List organization members
- `/api/invitation/{organizationId}` - List invitations
- `/api/license-assignment` - List license assignments
- `/api/user/organizations` - List user's organizations

---

## Pagination

### **Implementation Pattern**

All list endpoints use standardized pagination with `PaginationRequest` and `PaginationResponse<T>`.

### **PaginationRequest**

**File:** `Contracts/Common/PaginationRequest.cs`

```csharp
/// <summary>
/// Base request for paginated queries.
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Page number (1-based).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;
}
```

**Features:**
- ✅ Default page: 1
- ✅ Default page size: 10
- ✅ Max page size: 100
- ✅ Validation: Page ≥ 1, PageSize 1-100
- ✅ Prevents excessive data requests

**Validation:**
```csharp
// Valid
{ "page": 1, "pageSize": 10 }      // ✅
{ "page": 5, "pageSize": 50 }      // ✅

// Invalid
{ "page": 0, "pageSize": 10 }      // ❌ Page must be at least 1
{ "page": 1, "pageSize": 150 }     // ❌ PageSize must be between 1 and 100
{ "page": -1, "pageSize": 10 }     // ❌ Page must be at least 1
```

---

### **PaginationResponse**

**File:** `Contracts/Common/PaginationResponse.cs`

```csharp
/// <summary>
/// Paginated response wrapper.
/// </summary>
public class PaginationResponse<T>
{
    /// <summary>
    /// The items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
```

**Properties:**
- `Items` - Current page data
- `Page` - Current page number (1-based)
- `PageSize` - Items per page
- `TotalCount` - Total items across all pages
- `TotalPages` - Total number of pages
- `HasPreviousPage` - Navigation helper
- `HasNextPage` - Navigation helper

**Example Response:**
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "Enterprise License",
      "isValid": true
    },
    {
      "id": "223e4567-e89b-12d3-a456-426614174000",
      "name": "Team License",
      "isValid": true
    }
  ],
  "page": 2,
  "pageSize": 10,
  "totalCount": 45,
  "totalPages": 5,
  "hasPreviousPage": true,
  "hasNextPage": true
}
```

---

### **Pagination Implementation**

#### **Controller Pattern**

```csharp
[HttpGet]
public async Task<IActionResult> GetLicenses([FromQuery] GetLicensesRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    try
    {
        // Build query
        var query = _dbContext.Licenses.AsQueryable();

        // Apply filtering
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(l => l.OrganizationId == request.OrganizationId.Value);
        }

        if (request.IsValid.HasValue)
        {
            query = query.Where(l => l.IsValid == request.IsValid.Value);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            query = ApplySorting(query, request.SortBy, request.IsDescending);
        }
        else
        {
            query = query.OrderByDescending(l => l.CreatedAt);
        }

        // Get total count BEFORE pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var licenses = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Map to response DTOs
        var licenseResponses = licenses.Select(l => new LicenseResponse
        {
            Id = l.Id,
            Name = l.Name,
            // ... other properties
        }).ToList();

        // Build pagination response
        var response = new PaginationResponse<LicenseResponse>
        {
            Items = licenseResponses,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return Ok(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving licenses");
        throw new BadRequestException("Failed to retrieve licenses.");
    }
}
```

#### **Key Steps**

1. **Build Base Query** - Start with `DbSet.AsQueryable()`
2. **Apply Filters** - Add `.Where()` conditions
3. **Apply Sorting** - Add `.OrderBy()` or `.OrderByDescending()`
4. **Count Total** - Get total count BEFORE pagination
5. **Apply Pagination** - Use `.Skip()` and `.Take()`
6. **Execute Query** - Call `.ToListAsync()`
7. **Map to DTOs** - Transform entities to response objects
8. **Build Response** - Create `PaginationResponse<T>`

#### **Performance Optimization**

```csharp
// ✅ Good - Count before materialization
var totalCount = await query.CountAsync();
var items = await query.Skip(...).Take(...).ToListAsync();

// ❌ Bad - Materializes entire dataset
var allItems = await query.ToListAsync();
var totalCount = allItems.Count;
var items = allItems.Skip(...).Take(...).ToList();
```

---

## Filtering

### **Request Models with Filters**

#### **GetLicensesRequest**

**File:** `Contracts/Licenses/GetLicensesRequest.cs`

```csharp
public class GetLicensesRequest : PaginationRequest
{
    /// <summary>
    /// Filter by organization ID.
    /// </summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>
    /// Filter by license validity status.
    /// </summary>
    public bool? IsValid { get; set; }

    /// <summary>
    /// Sort field name (e.g., "Name", "StartDate", "ExpirationDate").
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (true for descending, false for ascending).
    /// </summary>
    public bool IsDescending { get; set; } = false;
}
```

#### **GetOrganizationsRequest**

**File:** `Contracts/Organizations/GetOrganizationsRequest.cs`

```csharp
public class GetOrganizationsRequest : PaginationRequest
{
    /// <summary>
    /// Search term to filter organizations by name.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Sort field name (e.g., "Name", "CreatedAt").
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (true for descending, false for ascending).
    /// </summary>
    public bool IsDescending { get; set; } = false;
}
```

#### **GetMembersRequest**

**File:** `Contracts/Members/GetMembersRequest.cs`

```csharp
public class GetMembersRequest : PaginationRequest
{
    /// <summary>
    /// Filter by member role.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Sort field name (e.g., "JoinedAt", "Role").
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (true for descending, false for ascending).
    /// </summary>
    public bool IsDescending { get; set; } = false;
}
```

---

### **Filter Implementation Patterns**

#### **Pattern 1: Optional Property Filters**

```csharp
// Filter by optional GUID
if (request.OrganizationId.HasValue)
{
    query = query.Where(l => l.OrganizationId == request.OrganizationId.Value);
}

// Filter by optional boolean
if (request.IsValid.HasValue)
{
    query = query.Where(l => l.IsValid == request.IsValid.Value);
}

// Filter by optional string (exact match)
if (!string.IsNullOrEmpty(request.Role))
{
    query = query.Where(m => m.Role == request.Role);
}
```

#### **Pattern 2: Search Term Filtering**

```csharp
// Case-insensitive search across multiple fields
if (!string.IsNullOrWhiteSpace(request.SearchTerm))
{
    var searchLower = request.SearchTerm.ToLower();
    query = query.Where(o => 
        o.Name.ToLower().Contains(searchLower) ||
        (o.Description != null && o.Description.ToLower().Contains(searchLower))
    );
}
```

#### **Pattern 3: Date Range Filtering**

```csharp
// Filter by date range
if (request.StartDate.HasValue)
{
    query = query.Where(l => l.StartDate >= request.StartDate.Value);
}

if (request.EndDate.HasValue)
{
    query = query.Where(l => l.ExpirationDate <= request.EndDate.Value);
}
```

#### **Pattern 4: Related Entity Filtering**

```csharp
// Include related entities for filtering
query = query.Include(la => la.User)
             .Include(la => la.License);

// Filter by related entity property
if (!string.IsNullOrEmpty(request.UserEmail))
{
    query = query.Where(la => la.User.Email == request.UserEmail);
}
```

---

## Sorting

### **Dynamic Sorting Implementation**

#### **Helper Method**

```csharp
private IQueryable<License> ApplySorting(
    IQueryable<License> query, 
    string sortBy, 
    bool isDescending)
{
    // Normalize sort field name
    var sortField = sortBy.ToLower();

    // Apply sorting based on field
    return sortField switch
    {
        "name" => isDescending 
            ? query.OrderByDescending(l => l.Name)
            : query.OrderBy(l => l.Name),
            
        "startdate" => isDescending
            ? query.OrderByDescending(l => l.StartDate)
            : query.OrderBy(l => l.StartDate),
            
        "expirationdate" => isDescending
            ? query.OrderByDescending(l => l.ExpirationDate)
            : query.OrderBy(l => l.ExpirationDate),
            
        "isvalid" => isDescending
            ? query.OrderByDescending(l => l.IsValid)
            : query.OrderBy(l => l.IsValid),
            
        "createdat" => isDescending
            ? query.OrderByDescending(l => l.CreatedAt)
            : query.OrderBy(l => l.CreatedAt),
            
        _ => query.OrderByDescending(l => l.CreatedAt) // Default sort
    };
}
```

#### **Usage in Controller**

```csharp
// Apply sorting
if (!string.IsNullOrEmpty(request.SortBy))
{
    query = ApplySorting(query, request.SortBy, request.IsDescending);
}
else
{
    // Default sort
    query = query.OrderByDescending(l => l.CreatedAt);
}
```

---

### **Sorting Options by Endpoint**

#### **Licenses**
| SortBy Value | Description | Type |
|-------------|-------------|------|
| `Name` | License name | string |
| `StartDate` | License start date | DateTime |
| `ExpirationDate` | License expiration date | DateTime? |
| `IsValid` | License validity status | bool |
| `CreatedAt` | Creation timestamp | DateTime |

#### **Organizations**
| SortBy Value | Description | Type |
|-------------|-------------|------|
| `Name` | Organization name | string |
| `CreatedAt` | Creation timestamp | DateTime |
| `UpdatedAt` | Last update timestamp | DateTime |

#### **Members**
| SortBy Value | Description | Type |
|-------------|-------------|------|
| `JoinedAt` | Member join date | DateTime |
| `Role` | Member role | string |
| `UserEmail` | User email (via join) | string |

#### **Invitations**
| SortBy Value | Description | Type |
|-------------|-------------|------|
| `CreatedAt` | Invitation sent date | DateTime |
| `Status` | Invitation status | enum |
| `Email` | Invited email | string |
| `Role` | Invited role | string |

---

## API Examples

### **Example 1: Basic Pagination**

**Request:**
```http
GET /api/license?page=1&pageSize=10
Authorization: Bearer {token}
```

**Response:**
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 45,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

### **Example 2: Pagination + Filtering**

**Request:**
```http
GET /api/license?page=1&pageSize=20&organizationId=123e4567-e89b-12d3-a456-426614174000&isValid=true
Authorization: Bearer {token}
```

**Query Parameters:**
- `page=1` - First page
- `pageSize=20` - 20 items per page
- `organizationId={guid}` - Filter by organization
- `isValid=true` - Only valid licenses

**Response:**
```json
{
  "items": [
    {
      "id": "...",
      "name": "Enterprise License",
      "organizationId": "123e4567-e89b-12d3-a456-426614174000",
      "isValid": true,
      "startDate": "2026-01-01T00:00:00Z",
      "expirationDate": "2027-01-01T00:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 5,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

### **Example 3: Pagination + Sorting**

**Request:**
```http
GET /api/license?page=2&pageSize=10&sortBy=ExpirationDate&isDescending=true
Authorization: Bearer {token}
```

**Query Parameters:**
- `page=2` - Second page
- `pageSize=10` - 10 items per page
- `sortBy=ExpirationDate` - Sort by expiration date
- `isDescending=true` - Descending order (newest first)

**Result:** Licenses sorted by expiration date (newest first), page 2

---

### **Example 4: Search Organizations**

**Request:**
```http
GET /api/organization?page=1&pageSize=10&searchTerm=tech&sortBy=Name&isDescending=false
Authorization: Bearer {token}
```

**Query Parameters:**
- `searchTerm=tech` - Search for "tech" in name/description
- `sortBy=Name` - Sort alphabetically by name
- `isDescending=false` - Ascending order (A-Z)

**Response:**
```json
{
  "items": [
    {
      "id": "...",
      "name": "Tech Innovations Inc",
      "description": "Leading technology solutions"
    },
    {
      "id": "...",
      "name": "TechCorp",
      "description": "Enterprise tech services"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 2,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

### **Example 5: Filter Members by Role**

**Request:**
```http
GET /api/member/123e4567-e89b-12d3-a456-426614174000?page=1&pageSize=10&role=OrganizationAdmin
Authorization: Bearer {token}
```

**Query Parameters:**
- `role=OrganizationAdmin` - Only organization admins
- `page=1&pageSize=10` - First page, 10 items

**Response:**
```json
{
  "items": [
    {
      "id": "...",
      "userId": "...",
      "userEmail": "admin@example.com",
      "role": "OrganizationAdmin",
      "joinedAt": "2026-01-01T00:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 3,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

## Performance Considerations

### **Database Optimization**

#### **1. Indexes**

Ensure indexes exist on commonly filtered/sorted columns:

```csharp
// License entity configuration
builder.HasIndex(l => l.OrganizationId);
builder.HasIndex(l => l.IsValid);
builder.HasIndex(l => l.StartDate);
builder.HasIndex(l => l.ExpirationDate);
builder.HasIndex(l => l.CreatedAt);

// Organization entity configuration
builder.HasIndex(o => o.Name);
builder.HasIndex(o => o.CreatedAt);

// OrganizationMember entity configuration
builder.HasIndex(om => new { om.OrganizationId, om.UserId });
builder.HasIndex(om => om.Role);
builder.HasIndex(om => om.JoinedAt);
```

#### **2. Query Efficiency**

```csharp
// ✅ Good - Count before materialization
var totalCount = await query.CountAsync();
var items = await query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

// ❌ Bad - Loads everything into memory
var allItems = await query.ToListAsync();
var totalCount = allItems.Count;
var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();
```

#### **3. Selective Loading**

```csharp
// ✅ Good - Only load needed columns
var licenses = await query
    .Select(l => new LicenseResponse
    {
        Id = l.Id,
        Name = l.Name,
        IsValid = l.IsValid
    })
    .Skip(skip)
    .Take(pageSize)
    .ToListAsync();

// ❌ Bad - Loads all columns
var licenses = await query.Skip(skip).Take(pageSize).ToListAsync();
```

#### **4. Avoid N+1 Queries**

```csharp
// ✅ Good - Eager loading with Include
var members = await _dbContext.OrganizationMembers
    .Include(om => om.User)
    .Where(om => om.OrganizationId == orgId)
    .Skip(skip)
    .Take(pageSize)
    .ToListAsync();

// ❌ Bad - Lazy loading causes N+1
var members = await _dbContext.OrganizationMembers
    .Where(om => om.OrganizationId == orgId)
    .Skip(skip)
    .Take(pageSize)
    .ToListAsync();

foreach (var member in members)
{
    // Triggers separate query for each member
    var user = member.User;
}
```

---

### **Pagination Limits**

```csharp
/// <summary>
/// Maximum page size to prevent excessive data retrieval.
/// </summary>
[Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
public int PageSize { get; set; } = 10;
```

**Reasoning:**
- ✅ Prevents memory issues
- ✅ Reduces database load
- ✅ Improves response time
- ✅ Protects against abuse

**Guidelines:**
- Default: 10 items
- Maximum: 100 items
- Recommended: 10-50 items for most use cases

---

## Endpoint Summary

### **Complete Implementation Matrix**

| Endpoint | Pagination | Filtering | Sorting | Search |
|----------|-----------|-----------|---------|--------|
| `GET /api/license` | ✅ | ✅ (OrgId, IsValid) | ✅ (Name, Dates, IsValid) | ❌ |
| `GET /api/organization` | ✅ | ❌ | ✅ (Name, CreatedAt) | ✅ (Name, Desc) |
| `GET /api/member/{orgId}` | ✅ | ✅ (Role) | ✅ (JoinedAt, Role) | ❌ |
| `GET /api/invitation/{orgId}` | ✅ | ✅ (Status) | ✅ (CreatedAt, Status) | ❌ |
| `GET /api/license-assignment` | ✅ | ✅ (LicenseId, UserId) | ✅ (AssignedAt) | ❌ |
| `GET /api/user/organizations` | ✅ | ❌ | ✅ (Name, JoinedAt) | ✅ (Name) |

---

## Testing Examples

### **cURL Commands**

#### **Test Pagination**
```bash
# Page 1
curl -X GET "https://localhost:5001/api/license?page=1&pageSize=10" \
  -H "Authorization: Bearer {token}"

# Page 2
curl -X GET "https://localhost:5001/api/license?page=2&pageSize=10" \
  -H "Authorization: Bearer {token}"

# Large page size
curl -X GET "https://localhost:5001/api/license?page=1&pageSize=50" \
  -H "Authorization: Bearer {token}"
```

#### **Test Filtering**
```bash
# Filter by organization
curl -X GET "https://localhost:5001/api/license?organizationId=123e4567-e89b-12d3-a456-426614174000" \
  -H "Authorization: Bearer {token}"

# Filter by validity
curl -X GET "https://localhost:5001/api/license?isValid=true" \
  -H "Authorization: Bearer {token}"

# Multiple filters
curl -X GET "https://localhost:5001/api/license?organizationId=123e4567-e89b-12d3-a456-426614174000&isValid=true" \
  -H "Authorization: Bearer {token}"
```

#### **Test Sorting**
```bash
# Sort by name ascending
curl -X GET "https://localhost:5001/api/license?sortBy=Name&isDescending=false" \
  -H "Authorization: Bearer {token}"

# Sort by date descending
curl -X GET "https://localhost:5001/api/license?sortBy=ExpirationDate&isDescending=true" \
  -H "Authorization: Bearer {token}"
```

#### **Test Search**
```bash
# Search organizations
curl -X GET "https://localhost:5001/api/organization?searchTerm=tech" \
  -H "Authorization: Bearer {token}"

# Search with pagination
curl -X GET "https://localhost:5001/api/organization?searchTerm=tech&page=1&pageSize=10" \
  -H "Authorization: Bearer {token}"
```

#### **Test Combined Features**
```bash
# Pagination + Filtering + Sorting
curl -X GET "https://localhost:5001/api/license?page=1&pageSize=20&organizationId=123e4567-e89b-12d3-a456-426614174000&isValid=true&sortBy=ExpirationDate&isDescending=true" \
  -H "Authorization: Bearer {token}"
```

---

## Best Practices

### **✅ Do's**

1. **Always Validate Input**
   ```csharp
   [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
   public int PageSize { get; set; } = 10;
   ```

2. **Count Before Pagination**
   ```csharp
   var totalCount = await query.CountAsync();
   var items = await query.Skip(...).Take(...).ToListAsync();
   ```

3. **Provide Default Sorting**
   ```csharp
   if (string.IsNullOrEmpty(request.SortBy))
   {
       query = query.OrderByDescending(l => l.CreatedAt);
   }
   ```

4. **Use Indexes for Filtered Columns**
   ```csharp
   builder.HasIndex(l => l.OrganizationId);
   builder.HasIndex(l => l.IsValid);
   ```

5. **Include Navigation Helpers**
   ```csharp
   public bool HasPreviousPage => Page > 1;
   public bool HasNextPage => Page < TotalPages;
   ```

### **❌ Don'ts**

1. **Don't Load Everything into Memory**
   ```csharp
   // Bad
   var all = await query.ToListAsync();
   return all.Skip(...).Take(...);
   ```

2. **Don't Allow Unlimited Page Size**
   ```csharp
   // Bad - no limit
   public int PageSize { get; set; }
   
   // Good - enforced limit
   [Range(1, 100)]
   public int PageSize { get; set; } = 10;
   ```

3. **Don't Forget Total Count**
   ```csharp
   // Bad - no way to know total pages
   return new { Items = items, Page = page };
   
   // Good - includes total count
   return new PaginationResponse<T> 
   { 
       Items = items, 
       TotalCount = totalCount 
   };
   ```

4. **Don't Use String Concatenation for Sorting**
   ```csharp
   // Bad - SQL injection risk
   query = query.OrderBy($"Entity.{sortBy}");
   
   // Good - strongly-typed switch
   query = sortBy switch
   {
       "name" => query.OrderBy(e => e.Name),
       _ => query.OrderBy(e => e.Id)
   };
   ```

---

## File Locations

```
CaseGuard.Backend.Assignment.Contracts/
├── Common/
│   ├── PaginationRequest.cs
│   └── PaginationResponse.cs
├── Licenses/
│   └── GetLicensesRequest.cs (extends PaginationRequest)
├── Organizations/
│   └── GetOrganizationsRequest.cs (extends PaginationRequest)
├── Members/
│   └── GetMembersRequest.cs (extends PaginationRequest)
└── Invitations/
    └── GetInvitationsRequest.cs (extends PaginationRequest)

CaseGuard.Backend.Assignment/
└── Controllers/
    ├── LicenseController.cs (implements pagination/filtering/sorting)
    ├── OrganizationController.cs (implements pagination/search/sorting)
    ├── MemberController.cs (implements pagination/filtering/sorting)
    ├── InvitationController.cs (implements pagination/filtering/sorting)
    └── UserController.cs (implements pagination/sorting)
```

---

## Related Documentation

- [Task 2: DTOs](Task2_Request_Response_DTOs.md) - Request/Response structures
- [Task 3: Admin Endpoints](Task3_Admin_Endpoints.md) - License pagination example
- [Task 4: Organization Endpoints](Task4_Organization_Owner_Admin_Endpoints.md) - Member/invitation pagination

---

**Status**: ✅ Task 9 Complete - Comprehensive pagination, filtering, and sorting implemented across all list endpoints with proper validation and performance optimization
