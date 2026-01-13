# How to Test Task 7: Authorization System

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Setting Up Test Data](#setting-up-test-data)
3. [Testing Scenarios](#testing-scenarios)
4. [Using Swagger UI](#using-swagger-ui)
5. [Expected Results](#expected-results)

---

## Prerequisites

Before testing, ensure:
- ✅ Application is running (`dotnet run --project CaseGuard.Backend.Assignment`)
- ✅ Database is set up and migrations are applied
- ✅ Swagger UI is accessible at `http://localhost:5000`
- ✅ PostgreSQL is running

---

## Setting Up Test Data

### Step 1: Prepare Test Users

You'll need to create test users in the database. Here are sample GUIDs you can use:

**Admin User:**
- UserId: `11111111-1111-1111-1111-111111111111`
- Email: `admin@caseguard.com`
- Role: `Admin`

**Organization Owner:**
- UserId: `22222222-2222-2222-2222-222222222222`
- Email: `owner@example.com`
- Role: `Owner`

**Organization Admin:**
- UserId: `33333333-3333-3333-3333-333333333333`
- Email: `orgadmin@example.com`
- Role: `OrganizationAdmin`

**Regular Member:**
- UserId: `44444444-4444-4444-4444-444444444444`
- Email: `member@example.com`
- Role: `Member`

### Step 2: Insert Test Data into Database

You can use **pgAdmin 4** or **psql** to insert test data. Here's a SQL script:

```sql
-- Insert Test Users
INSERT INTO "Users" ("Id", "Email", "Name", "CreatedAt", "UpdatedAt")
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'admin@caseguard.com', 'System Admin', NOW(), NOW()),
    ('22222222-2222-2222-2222-222222222222', 'owner@example.com', 'Organization Owner', NOW(), NOW()),
    ('33333333-3333-3333-3333-333333333333', 'orgadmin@example.com', 'Org Admin', NOW(), NOW()),
    ('44444444-4444-4444-4444-444444444444', 'member@example.com', 'Regular Member', NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Insert Test Organization
INSERT INTO "Organizations" ("Id", "Name", "CreatedAt", "UpdatedAt")
VALUES 
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Test Organization', NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Insert Organization Memberships
INSERT INTO "OrganizationMembers" ("Id", "UserId", "OrganizationId", "Role", "JoinedAt", "CreatedAt", "UpdatedAt")
VALUES 
    -- Owner
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '22222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Owner', NOW(), NOW(), NOW()),
    -- Organization Admin
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', '33333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'OrganizationAdmin', NOW(), NOW(), NOW()),
    -- Member
    ('dddddddd-dddd-dddd-dddd-dddddddddddd', '44444444-4444-4444-4444-444444444444', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Member', NOW(), NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;
```

### Step 3: Using pgAdmin 4

1. Open **pgAdmin 4**
2. Connect to your PostgreSQL server
3. Navigate to: `Servers` → `PostgreSQL 16` → `Databases` → `CaseGuardDb`
4. Right-click on `CaseGuardDb` → **Query Tool**
5. Paste the SQL script above
6. Click **Execute** (F5)

### Step 4: Verify Data

Run this query to verify the data was inserted:

```sql
-- Check Users
SELECT "Id", "Email", "Name" FROM "Users";

-- Check Organizations
SELECT "Id", "Name" FROM "Organizations";

-- Check Organization Members
SELECT 
    om."Id",
    u."Email",
    o."Name" as "OrganizationName",
    om."Role"
FROM "OrganizationMembers" om
JOIN "Users" u ON om."UserId" = u."Id"
JOIN "Organizations" o ON om."OrganizationId" = o."Id";
```

---

## Testing Scenarios

### Scenario 1: Test JWT Token Generation (Login)

**Purpose**: Verify that login endpoint generates JWT tokens with correct claims.

#### Steps:

1. **Open Swagger UI**: Navigate to `http://localhost:5000`

2. **Test Admin Login**:
   - Find `POST /api/Auth/login`
   - Click **"Try it out"**
   - Enter request body:
     ```json
     {
       "userId": "11111111-1111-1111-1111-111111111111",
       "email": "admin@caseguard.com",
       "role": "Admin"
     }
     ```
   - Click **"Execute"**
   - **Expected**: Status 200, response contains JWT token
   - **Copy the token** for later use

3. **Test Owner Login**:
   - Use request body:
     ```json
     {
       "userId": "22222222-2222-2222-2222-222222222222",
       "email": "owner@example.com",
       "role": "Owner"
     }
     ```
   - **Expected**: Status 200, JWT token with Owner role

4. **Test Member Login**:
   - Use request body:
     ```json
     {
       "userId": "44444444-4444-4444-4444-444444444444",
       "email": "member@example.com",
       "role": "Member"
     }
     ```
   - **Expected**: Status 200, JWT token with Member role

#### Verify Claims:

1. **Authorize with Admin Token**:
   - Click **"Authorize"** button (top-right)
   - Enter: `Bearer <admin-token>`
   - Click **"Authorize"** → **"Close"**

2. **Get Claims**:
   - Find `GET /api/Auth/claims`
   - Click **"Try it out"** → **"Execute"**
   - **Expected Response**:
     ```json
     {
       "userId": "11111111-1111-1111-1111-111111111111",
       "email": "admin@caseguard.com",
       "role": "Admin",
       "allClaims": {
         "userId": "11111111-1111-1111-1111-111111111111",
         "email": "admin@caseguard.com",
         "role": "Admin",
         ...
       }
     }
     ```

---

### Scenario 2: Test ClaimsHelper Methods

**Purpose**: Verify that `ClaimsHelper` correctly extracts claims from JWT tokens.

#### Test via Claims Endpoint:

1. **Login as different users** and get tokens
2. **Authorize with each token** in Swagger
3. **Call `/api/Auth/claims`** for each user
4. **Verify**:
   - `userId` matches the logged-in user
   - `email` matches the logged-in user
   - `role` matches the logged-in user's role

#### Expected Results:

| User | Expected Role | Expected Email |
|------|---------------|----------------|
| Admin | `Admin` | `admin@caseguard.com` |
| Owner | `Owner` | `owner@example.com` |
| OrgAdmin | `OrganizationAdmin` | `orgadmin@example.com` |
| Member | `Member` | `member@example.com` |

---

### Scenario 3: Test BaseController Properties

**Purpose**: Verify that `BaseController` properties work correctly.

#### Note:
Since we haven't implemented controllers yet, you can test this by:
1. Creating a simple test endpoint
2. Or wait until we implement controllers (Tasks 8-13)

#### What to Test (when controllers are ready):

- `CurrentUserId` - Should return user ID as string
- `CurrentUserIdGuid` - Should return user ID as Guid
- `CurrentUserEmail` - Should return email
- `CurrentUserRole` - Should return role
- `IsAdmin` - Should return `true` for Admin, `false` for others
- `IsOwnerOrOrganizationAdmin` - Should return `true` for Owner/OrgAdmin/Admin
- `CurrentOrganizationId` - May return `null` if not in claims

---

### Scenario 4: Test Authorization Policies

**Purpose**: Verify that authorization policies work correctly.

#### Test AdminOnly Policy:

1. **Login as Admin** → Get token
2. **Authorize with Admin token**
3. **Call endpoint with `[Authorize(Policy = "AdminOnly")]`**
   - **Expected**: Status 200 (if endpoint exists)

4. **Login as Member** → Get token
5. **Authorize with Member token**
6. **Call same endpoint**
   - **Expected**: Status 403 Forbidden

#### Test OrganizationOwnerOrAdmin Policy:

1. **Login as Owner** → Get token
2. **Authorize with Owner token**
3. **Call endpoint with `[Authorize(Policy = "OrganizationOwnerOrAdmin")]`**
   - **Expected**: Status 200

4. **Login as Member** → Get token
5. **Authorize with Member token**
6. **Call same endpoint**
   - **Expected**: Status 403 Forbidden

#### Test Member Policy:

1. **Login as any user** → Get token
2. **Authorize with token**
3. **Call endpoint with `[Authorize]` or `[Authorize(Policy = "Member")]`**
   - **Expected**: Status 200 (any authenticated user)

4. **Don't authorize** (no token)
5. **Call same endpoint**
   - **Expected**: Status 401 Unauthorized

---

### Scenario 5: Test AuthorizationHelper (Database Checks)

**Purpose**: Verify that `AuthorizationHelper` correctly checks organization membership.

#### Note:
Since `AuthorizationHelper` is used in controllers, we'll test it when we implement controllers. However, you can verify the database structure:

#### Verify Organization Membership:

Run this SQL query:

```sql
-- Check if user is member of organization
SELECT 
    u."Email",
    o."Name" as "OrganizationName",
    om."Role"
FROM "OrganizationMembers" om
JOIN "Users" u ON om."UserId" = u."Id"
JOIN "Organizations" o ON om."OrganizationId" = o."Id"
WHERE u."Id" = '44444444-4444-4444-4444-444444444444'  -- Member user
  AND o."Id" = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';  -- Test Organization
```

**Expected**: Should return 1 row with Role = "Member"

#### Test Different Scenarios:

1. **User is member** → Should return membership record
2. **User is NOT member** → Should return no rows
3. **User is Owner** → Should return Role = "Owner"
4. **User is OrganizationAdmin** → Should return Role = "OrganizationAdmin"

---

## Using Swagger UI

### Step-by-Step Testing Process

#### 1. Open Swagger UI
- Navigate to: `http://localhost:5000`
- You should see the API documentation

#### 2. Test Login Endpoint
```
POST /api/Auth/login
Request Body:
{
  "userId": "11111111-1111-1111-1111-111111111111",
  "email": "admin@caseguard.com",
  "role": "Admin"
}
```

**Steps:**
1. Click on `POST /api/Auth/login`
2. Click **"Try it out"**
3. Paste the JSON above
4. Click **"Execute"**
5. Copy the `token` from response

#### 3. Authorize with Token
1. Click **"Authorize"** button (🔒 icon, top-right)
2. In the "Value" field, enter: `Bearer <your-token>`
   - Example: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
3. Click **"Authorize"**
4. Click **"Close"**
5. You should see a green checkmark (✅) next to "Authorize"

#### 4. Test Protected Endpoint
```
GET /api/Auth/claims
```

**Steps:**
1. Click on `GET /api/Auth/claims`
2. Click **"Try it out"**
3. Click **"Execute"**
4. **Expected**: Status 200, response contains your claims

#### 5. Test Without Authorization
1. Click **"Authorize"** again
2. Click **"Logout"** (to remove token)
3. Try `GET /api/Auth/claims` again
4. **Expected**: Status 401 Unauthorized

---

## Expected Results

### Success Scenarios

| Action | User Role | Expected Status | Expected Response |
|--------|----------|----------------|-------------------|
| Login | Any valid user | 200 OK | JWT token |
| Get Claims | Authenticated user | 200 OK | Claims object |
| Access Admin endpoint | Admin | 200 OK | Success |
| Access Admin endpoint | Member | 403 Forbidden | Error message |
| Access Org endpoint | Owner (member) | 200 OK | Success |
| Access Org endpoint | Member (not member) | 403 Forbidden | Error message |

### Error Scenarios

| Scenario | Expected Status | Expected Error |
|----------|----------------|----------------|
| No token provided | 401 Unauthorized | "Authorization header is missing" |
| Invalid token | 401 Unauthorized | "Invalid token" |
| Token expired | 401 Unauthorized | "Token has expired" |
| User not member | 403 Forbidden | "You do not have access to this organization" |
| Insufficient role | 403 Forbidden | "You do not have the required role" |

---

## Quick Test Checklist

### ✅ Basic Authentication
- [ ] Login with Admin user → Get token
- [ ] Login with Owner user → Get token
- [ ] Login with Member user → Get token
- [ ] Authorize with token in Swagger
- [ ] Get claims endpoint returns correct user info

### ✅ Authorization Policies
- [ ] Admin can access AdminOnly endpoints
- [ ] Member cannot access AdminOnly endpoints (403)
- [ ] Owner can access OrganizationOwnerOrAdmin endpoints
- [ ] Member cannot access OrganizationOwnerOrAdmin endpoints (403)
- [ ] Any authenticated user can access Member endpoints
- [ ] Unauthenticated user cannot access Member endpoints (401)

### ✅ Database Checks (When Controllers Are Ready)
- [ ] User who is member can access organization resources
- [ ] User who is NOT member gets 403 Forbidden
- [ ] Owner can perform owner-only actions
- [ ] OrganizationAdmin can perform admin actions
- [ ] Member cannot perform admin actions (403)

---

## Troubleshooting

### Issue: "401 Unauthorized" when token is provided
**Solution:**
- Check token format: Should be `Bearer <token>` or just `<token>`
- Verify token is not expired
- Check JWT secret key in `appsettings.json` matches

### Issue: "403 Forbidden" when user should have access
**Solution:**
- Verify user is a member of the organization in database
- Check user's role in `OrganizationMembers` table
- Verify the organization ID is correct

### Issue: Token not working in Swagger
**Solution:**
- Make sure you clicked "Authorize" and entered token correctly
- Check for green checkmark (✅) next to "Authorize" button
- Try logging out and re-authorizing

### Issue: Database connection errors
**Solution:**
- Verify PostgreSQL is running
- Check connection string in `appsettings.json`
- Verify database `CaseGuardDb` exists
- Check migrations are applied

---

## Next Steps

Once you've tested Task 7 (Authorization System), you're ready to:
- **Task 8**: Implement LicenseController (will use authorization)
- **Task 9**: Implement OrganizationController (will use authorization)
- **Task 10**: Implement MemberController (will use authorization)
- And so on...

Each controller will use the authorization system we just tested!

---

## Summary

This testing guide covers:
1. ✅ Setting up test data in the database
2. ✅ Testing JWT token generation and claims
3. ✅ Testing authorization policies
4. ✅ Using Swagger UI for testing
5. ✅ Expected results and error scenarios
6. ✅ Troubleshooting common issues

**Remember**: Some tests require controllers to be implemented (Tasks 8-13). For now, you can test:
- ✅ Login endpoint (JWT token generation)
- ✅ Claims endpoint (JWT token validation)
- ✅ Authorization policies (when controllers are ready)

---

**Happy Testing!** 🚀
