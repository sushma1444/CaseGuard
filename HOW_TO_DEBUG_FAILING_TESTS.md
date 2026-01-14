# How to Debug the 4 Failing Tests

## Current Status
- ✅ 14 tests passing
- ❌ 4 tests failing (same as before)

Since you've loaded the test data, but tests are still failing, we need to check the **actual error messages** from the application.

---

## Step 1: Check Application Console Logs

The real error is logged in the terminal where `dotnet run` is executing.

### What to Look For:

1. **Open the terminal where the application is running**
2. **Look for error messages** when you run the tests
3. **Find lines that say:**
   - `fail:`
   - `error:`
   - `Exception:`
   - `Error creating organization`

### Example of what you might see:
```
fail: CaseGuard.Backend.Assignment.Controllers.OrganizationController[0]
      Error creating organization
      Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while updating the entries. See the inner exception for details.
      ---> Npgsql.PostgresException: 23503: insert or update on table "OrganizationMembers" violates foreign key constraint "FK_OrganizationMembers_Users_UserId"
```

This will tell us the **actual problem**.

---

## Step 2: Verify Test Data Was Loaded

Since `psql` might not be in your PATH, check the data using **pgAdmin** or verify through the application:

### In pgAdmin:
```sql
-- Check users
SELECT COUNT(*) FROM "Users";
-- Should return at least 6

-- Check owner user specifically
SELECT * FROM "Users" WHERE "Id" = '22222222-2222-2222-2222-222222222222';
-- Should return 1 row

-- Check owner's memberships
SELECT * FROM "OrganizationMembers" 
WHERE "UserId" = '22222222-2222-2222-2222-222222222222';
-- Should return at least 1 row
```

---

## Step 3: Common Issues and Solutions

### Issue 1: User Doesn't Exist
**Error in logs:** `foreign key constraint "FK_OrganizationMembers_Users_UserId"`

**Solution:**
```powershell
.\setup-test-data.ps1
```

### Issue 2: Duplicate Organization Name
**Error in logs:** `An organization with this name already exists`

**Solution:** The test script now generates unique names, but if this happens:
- Check what name was used
- The script should retry automatically

### Issue 3: Database Connection Issue
**Error in logs:** Connection timeout or database errors

**Solution:**
- Check PostgreSQL is running
- Verify connection string in `appsettings.json`

### Issue 4: Token/Authorization Issue
**Error in logs:** `UnauthorizedException` or token validation errors

**Solution:**
- Check if token is being sent correctly
- Verify JWT configuration

---

## Step 4: Get Detailed Error Information

The application logs the real error. To see it:

1. **Keep the application terminal visible**
2. **Run the test script in another terminal**
3. **Watch the application terminal for error messages**
4. **Copy the full error message** (especially the inner exception)

---

## Step 5: Quick Diagnostic

Run this to check if the application can see the data:

```powershell
# Test if owner user exists by trying to login and get organizations
# (This is what the test script does)
```

Or manually test in Swagger:
1. Login as Owner: `POST /api/auth/login`
   ```json
   {
     "userId": "22222222-2222-2222-2222-222222222222",
     "email": "owner@example.com",
     "role": "Owner"
   }
   ```
2. Copy the token
3. Try to create an organization: `POST /api/Organization`
   - Use token in Authorization header
   - Check the response and application logs

---

## What to Share

If you need help, share:
1. **The error message from the application console** (the full exception)
2. **The result of the SQL queries** (user count, etc.)
3. **What happens when you manually test in Swagger**

---

## Most Likely Issue

Based on the code, the most likely issue is:

**The user from the JWT token doesn't exist in the database.**

When creating an organization:
- Line 79: `UserId = CurrentUserIdGuid` - Gets userId from token
- Line 87: `_dbContext.OrganizationMembers.Add(membership)` - Tries to create membership
- Line 89: `await _dbContext.SaveChangesAsync()` - **FAILS HERE** if user doesn't exist

**Fix:** Make sure the test data SQL was executed successfully and the user exists.

---

## Next Steps

1. ✅ Check application console logs for actual error
2. ✅ Verify test data exists in database
3. ✅ Share the actual error message if you need help
4. ✅ Try manual test in Swagger to see detailed error
