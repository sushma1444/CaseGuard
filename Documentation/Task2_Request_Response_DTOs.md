# Task 2: Design Request/Response DTOs - Documentation

## Overview
Complete set of Data Transfer Objects (DTOs) for all API endpoints in the Organization and License Management System.

---

## DTO Structure

All DTOs are organized by feature area in the `CaseGuard.Backend.Assignment.Contracts` project:

```
CaseGuard.Backend.Assignment.Contracts/
├── Auth/                    # Authentication DTOs (reference implementation)
├── Common/                  # Shared DTOs
├── Organizations/           # Organization management
├── Members/                 # Member management
├── Invitations/            # Invitation management
├── Licenses/               # License management
└── Users/                  # User-facing DTOs
```

---

## Common DTOs

### **PaginationRequest**
Base class for paginated list requests. Supports extra points requirements.

```csharp
- Page: int (1-based, min: 1, default: 1)
- PageSize: int (min: 1, max: 100, default: 10)
- SortBy: string? (field name)
- SortDirection: string? (asc/desc, default: "asc")
- SearchTerm: string? (search query)
```

### **PaginationResponse<T>**
Generic wrapper for paginated responses.

```csharp
- Items: List<T> (page data)
- TotalCount: int (total items)
- Page: int (current page)
- PageSize: int (items per page)
- TotalPages: int (calculated)
- HasNextPage: bool (calculated)
- HasPreviousPage: bool (calculated)
```

---

## Organizations DTOs

### **Requests**

#### **CreateOrganizationRequest**
```csharp
- Name: string [Required, MaxLength: 200]
- Description: string? [MaxLength: 1000]
```

#### **UpdateOrganizationRequest**
```csharp
- Name: string [Required, MaxLength: 200]
- Description: string? [MaxLength: 1000]
```

#### **GetOrganizationsRequest : PaginationRequest**
```csharp
+ Inherits pagination, sorting, filtering
```

### **Responses**

#### **OrganizationResponse**
```csharp
- Id: Guid
- Name: string
- Description: string?
- CreatedAt: DateTime
- UpdatedAt: DateTime
- MemberCount: int (computed)
- ActiveLicenseCount: int (computed)
- CurrentUserRole: string? (context-aware)
```

#### **GetOrganizationsResponse**
```csharp
- Organizations: PaginationResponse<OrganizationResponse>
```

---

## Members DTOs

### **Requests**

#### **InviteMemberRequest**
```csharp
- Email: string [Required, EmailAddress]
- Role: string [Required] (Owner, OrganizationAdmin, Member)
```

#### **GetMembersRequest : PaginationRequest**
```csharp
+ Inherits pagination, sorting, filtering
- OrganizationId: Guid [Required]
- Role: string? (filter by role)
- HasLicense: bool? (filter by license status)
```

#### **UpdateMemberRoleRequest**
```csharp
- Role: string [Required] (Owner, OrganizationAdmin, Member)
```

### **Responses**

#### **MemberResponse**
```csharp
- Id: Guid (membership ID)
- UserId: Guid
- Email: string
- Name: string
- Role: string
- JoinedAt: DateTime
- AssignedLicenseCount: int (computed)
- HasActiveLicense: bool (computed)
```

#### **InviteMemberResponse**
```csharp
- InvitationId: Guid
- Email: string
- OrganizationId: Guid
- OrganizationName: string
- Role: string
- ExpiresAt: DateTime
- Status: string
```

#### **GetMembersResponse**
```csharp
- Members: PaginationResponse<MemberResponse>
```

---

## Invitations DTOs

### **Requests**

#### **GetInvitationsRequest : PaginationRequest**
```csharp
+ Inherits pagination, sorting, filtering
- OrganizationId: Guid [Required]
- Status: string? (filter: Pending, Accepted, Cancelled, Expired)
```

### **Responses**

#### **InvitationResponse**
```csharp
- Id: Guid
- OrganizationId: Guid
- OrganizationName: string
- Email: string
- Role: string
- Status: InvitationStatus (Pending, Accepted, Cancelled, Expired)
- ExpiresAt: DateTime
- CreatedAt: DateTime
- AcceptedAt: DateTime?
- CancelledAt: DateTime?
```

#### **GetInvitationsResponse**
```csharp
- Invitations: PaginationResponse<InvitationResponse>
```

---

## Licenses DTOs

### **Requests**

#### **CreateLicenseRequest** (Admin only)
```csharp
- OrganizationId: Guid [Required]
- Name: string [Required, MaxLength: 200]
- StartDate: DateTime? (default: now)
- ExpirationDate: DateTime? (default: StartDate + 10 minutes)
- AutoRenewalEnabled: bool (default: false)
```

#### **UpdateLicenseRequest** (Admin only)
```csharp
- Name: string? [MaxLength: 200]
- ExpirationDate: DateTime?
- AutoRenewalEnabled: bool?
- IsActive: bool?
```

#### **GetLicensesRequest : PaginationRequest**
```csharp
+ Inherits pagination, sorting, filtering
- OrganizationId: Guid? (admin: optional, org-owner: auto-filled)
- IsActive: bool? (filter by status)
- AutoRenewalEnabled: bool? (filter by auto-renewal)
- ExpiringBefore: DateTime? (filter expiring soon)
```

#### **AssignLicenseRequest**
```csharp
- LicenseId: Guid [Required]
- UserId: Guid [Required]
```

#### **GetLicenseAssignmentsRequest : PaginationRequest**
```csharp
+ Inherits pagination, sorting, filtering
- OrganizationId: Guid? 
- LicenseId: Guid?
- UserId: Guid?
- IsActive: bool? (filter active/unassigned)
```

### **Responses**

#### **LicenseResponse**
```csharp
- Id: Guid
- OrganizationId: Guid
- OrganizationName: string
- Name: string
- StartDate: DateTime
- ExpirationDate: DateTime
- AutoRenewalEnabled: bool
- IsActive: bool
- CreatedAt: DateTime
- UpdatedAt: DateTime
- IsExpired: bool (computed)
- DaysUntilExpiration: int (computed)
- AssignedUserCount: int (computed)
```

#### **GetLicensesResponse**
```csharp
- Licenses: PaginationResponse<LicenseResponse>
```

#### **LicenseAssignmentResponse**
```csharp
- Id: Guid
- LicenseId: Guid
- LicenseName: string
- UserId: Guid
- UserEmail: string
- UserName: string
- OrganizationId: Guid
- AssignedAt: DateTime
- UnassignedAt: DateTime?
- IsActive: bool (computed)
```

#### **GetLicenseAssignmentsResponse**
```csharp
- Assignments: PaginationResponse<LicenseAssignmentResponse>
```

---

## Users DTOs

### **Requests**

#### **GetUserOrganizationsRequest : PaginationRequest**
```csharp
+ Inherits pagination, sorting, filtering
- Role: string? (filter by user's role)
- HasActiveLicense: bool? (filter by license status)
```

#### **AcceptInvitationRequest**
```csharp
- InvitationId: Guid [Required]
```

### **Responses**

#### **UserOrganizationResponse**
```csharp
- OrganizationId: Guid
- OrganizationName: string
- Description: string?
- Role: string (user's role in this org)
- JoinedAt: DateTime
- MemberCount: int
- HasActiveLicense: bool
- AssignedLicenseCount: int
```

#### **GetUserOrganizationsResponse**
```csharp
- Organizations: PaginationResponse<UserOrganizationResponse>
```

#### **AcceptInvitationResponse**
```csharp
- OrganizationId: Guid
- OrganizationName: string
- Role: string
- MembershipId: Guid
- JoinedAt: DateTime
```

---

## Validation Rules

### **Data Annotations Used**
- `[Required]` - Field must have a value
- `[StringLength]` - Maximum character length
- `[Range]` - Numeric range validation
- `[EmailAddress]` - Valid email format
- `[RegularExpression]` - Pattern matching

### **Common Validations**
```csharp
Name fields: MaxLength(200)
Description fields: MaxLength(1000)
Email fields: MaxLength(256) + EmailAddress
Page numbers: Range(1, int.MaxValue)
PageSize: Range(1, 100)
```

---

## Computed Properties

DTOs include business-logic computed properties for better UX:

### **Organization**
- `MemberCount` - Total members
- `ActiveLicenseCount` - Active licenses count
- `CurrentUserRole` - Context-aware role info

### **Member**
- `AssignedLicenseCount` - Number of licenses
- `HasActiveLicense` - Boolean flag

### **License**
- `IsExpired` - DateTime.UtcNow > ExpirationDate
- `DaysUntilExpiration` - Calculated remaining time
- `AssignedUserCount` - Number of assigned users

### **LicenseAssignment**
- `IsActive` - UnassignedAt == null

---

## Extra Points Features

### **Pagination Support**
✅ All list endpoints support pagination via `PaginationRequest`
- Page-based navigation (1-indexed)
- Configurable page size (1-100 items)
- Total count and page calculations

### **Filtering Support**
✅ Context-specific filters on list endpoints:
- Organizations: Search by name/description
- Members: Filter by role, license status
- Invitations: Filter by status
- Licenses: Filter by active status, auto-renewal, expiration

### **Sorting Support**
✅ All list endpoints support sorting:
- `SortBy` - Field name to sort by
- `SortDirection` - "asc" or "desc"
- Default sorting configured per endpoint

---

## Files Location

```
CaseGuard.Backend.Assignment.Contracts/
├── Common/
│   ├── PaginationRequest.cs
│   └── PaginationResponse.cs
├── Organizations/
│   ├── Requests/
│   │   ├── CreateOrganizationRequest.cs
│   │   ├── UpdateOrganizationRequest.cs
│   │   └── GetOrganizationsRequest.cs
│   └── Responses/
│       ├── OrganizationResponse.cs
│       └── GetOrganizationsResponse.cs
├── Members/
│   ├── Requests/
│   │   ├── InviteMemberRequest.cs
│   │   ├── GetMembersRequest.cs
│   │   └── UpdateMemberRoleRequest.cs
│   └── Responses/
│       ├── MemberResponse.cs
│       ├── InviteMemberResponse.cs
│       └── GetMembersResponse.cs
├── Invitations/
│   ├── Requests/
│   │   └── GetInvitationsRequest.cs
│   └── Responses/
│       ├── InvitationResponse.cs
│       └── GetInvitationsResponse.cs
├── Licenses/
│   ├── Requests/
│   │   ├── CreateLicenseRequest.cs
│   │   ├── UpdateLicenseRequest.cs
│   │   ├── GetLicensesRequest.cs
│   │   ├── AssignLicenseRequest.cs
│   │   └── GetLicenseAssignmentsRequest.cs
│   └── Responses/
│       ├── LicenseResponse.cs
│       ├── GetLicensesResponse.cs
│       ├── LicenseAssignmentResponse.cs
│       └── GetLicenseAssignmentsResponse.cs
└── Users/
    ├── Requests/
    │   ├── GetUserOrganizationsRequest.cs
    │   └── AcceptInvitationRequest.cs
    └── Responses/
        ├── UserOrganizationResponse.cs
        ├── GetUserOrganizationsResponse.cs
        └── AcceptInvitationResponse.cs
```

---

**Status**: ✅ Task 2 Complete - All DTOs designed and implemented with validation, pagination, filtering, and sorting support
