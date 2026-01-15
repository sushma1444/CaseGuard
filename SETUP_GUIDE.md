# Setup Guide for Evaluators

## Overview
This guide provides step-by-step instructions for setting up and running the CaseGuard Backend Assignment project.

---

## Prerequisites

Before you begin, ensure you have the following installed:

### Required Software
- ✅ **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- ✅ **PostgreSQL** - [Download](https://www.postgresql.org/download/)
- ✅ **IDE** (Optional but recommended):
  - Visual Studio 2022
  - JetBrains Rider
  - Visual Studio Code with C# extension

### Verify Installations

Check .NET version:
```bash
dotnet --version
```
Expected output: `8.0.x` or higher

Check PostgreSQL:
```bash
psql --version
```
Expected output: `psql (PostgreSQL) 14.x` or higher

---

## Setup Steps

### Step 1: Extract/Clone the Project

Extract the project files to your local machine or clone from repository.

```bash
cd CaseGuard
```

---

### Step 2: Configure Database Connection

Open `CaseGuard.Backend.Assignment/appsettings.json` and update the connection string:

**Current Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=CaseGuardDb;Username=postgres;Password=sushma"
  }
}
```

**Update with Your Credentials:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=CaseGuardDb;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

**Connection String Parameters:**
- `Host` - PostgreSQL server address (default: `localhost`)
- `Database` - Database name (keep as `CaseGuardDb` or change if needed)
- `Username` - PostgreSQL username (default: `postgres`)
- `Password` - **YOUR PostgreSQL password**
- `Port` - Optional, add `;Port=5432` if using non-default port

---

### Step 3: Create Database

Open **pgAdmin** or **psql** terminal and create the database:

**Option A: Using psql**
```bash
psql -U postgres
CREATE DATABASE "CaseGuardDb";
\q
```

**Option B: Using pgAdmin**
1. Open pgAdmin
2. Right-click "Databases"
3. Select "Create" → "Database"
4. Enter name: `CaseGuardDb`
5. Click "Save"

---

### Step 4: Install EF Core Tools

If you don't have EF Core tools installed:

```bash
dotnet tool install --global dotnet-ef
```

If already installed, verify:
```bash
dotnet ef --version
```

---

### Step 5: Apply Database Migrations

Navigate to the main project directory:

```bash
cd CaseGuard.Backend.Assignment
```

Apply migrations to create all database tables:

```bash
dotnet ef database update
```

**Expected Output:**
```
Build succeeded.
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand...
      CREATE TABLE "Users" (...)
      CREATE TABLE "Organizations" (...)
      CREATE TABLE "Licenses" (...)
      ...
Done.
```

**What This Does:**
- ✅ Creates all 6 tables (Users, Organizations, OrganizationMembers, Licenses, LicenseAssignments, Invitations)
- ✅ Sets up all foreign key relationships
- ✅ Creates all indexes for performance
- ✅ Applies all constraints

---

### Step 6: Build the Project

Build the solution to ensure everything compiles:

```bash
dotnet build
```

**Expected Output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### Step 7: Run the Application

Start the API server:

```bash
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

**The API is now running!**
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

---

## Testing the API

### Option 1: Using Swagger UI (Recommended)

1. Open your browser
2. Navigate to: `https://localhost:5001/swagger`
3. You'll see all available endpoints

**To Authenticate:**
1. Click on `/api/auth/login` endpoint
2. Click "Try it out"
3. Enter request body:
   ```json
   {
     "userId": "admin-user-123",
     "email": "admin@example.com",
     "role": "Admin"
   }
   ```
4. Click "Execute"
5. Copy the `accessToken` from response
6. Click the "Authorize" button at the top
7. Enter: `Bearer YOUR_TOKEN`
8. Click "Authorize"

Now you can test all authenticated endpoints!

---

### Option 2: Using cURL

**Get JWT Token:**
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"userId":"admin-123","email":"admin@example.com","role":"Admin"}'
```

**Use Token for Authenticated Requests:**
```bash
curl -X GET "https://localhost:5001/api/organization" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

### Option 3: Using Postman

1. Import the API into Postman
2. Create a new request to `POST https://localhost:5001/api/auth/login`
3. Set body to:
   ```json
   {
     "userId": "admin-123",
     "email": "admin@example.com",
     "role": "Admin"
   }
   ```
4. Copy the access token
5. For other requests, add Header:
   - Key: `Authorization`
   - Value: `Bearer YOUR_TOKEN`

---

## Verifying the Setup

### Check Health Endpoint

```bash
curl http://localhost:5000/api/health
```

**Expected Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2026-01-15T10:30:00Z"
}
```

### Check Database Connection

```bash
psql -U postgres -d CaseGuardDb
```

List tables:
```sql
\dt
```

**Expected Tables:**
```
 Schema |          Name               | Type  |  Owner
--------+-----------------------------+-------+----------
 public | Invitations                 | table | postgres
 public | LicenseAssignments          | table | postgres
 public | Licenses                    | table | postgres
 public | OrganizationMembers         | table | postgres
 public | Organizations               | table | postgres
 public | Users                       | table | postgres
 public | __EFMigrationsHistory       | table | postgres
```

Exit psql:
```sql
\q
```

---

## Common Issues & Solutions

### Issue 1: "dotnet-ef command not found"

**Solution:**
```bash
dotnet tool install --global dotnet-ef
```

Then restart your terminal.

---

### Issue 2: "password authentication failed"

**Solution:**
1. Verify PostgreSQL password
2. Update `appsettings.json` with correct password
3. Test connection:
   ```bash
   psql -U postgres
   ```

---

### Issue 3: "database does not exist"

**Solution:**
```bash
psql -U postgres
CREATE DATABASE "CaseGuardDb";
\q
```

Then run migrations again:
```bash
dotnet ef database update
```

---

### Issue 4: Port Already in Use

**Solution:**

If port 5000/5001 is in use, modify `Program.cs` or use:
```bash
dotnet run --urls "http://localhost:5100;https://localhost:5101"
```

---

### Issue 5: SSL Certificate Warning in Browser

**Solution:**

This is normal for local development. Click "Advanced" → "Proceed to localhost" in your browser.

Or trust the dev certificate:
```bash
dotnet dev-certs https --trust
```

---

## Project Structure

```
CaseGuard/
├── CaseGuard.Backend.Assignment/          # Main API project
│   ├── Controllers/                       # API endpoints (ALL IMPLEMENTED)
│   │   ├── AuthController.cs             # Authentication
│   │   ├── LicenseController.cs          # Admin license management
│   │   ├── OrganizationController.cs     # Organization CRUD
│   │   ├── MemberController.cs           # Member management
│   │   ├── InvitationController.cs       # Invitation system
│   │   ├── LicenseAssignmentController.cs# License assignments
│   │   └── UserController.cs             # User endpoints
│   ├── Data/
│   │   ├── ApplicationDbContext.cs       # EF Core DbContext
│   │   └── Configurations/               # Entity configurations
│   ├── Entities/                         # Domain models (6 entities)
│   ├── Services/                         # Business logic
│   │   ├── LicenseRenewalService.cs      # License renewal logic
│   │   ├── LicenseExpirationService.cs   # Expiration checks
│   │   └── LicenseRenewalBackgroundService.cs # Background job
│   ├── Helpers/                          # Authorization helpers
│   ├── Middleware/                       # Global exception handler
│   ├── Exceptions/                       # Custom exceptions
│   └── Migrations/                       # EF Core migrations
│
├── CaseGuard.Backend.Assignment.Contracts/# DTOs (Request/Response)
│   ├── Organizations/                    # Organization DTOs
│   ├── Members/                          # Member DTOs
│   ├── Invitations/                      # Invitation DTOs
│   ├── Licenses/                         # License DTOs
│   ├── Users/                            # User DTOs
│   └── Common/                           # Shared DTOs (Pagination)
│
├── Documentation/                         # Comprehensive task documentation
│   ├── Task1_Database_Schema_Design.md
│   ├── Task2_Request_Response_DTOs.md
│   ├── Task3_Admin_Endpoints.md
│   ├── Task4_Organization_Owner_Admin_Endpoints.md
│   ├── Task5_Regular_User_Endpoints.md
│   ├── Task6_Authorization_Implementation.md
│   ├── Task7_System_Jobs_License_AutoRenewal.md
│   ├── Task8_Error_Handling_Validation.md
│   └── Task9_Extra_Points_Features.md
│
└── README.md                              # Project overview
```

---

## Implementation Status

### ✅ All User Stories Completed

**Admin Stories (4/4):**
- ✅ Create license for organization
- ✅ View all licenses
- ✅ Update license properties
- ✅ Cancel/revoke license

**Organization Owner/Admin Stories (13/13):**
- ✅ Create organization
- ✅ Update organization
- ✅ Delete organization
- ✅ Invite users
- ✅ Remove users
- ✅ Assign licenses
- ✅ Unassign licenses
- ✅ View all members
- ✅ View member details
- ✅ Update member role
- ✅ View pending invitations
- ✅ View invitation details
- ✅ Cancel invitation

**Regular User Stories (4/4):**
- ✅ View all organizations
- ✅ View organization details
- ✅ Accept invitation
- ✅ Leave organization

**System Stories (2/2):**
- ✅ Auto-renew licenses (background job runs hourly)
- ✅ Authorization enforcement (JWT-based role checking)

### ✅ Extra Features Implemented

- ✅ **Pagination**: All list endpoints support page-based navigation
- ✅ **Filtering**: Dynamic filtering by entity properties
- ✅ **Sorting**: Multi-column sorting with direction control
- ✅ **Search**: Text search across relevant fields
- ✅ **Comprehensive Error Handling**: Custom exceptions with ProblemDetails format
- ✅ **Background Services**: Automatic license renewal every hour

---

## Testing Workflows

### Workflow 1: Admin Creates License

1. Login as Admin
2. Create a license:
   ```
   POST /api/license
   {
     "name": "Enterprise License",
     "organizationId": "{org-guid}",
     "autoRenewalEnabled": true
   }
   ```
3. Verify license created with 10-minute expiration
4. Wait for background job to renew (or check after 1 hour)

### Workflow 2: Organization Management

1. Login as regular user
2. Create organization:
   ```
   POST /api/organization
   {
     "name": "Tech Corp",
     "description": "Technology company"
   }
   ```
3. Invite member:
   ```
   POST /api/invitation/{orgId}
   {
     "email": "member@example.com",
     "role": "Member"
   }
   ```
4. View pending invitations:
   ```
   GET /api/invitation/{orgId}
   ```

### Workflow 3: User Accepts Invitation

1. Login as invited user (use same email from invitation)
2. View organizations:
   ```
   GET /api/user/organizations
   ```
3. Accept invitation:
   ```
   POST /api/user/invitation/{invitationId}/accept
   ```
4. View organization details:
   ```
   GET /api/user/organization/{orgId}
   ```

---

## Background Services

### License Auto-Renewal Service

**How It Works:**
- Runs automatically every **1 hour**
- Checks all licenses with `AutoRenewalEnabled = true`
- Renews licenses within 7 days of expiration
- Extends expiration by another 10 minutes

**Testing:**
1. Create a license with auto-renewal enabled
2. Wait 1 hour (or restart the application to trigger immediately)
3. Check license expiration date - should be extended
4. Check logs for renewal activity

**Logs to Look For:**
```
info: License with ID {LicenseId} renewed. New expiration: {NewExpiration}
info: Processed {Count} licenses for renewal
```

---

## API Endpoints Summary

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/login` | Get JWT token | No |
| GET | `/api/auth/claims` | View token claims | Yes |
| GET | `/api/health` | Health check | No |
| **Licenses (Admin Only)** |
| GET | `/api/license` | List all licenses | Admin |
| POST | `/api/license` | Create license | Admin |
| GET | `/api/license/{id}` | Get license details | Admin |
| PUT | `/api/license/{id}` | Update license | Admin |
| DELETE | `/api/license/{id}` | Cancel license | Admin |
| **Organizations** |
| GET | `/api/organization` | List organizations | User |
| POST | `/api/organization` | Create organization | User |
| GET | `/api/organization/{id}` | Get organization | Member |
| PUT | `/api/organization/{id}` | Update organization | Owner/Admin |
| DELETE | `/api/organization/{id}` | Delete organization | Owner |
| **Members** |
| GET | `/api/member/{orgId}` | List members | Member |
| GET | `/api/member/{orgId}/{memberId}` | Get member | Member |
| PUT | `/api/member/{orgId}/{memberId}` | Update role | Owner/Admin |
| DELETE | `/api/member/{orgId}/{memberId}` | Remove member | Owner/Admin |
| **Invitations** |
| GET | `/api/invitation/{orgId}` | List invitations | Owner/Admin |
| POST | `/api/invitation/{orgId}` | Invite member | Owner/Admin |
| GET | `/api/invitation/{orgId}/{invitationId}` | Get invitation | Owner/Admin |
| DELETE | `/api/invitation/{orgId}/{invitationId}` | Cancel invitation | Owner/Admin |
| **License Assignments** |
| GET | `/api/license-assignment` | List assignments | Admin |
| POST | `/api/license-assignment` | Assign license | Owner/Admin |
| DELETE | `/api/license-assignment/{assignmentId}` | Unassign license | Owner/Admin |
| **User** |
| GET | `/api/user/organizations` | My organizations | User |
| GET | `/api/user/organization/{orgId}` | Organization details | Member |
| POST | `/api/user/invitation/{invitationId}/accept` | Accept invitation | User |
| DELETE | `/api/user/organization/{orgId}/leave` | Leave organization | Member |

---

## Documentation

Complete documentation for all tasks is available in the `Documentation/` folder:

1. **Task 1**: Database schema, entities, relationships
2. **Task 2**: Request/Response DTOs with validation
3. **Task 3**: Admin license endpoints
4. **Task 4**: Organization owner/admin endpoints
5. **Task 5**: Regular user endpoints
6. **Task 6**: Authorization and JWT implementation
7. **Task 7**: Background jobs and license auto-renewal
8. **Task 8**: Error handling and validation system
9. **Task 9**: Pagination, filtering, and sorting features

Each document includes:
- Implementation details
- Code examples
- API usage examples
- Testing scenarios

---

## Support

If you encounter any issues during setup:

1. Check the [Common Issues](#common-issues--solutions) section
2. Review the relevant documentation in `Documentation/` folder
3. Verify all prerequisites are correctly installed
4. Check PostgreSQL is running: `pg_ctl status`
5. Review application logs for error messages

---

**Setup Complete!** 🎉

The project is now ready for evaluation. All 23 user stories are implemented and working end-to-end.
