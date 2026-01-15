# Task 8: Error Handling & Validation - Documentation

## Overview
Complete implementation of error handling and validation system with custom exceptions, global exception handler, and comprehensive validation rules.

---

## Error Handling Architecture

### **Three-Layer Approach**

```
1. Validation Layer
   ├── Data Annotations (DTOs)
   ├── ModelState Validation
   └── Business Rule Validation

2. Exception Layer
   ├── Custom Exceptions
   ├── Built-in Exceptions
   └── Unexpected Exceptions

3. Response Layer
   └── Global Exception Handler
       └── ProblemDetails Format
```

---

## Custom Exceptions

### **Location**
`Exceptions/` folder

### **Base Exception Hierarchy**

```
Exception (System)
    ↓
Custom Business Exceptions
    ├── BadRequestException (400)
    ├── UnauthorizedException (401)
    ├── ForbiddenException (403)
    └── NotFoundException (404)
```

---

### **BadRequestException**

**File:** `Exceptions/BadRequestException.cs`

**HTTP Status:** `400 Bad Request`

**Purpose:** Validation errors, business rule violations, invalid input

**Implementation:**
```csharp
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
```

**When to Use:**
- ✅ Invalid input data
- ✅ Business rule violations
- ✅ Duplicate resources
- ✅ Constraint violations
- ✅ Invalid state transitions

**Examples:**
```csharp
// Duplicate resource
throw new BadRequestException("An organization with this name already exists.");

// Business rule violation
throw new BadRequestException("Cannot remove the last Owner from the organization.");

// Invalid state
throw new BadRequestException("License must be active to assign to users.");

// Constraint violation
throw new BadRequestException("Expiration date must be after start date.");

// Invalid operation
throw new BadRequestException("User is already a member of this organization.");
```

**Response Format:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "An organization with this name already exists.",
  "traceId": "00-1234567890abcdef-1234567890abcdef-00"
}
```

---

### **UnauthorizedException**

**File:** `Exceptions/UnauthorizedException.cs`

**HTTP Status:** `401 Unauthorized`

**Purpose:** Authentication failures, missing/invalid credentials

**Implementation:**
```csharp
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
```

**When to Use:**
- ✅ Missing JWT token
- ✅ Invalid JWT token
- ✅ Expired JWT token
- ✅ Missing required claims
- ✅ Invalid user credentials

**Examples:**
```csharp
// Missing authentication
throw new UnauthorizedException("User is not authenticated.");

// Invalid claims
throw new UnauthorizedException("Invalid user ID in token claims.");

// Missing required claim
throw new UnauthorizedException("Email claim is required.");
```

**Response Format:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "User is not authenticated.",
  "traceId": "00-1234567890abcdef-1234567890abcdef-00"
}
```

**Usage in BaseController:**
```csharp
protected Guid CurrentUserIdGuid
{
    get
    {
        var userIdClaim = User.FindFirst(CustomClaimTypes.UserId)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Invalid or missing user ID in token.");
        }
        
        return userId;
    }
}
```

---

### **ForbiddenException**

**File:** `Exceptions/ForbiddenException.cs`

**HTTP Status:** `403 Forbidden`

**Purpose:** Authorization failures, insufficient permissions

**Implementation:**
```csharp
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }

    public ForbiddenException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
```

**When to Use:**
- ✅ User lacks required role
- ✅ User not member of organization
- ✅ User not Owner/Admin
- ✅ Insufficient permissions
- ✅ Access denied to resource

**Examples:**
```csharp
// Not organization member
throw new ForbiddenException("You do not have access to this organization.");

// Insufficient role
throw new ForbiddenException("You do not have permission to manage members of this organization.");

// Not owner
throw new ForbiddenException("You must be the organization owner to perform this action.");

// Admin-only operation
throw new ForbiddenException("This operation requires administrator privileges.");
```

**Response Format:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to manage members of this organization.",
  "traceId": "00-1234567890abcdef-1234567890abcdef-00"
}
```

**Usage in AuthorizationHelper:**
```csharp
public static async Task EnsureUserIsOwnerOrAdminOfOrganizationAsync(
    ApplicationDbContext dbContext,
    Guid userId,
    Guid organizationId)
{
    var isAuthorized = await IsOwnerOrAdminOfOrganizationAsync(
        dbContext, userId, organizationId);
    
    if (!isAuthorized)
    {
        throw new ForbiddenException(
            "You do not have permission to manage members of this organization.");
    }
}
```

---

### **NotFoundException**

**File:** `Exceptions/NotFoundException.cs`

**HTTP Status:** `404 Not Found`

**Purpose:** Resource not found or doesn't exist

**Implementation:**
```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string resourceName, object resourceId) 
        : base($"{resourceName} with ID '{resourceId}' was not found.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
```

**When to Use:**
- ✅ Resource doesn't exist in database
- ✅ ID not found
- ✅ User not member (hides existence)
- ✅ Related entity missing

**Examples:**
```csharp
// Resource not found by ID
throw new NotFoundException("Organization", organizationId);
// Message: "Organization with ID 'guid' was not found."

throw new NotFoundException("License", licenseId);
// Message: "License with ID 'guid' was not found."

throw new NotFoundException("Invitation", invitationId);
// Message: "Invitation with ID 'guid' was not found."

// Custom message
throw new NotFoundException("The requested organization does not exist or you are not a member.");

// Related entity missing
throw new NotFoundException("User not found. Please ensure you are properly authenticated.");
```

**Response Format:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Organization with ID '123e4567-e89b-12d3-a456-426614174000' was not found.",
  "traceId": "00-1234567890abcdef-1234567890abcdef-00"
}
```

**Security Note:**
Using 404 instead of 403 for non-members prevents information disclosure:
```csharp
// User tries to access organization they're not a member of
var membership = await _dbContext.OrganizationMembers
    .FirstOrDefaultAsync(om => om.UserId == userId && om.OrganizationId == orgId);

if (membership == null)
{
    // Return 404, not 403 - doesn't reveal organization existence
    throw new NotFoundException("Organization", orgId);
}
```

---

## Global Exception Handler

### **GlobalExceptionHandlingMiddleware**

**File:** `Middleware/GlobalExceptionHandlingMiddleware.cs`

**Purpose:** Centralized exception handling, converts exceptions to ProblemDetails format

### **Implementation**

```csharp
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log the exception
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        // Create ProblemDetails response
        var problemDetails = CreateProblemDetails(context, exception);

        // Set response
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? 500;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
```

### **Exception Mapping**

```csharp
private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
{
    return exception switch
    {
        BadRequestException => new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = exception.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        },
        
        UnauthorizedException => new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = exception.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        },
        
        ForbiddenException => new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = exception.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        },
        
        NotFoundException => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found",
            Detail = exception.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
        },
        
        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        }
    };
}
```

### **Registration**

**File:** `Program.cs`

```csharp
// Add middleware to pipeline (after routing, before endpoints)
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Global exception handler
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.MapControllers();
```

### **Benefits**

✅ **Consistent Error Format** - All errors follow ProblemDetails standard
✅ **Automatic Logging** - All exceptions logged automatically
✅ **No Try-Catch Clutter** - Controllers stay clean
✅ **Standardized Responses** - Clients receive predictable error format
✅ **Security** - Internal details hidden from clients
✅ **Traceability** - TraceId included for debugging

---

## Validation System

### **Layer 1: Data Annotations**

**Location:** DTO classes in `Contracts/` project

**Purpose:** Input validation at model level

#### **Common Annotations**

```csharp
[Required(ErrorMessage = "Organization name is required.")]
public string Name { get; set; }

[StringLength(200, ErrorMessage = "Organization name cannot exceed 200 characters.")]
public string Name { get; set; }

[EmailAddress(ErrorMessage = "Invalid email address format.")]
public string Email { get; set; }

[Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
public int PageSize { get; set; }

[RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Only alphanumeric characters allowed.")]
public string Code { get; set; }
```

#### **Example: CreateOrganizationRequest**

```csharp
public class CreateOrganizationRequest
{
    /// <summary>
    /// Name of the organization.
    /// </summary>
    [Required(ErrorMessage = "Organization name is required.")]
    [StringLength(200, ErrorMessage = "Organization name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the organization.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }
}
```

#### **Validation Happens Automatically**

ASP.NET Core validates automatically before controller action executes:
```csharp
[HttpPost]
public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
{
    // ModelState.IsValid is already checked by framework
    // If invalid, returns 400 BadRequest with validation errors
    
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    
    // Continue with business logic
}
```

**Validation Error Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "Organization name is required."
    ],
    "Description": [
      "Description cannot exceed 1000 characters."
    ]
  }
}
```

---

### **Layer 2: ModelState Validation**

**Purpose:** Check validation results in controller

#### **Standard Pattern**

```csharp
public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
{
    // Check ModelState
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    
    try
    {
        // Business logic
    }
    catch (Exception ex)
    {
        // Exception handling
    }
}
```

#### **Custom Validation**

```csharp
public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    
    // Custom validation
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        ModelState.AddModelError("Name", "Name cannot be empty or whitespace.");
        return BadRequest(ModelState);
    }
    
    // Continue...
}
```

---

### **Layer 3: Business Rule Validation**

**Purpose:** Validate business logic rules

#### **Common Patterns**

**1. Existence Validation**
```csharp
var organization = await _dbContext.Organizations
    .FirstOrDefaultAsync(o => o.Id == organizationId);

if (organization == null)
{
    throw new NotFoundException("Organization", organizationId);
}
```

**2. Duplicate Validation**
```csharp
var existingOrganization = await _dbContext.Organizations
    .FirstOrDefaultAsync(o => o.Name.ToLower() == request.Name.ToLower());

if (existingOrganization != null)
{
    throw new BadRequestException("An organization with this name already exists.");
}
```

**3. State Validation**
```csharp
if (!license.IsValid)
{
    throw new BadRequestException("Cannot assign an invalid or expired license.");
}
```

**4. Constraint Validation**
```csharp
if (request.ExpirationDate <= request.StartDate)
{
    throw new BadRequestException("Expiration date must be after start date.");
}
```

**5. Relationship Validation**
```csharp
var isMember = await _dbContext.OrganizationMembers
    .AnyAsync(om => om.UserId == userId && om.OrganizationId == organizationId);

if (!isMember)
{
    throw new BadRequestException("User must be a member of the organization.");
}
```

**6. Business Rule Validation**
```csharp
// Cannot remove last Owner
var ownerCount = await _dbContext.OrganizationMembers
    .CountAsync(om => om.OrganizationId == organizationId && om.Role == Roles.Owner);

if (ownerCount <= 1 && member.Role == Roles.Owner)
{
    throw new BadRequestException("Cannot remove the last Owner from the organization.");
}
```

---

## Error Handling Patterns

### **Pattern 1: Try-Catch with Specific Exceptions**

```csharp
public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    try
    {
        // Business logic
        return CreatedAtAction(...);
    }
    catch (NotFoundException)
    {
        throw; // Re-throw custom exception (handled by middleware)
    }
    catch (BadRequestException)
    {
        throw; // Re-throw custom exception
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating organization");
        throw new BadRequestException("Failed to create organization. Please check your input and try again.");
    }
}
```

**Why Re-throw Custom Exceptions?**
- Custom exceptions already have correct status codes
- Message already appropriate for client
- Middleware handles them properly
- Preserves exception type

### **Pattern 2: Validation Before Operation**

```csharp
public async Task<IActionResult> UpdateLicense(Guid id, [FromBody] UpdateLicenseRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    try
    {
        // 1. Verify resource exists
        var license = await _dbContext.Licenses.FindAsync(id);
        if (license == null)
        {
            throw new NotFoundException("License", id);
        }
        
        // 2. Validate business rules
        if (request.ExpirationDate.HasValue && 
            request.ExpirationDate.Value <= license.StartDate)
        {
            throw new BadRequestException("Expiration date must be after start date.");
        }
        
        // 3. Perform update
        // ...
        
        return Ok(license);
    }
    catch (NotFoundException)
    {
        throw;
    }
    catch (BadRequestException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating license {LicenseId}", id);
        throw new BadRequestException("Failed to update license.");
    }
}
```

### **Pattern 3: Authorization with Error Handling**

```csharp
public async Task<IActionResult> InviteMember(Guid organizationId, [FromBody] InviteMemberRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    try
    {
        // 1. Verify organization exists
        var organization = await _dbContext.Organizations.FindAsync(organizationId);
        if (organization == null)
        {
            throw new NotFoundException("Organization", organizationId);
        }
        
        // 2. Check authorization (throws ForbiddenException)
        if (!IsAdmin)
        {
            await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
                _dbContext, CurrentUserIdGuid, organizationId);
        }
        
        // 3. Validate business rules
        // ...
        
        // 4. Perform operation
        // ...
        
        return CreatedAtAction(...);
    }
    catch (NotFoundException)
    {
        throw;
    }
    catch (ForbiddenException)
    {
        throw;
    }
    catch (BadRequestException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error inviting member to organization {OrganizationId}", organizationId);
        throw new BadRequestException("Failed to invite member.");
    }
}
```

---

## Validation Rules by Entity

### **Organization**

| Rule | Validation Type | Error |
|------|----------------|-------|
| Name is required | Data Annotation | 400 - Name is required |
| Name max 200 chars | Data Annotation | 400 - Name cannot exceed 200 characters |
| Description max 1000 chars | Data Annotation | 400 - Description cannot exceed 1000 characters |
| Name is unique | Business Rule | 400 - Organization with this name already exists |
| User exists | Business Rule | 400 - User not found |

### **Invitation**

| Rule | Validation Type | Error |
|------|----------------|-------|
| Email is required | Data Annotation | 400 - Email is required |
| Email is valid format | Data Annotation | 400 - Invalid email address |
| Email max 256 chars | Data Annotation | 400 - Email cannot exceed 256 characters |
| Role is required | Data Annotation | 400 - Role is required |
| Organization exists | Business Rule | 404 - Organization not found |
| User not already member | Business Rule | 400 - User is already a member |
| No duplicate pending invitation | Business Rule | 400 - Pending invitation already exists |
| Invitation matches user email | Business Rule | 400 - Invitation not for your email |
| Invitation is pending | Business Rule | 400 - Invitation is not pending |
| Invitation not expired | Business Rule | 400 - Invitation has expired |

### **License**

| Rule | Validation Type | Error |
|------|----------------|-------|
| Name is required | Data Annotation | 400 - Name is required |
| Name max 200 chars | Data Annotation | 400 - Name cannot exceed 200 characters |
| OrganizationId is required | Data Annotation | 400 - Organization ID is required |
| Organization exists | Business Rule | 404 - Organization not found |
| Expiration after start | Business Rule | 400 - Expiration must be after start date |
| License is active | Business Rule | 400 - License is not active |
| License not expired | Business Rule | 400 - License has expired |

### **Member**

| Rule | Validation Type | Error |
|------|----------------|-------|
| Role is valid | Data Annotation | 400 - Invalid role |
| Organization exists | Business Rule | 404 - Organization not found |
| User exists | Business Rule | 404 - User not found |
| Cannot remove last Owner | Business Rule | 400 - Cannot remove last Owner |
| Cannot change last Owner role | Business Rule | 400 - Cannot change last Owner role |
| Owner cannot leave | Business Rule | 400 - Owner cannot leave organization |

### **License Assignment**

| Rule | Validation Type | Error |
|------|----------------|-------|
| LicenseId is required | Data Annotation | 400 - License ID is required |
| UserId is required | Data Annotation | 400 - User ID is required |
| License exists | Business Rule | 404 - License not found |
| User exists | Business Rule | 404 - User not found |
| License is valid | Business Rule | 400 - License is not valid |
| User is member | Business Rule | 400 - User must be a member |
| No duplicate assignment | Business Rule | 409 - License already assigned |

---

## Logging

### **Exception Logging**

All exceptions automatically logged by middleware:
```csharp
_logger.LogError(exception, "An error occurred: {Message}", exception.Message);
```

### **Controller-Level Logging**

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, 
        "Error creating organization for user {UserId}", 
        CurrentUserId);
    throw new BadRequestException("Failed to create organization.");
}
```

### **Log Levels**

| Level | Usage | Example |
|-------|-------|---------|
| Error | Exceptions, failures | Failed database operations |
| Warning | Validation failures | Invalid state transitions |
| Information | Success operations | Resource created successfully |
| Debug | Detailed flow | Authorization checks passed |

---

## Testing Error Handling

### **Test Scenarios**

#### **1. Validation Errors**
```bash
# Missing required field
curl -X POST "https://localhost:5001/api/organization" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"description": "Test"}' # Missing name

Expected: 400 Bad Request with validation errors
```

#### **2. Not Found**
```bash
# Non-existent resource
curl -X GET "https://localhost:5001/api/organization/99999999-9999-9999-9999-999999999999" \
  -H "Authorization: Bearer {token}"

Expected: 404 Not Found
```

#### **3. Forbidden**
```bash
# Insufficient permissions
curl -X DELETE "https://localhost:5001/api/member/{orgId}/{memberId}" \
  -H "Authorization: Bearer {member-token}"

Expected: 403 Forbidden
```

#### **4. Business Rule Violation**
```bash
# Remove last owner
curl -X DELETE "https://localhost:5001/api/member/{orgId}/{lastOwnerId}" \
  -H "Authorization: Bearer {owner-token}"

Expected: 400 Bad Request - "Cannot remove last Owner"
```

---

## Best Practices

### **✅ Do's**

1. **Use Specific Exceptions**
   ```csharp
   throw new NotFoundException("Organization", id);
   // Not: throw new Exception("Not found");
   ```

2. **Provide Clear Messages**
   ```csharp
   throw new BadRequestException("User is already a member of this organization.");
   // Not: throw new BadRequestException("Invalid");
   ```

3. **Validate Early**
   ```csharp
   if (!ModelState.IsValid)
   {
       return BadRequest(ModelState);
   }
   // Before any database operations
   ```

4. **Re-throw Custom Exceptions**
   ```csharp
   catch (NotFoundException)
   {
       throw; // Preserve exception type
   }
   ```

5. **Log with Context**
   ```csharp
   _logger.LogError(ex, 
       "Error updating license {LicenseId} for organization {OrganizationId}", 
       licenseId, organizationId);
   ```

### **❌ Don'ts**

1. **Don't Expose Internal Details**
   ```csharp
   // Bad
   throw new Exception("SQL Server connection failed");
   
   // Good
   throw new BadRequestException("Failed to create organization.");
   ```

2. **Don't Swallow Exceptions**
   ```csharp
   // Bad
   catch (Exception ex)
   {
       // Silent failure
   }
   
   // Good
   catch (Exception ex)
   {
       _logger.LogError(ex, "Error occurred");
       throw new BadRequestException("Operation failed.");
   }
   ```

3. **Don't Use Generic Messages**
   ```csharp
   // Bad
   throw new BadRequestException("Error");
   
   // Good
   throw new BadRequestException("Organization name already exists.");
   ```

4. **Don't Return 403 When 404 is Appropriate**
   ```csharp
   // Bad - reveals existence
   if (!isMember)
   {
       throw new ForbiddenException("Access denied");
   }
   
   // Good - hides existence
   if (!isMember)
   {
       throw new NotFoundException("Organization", id);
   }
   ```

---

## File Locations

```
CaseGuard.Backend.Assignment/
├── Exceptions/
│   ├── BadRequestException.cs
│   ├── UnauthorizedException.cs
│   ├── ForbiddenException.cs
│   └── NotFoundException.cs
├── Middleware/
│   └── GlobalExceptionHandlingMiddleware.cs
└── Contracts/
    └── **/*Request.cs (Data Annotations)
```

---

## Related Documentation

- [Task 6: Authorization](Task6_Authorization_Implementation.md) - ForbiddenException usage
- [Task 2: DTOs](Task2_Request_Response_DTOs.md) - Data annotation validation

---

**Status**: ✅ Task 8 Complete - Comprehensive error handling and validation system with custom exceptions, global handler, and multi-layer validation fully implemented
