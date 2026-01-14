# Why 4 Tests Are Failing - Detailed Analysis

## Summary

**4 Tests Failing:**
1. ❌ **Get Claims** (401 Unauthorized)
2. ❌ **Create Organization** (400 Bad Request)
3. ❌ **Get All Organizations** (400 Bad Request)
4. ❌ **Get User Organizations** (400 Bad Request)

**14 Tests Passing** ✅

---

## 1. Get Claims - 401 Unauthorized

### Problem:
The `/api/auth/claims` endpoint requires authentication (`[Authorize]` attribute), but the test is getting a 401 error.

### Root Cause:
The test script is calling the endpoint immediately after login, but there might be an issue with:
- Token format/encoding
- Authorization header not being sent correctly
- Token validation failing

### Code Location:
- `AuthController.cs` line 92: `[Authorize]` attribute
- `test-api.ps1` line 160: Test call

### Why It's Failing:
The `GetClaims()` method requires a valid JWT token with proper claims. The 401 suggests:
1. Token might not be properly formatted in the Authorization header
2. Token might be expired (unlikely immediately after login)
3. JWT validation might be failing

### Solution:
Check the `Invoke-ApiRequest` function to ensure the Authorization header is set correctly:
```powershell
$headers["Authorization"] = "Bearer $Token"
```

**Status:** Likely a test script issue with token handling.

---

## 2. Create Organization - 400 Bad Request

### Problem:
Creating an organization returns `400 Bad Request` with message: "Failed to create organization. Please check your input and try again."

### Root Cause:
The error is caught in the generic exception handler (line 112), which means the actual error is being logged but not returned. Most likely causes:

1. **User doesn't exist in database** - The token contains a userId, but that user doesn't exist in the `Users` table, causing a foreign key constraint violation when creating `OrganizationMember`.

2. **Database constraint violation** - Some other database constraint is failing.

### Code Location:
- `OrganizationController.cs` line 45-113: `CreateOrganization` method
- Line 79: `UserId = CurrentUserIdGuid` - This is where it might fail if user doesn't exist

### Why It's Failing:
The code tries to create an `OrganizationMember` with `UserId = CurrentUserIdGuid`. If this user doesn't exist in the `Users` table, the foreign key constraint will fail.

### Solution:
1. **Check if the user exists:**
   ```sql
   SELECT * FROM "Users" WHERE "Id" = '22222222-2222-2222-2222-222222222222';
   ```

2. **If user doesn't exist, load test data:**
   ```powershell
   .\setup-test-data.ps1
   ```

3. **Check application console logs** for the actual database error.

**Status:** Likely missing user data in database.

---

## 3. Get All Organizations - 400 Bad Request

### Problem:
Getting all organizations returns `400 Bad Request` with message: "Failed to retrieve organizations. Please check your query parameters and try again."

### Root Cause:
This error is caught in the generic exception handler (line 267). The code has a fix for empty `organizationIds` lists (lines 218-225), but there might be another issue:

1. **Query exception** - Something in the LINQ query is failing
2. **Database connection issue**
3. **Authorization check failing** - The `EnsureUserIsMemberOfOrganizationAsync` might be throwing an exception

### Code Location:
- `OrganizationController.cs` line 142-269: `GetOrganizations` method
- Line 267: Generic exception handler

### Why It's Failing:
The method tries to:
1. Get user's organization memberships
2. Build a query with member counts and license counts
3. If the user has no memberships, it should return empty results (this was fixed)

But if there's an exception in the query building or database access, it gets caught and returns 400.

### Solution:
1. **Check application console logs** for the actual exception
2. **Verify user has memberships:**
   ```sql
   SELECT * FROM "OrganizationMembers" 
   WHERE "UserId" = '22222222-2222-2222-2222-222222222222';
   ```

3. **If no memberships, the query should still work** (returns empty list), so the issue might be elsewhere.

**Status:** Need to check application logs for actual error.

---

## 4. Get User Organizations - 400 Bad Request

### Problem:
Getting user organizations returns `400 Bad Request`.

### Root Cause:
Similar to #3, this is caught in a generic exception handler. The code has a fix for empty lists (line 118), but there might be another issue.

### Code Location:
- `UserController.cs` line 46-180: `GetUserOrganizations` method

### Why It's Failing:
The method:
1. Queries `OrganizationMembers` for the current user
2. Gets member counts and license counts
3. If no memberships, should return empty results

But if there's an exception (database error, query issue, etc.), it returns 400.

### Solution:
1. **Check application console logs** for the actual exception
2. **Verify the user exists and has memberships**
3. **Check if there's a database connection issue**

**Status:** Need to check application logs for actual error.

---

## Common Patterns

All 4 failures have something in common:

1. **Generic error messages** - The actual exceptions are being caught and replaced with generic messages
2. **Need application logs** - The real errors are logged but not shown in the API response
3. **Likely data-related** - Most failures seem related to missing data or database issues

---

## How to Debug

### Step 1: Check Application Console Logs
Look at the terminal where `dotnet run` is executing. You should see:
- Exception stack traces
- Database errors
- Lines starting with `fail:` or `error:`

### Step 2: Verify Test Data
```sql
-- Check if users exist
SELECT COUNT(*) FROM "Users";

-- Check if owner user exists
SELECT * FROM "Users" WHERE "Id" = '22222222-2222-2222-2222-222222222222';

-- Check if owner has memberships
SELECT * FROM "OrganizationMembers" 
WHERE "UserId" = '22222222-2222-2222-2222-222222222222';
```

### Step 3: Load Test Data (if missing)
```powershell
.\setup-test-data.ps1
```

### Step 4: Re-run Tests
```powershell
.\test-api.ps1
```

---

## Expected Fixes

1. **Get Claims (401)** - Fix token handling in test script
2. **Create Organization (400)** - Load test data (user must exist)
3. **Get All Organizations (400)** - Check logs, likely related to #2
4. **Get User Organizations (400)** - Check logs, likely related to #2

---

## Quick Fix Command

If test data is missing:
```powershell
.\setup-everything.ps1
```

Then re-run tests:
```powershell
.\test-api.ps1
```

This should fix at least 3 of the 4 failures (all except Get Claims, which might be a token handling issue).
