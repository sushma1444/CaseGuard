# Task 1: Database Schema Design - Documentation

## Overview
Complete database schema for the Organization and License Management System using Entity Framework Core with PostgreSQL.

---

## Entity Relationships

```
User
├── Has many OrganizationMembers (memberships in different organizations)
├── Has many Invitations (received invitations)
└── Has many LicenseAssignments (licenses assigned to user)

Organization
├── Has many OrganizationMembers (members in the organization)
├── Has many Licenses (organization's subscriptions)
└── Has many Invitations (pending invitations to join)

OrganizationMember (Join table: User ↔ Organization)
├── Belongs to User
├── Belongs to Organization
├── Has Role (Owner, OrganizationAdmin, Member)
└── Has many LicenseAssignments

License
├── Belongs to Organization
└── Has many LicenseAssignments (users assigned this license)

LicenseAssignment
├── Belongs to License
├── Belongs to User
└── Belongs to OrganizationMember

Invitation
├── Belongs to Organization
└── Optional reference to User (if user exists in system)
```

---

## Entity Details

### **User**
```csharp
- Id: Guid (PK, auto-generated)
- Email: string (required, max 256, unique index)
- Name: string (required, max 200)
- CreatedAt: DateTime (required)
- UpdatedAt: DateTime (required)
```

### **Organization**
```csharp
- Id: Guid (PK, auto-generated)
- Name: string (required, max 200, indexed)
- Description: string? (optional, max 1000)
- CreatedAt: DateTime (required)
- UpdatedAt: DateTime (required)
```

### **OrganizationMember**
```csharp
- Id: Guid (PK, auto-generated)
- UserId: Guid (FK to User)
- OrganizationId: Guid (FK to Organization)
- Role: string (default: "Member")
- JoinedAt: DateTime (required)
- CreatedAt: DateTime (required)
- UpdatedAt: DateTime (required)
```

**Roles**: Owner, OrganizationAdmin, Member

### **License**
```csharp
- Id: Guid (PK, auto-generated)
- OrganizationId: Guid (FK to Organization, indexed)
- Name: string (required, max 200)
- StartDate: DateTime (required)
- ExpirationDate: DateTime (required)
- AutoRenewalEnabled: bool (default: false, indexed when true)
- IsActive: bool (default: true)
- CreatedAt: DateTime (required)
- UpdatedAt: DateTime (required)
```

**Business Rules**:
- Default expiration: 10 minutes from creation
- Auto-renewal extends for another 10 minutes

### **LicenseAssignment**
```csharp
- Id: Guid (PK, auto-generated)
- LicenseId: Guid (FK to License)
- UserId: Guid (FK to User)
- OrganizationMemberId: Guid (FK to OrganizationMember)
- AssignedAt: DateTime (required)
- CreatedAt: DateTime (required)
- UnassignedAt: DateTime? (optional)
- IsActive: computed property (UnassignedAt == null)
```

### **Invitation**
```csharp
- Id: Guid (PK, auto-generated)
- OrganizationId: Guid (FK to Organization)
- Email: string (required, max 256)
- UserId: Guid? (optional FK to User)
- Role: string (default: "Member")
- Status: InvitationStatus (Pending, Accepted, Cancelled, Expired)
- ExpiresAt: DateTime (required)
- CreatedAt: DateTime (required)
- UpdatedAt: DateTime (required)
- AcceptedAt: DateTime? (optional)
- CancelledAt: DateTime? (optional)
```

---

## Database Indexes

### Performance Optimization
1. **Users.Email** - Unique index (authentication & lookup)
2. **Organizations.Name** - Non-unique index (search/filter)
3. **Licenses.OrganizationId** - Non-unique index (queries by organization)
4. **Licenses (OrganizationId, IsActive, ExpirationDate)** - Composite index (active licenses query)
5. **Licenses.AutoRenewalEnabled** - Filtered index (background job queries)

---

## Delete Behaviors

### Cascade Deletes
- Delete **User** → Cascades to OrganizationMembers, LicenseAssignments
- Delete **Organization** → Cascades to OrganizationMembers, Licenses, Invitations
- Delete **License** → Cascades to LicenseAssignments

### Set Null
- Delete **User** → Sets Invitation.UserId to NULL (preserves invitation record)

---

## Migration Status

✅ **Initial Migration Created**: `20260112220812_InitialCreate`
- All 6 entities defined
- All relationships configured
- All indexes created
- Ready for `dotnet ef database update`

---

## Configuration Files Location

```
CaseGuard.Backend.Assignment/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Configurations/
│       ├── UserConfiguration.cs
│       ├── OrganizationConfiguration.cs
│       ├── OrganizationMemberConfiguration.cs
│       ├── LicenseConfiguration.cs
│       ├── LicenseAssignmentConfiguration.cs
│       └── InvitationConfiguration.cs
├── Entities/
│   ├── User.cs
│   ├── Organization.cs
│   ├── OrganizationMember.cs
│   ├── License.cs
│   ├── LicenseAssignment.cs
│   └── Invitation.cs
└── Migrations/
    ├── 20260112220812_InitialCreate.cs
    ├── 20260112220812_InitialCreate.Designer.cs
    └── ApplicationDbContextModelSnapshot.cs
```

---

## Database Commands

```bash
# Create new migration
dotnet ef migrations add MigrationName --project CaseGuard.Backend.Assignment

# Apply migrations to database
dotnet ef database update --project CaseGuard.Backend.Assignment

# Remove last migration (if not applied)
dotnet ef migrations remove --project CaseGuard.Backend.Assignment

# Generate SQL script
dotnet ef migrations script --project CaseGuard.Backend.Assignment
```

---

**Status**: ✅ Task 1 Complete - Database schema fully designed and ready for implementation
