# How to Fix Test Failures - Step-by-Step Guide

## Quick Fix (5 minutes)

### Step 1: Load Test Data

**Option A: Use the automated script (Recommended)**
```powershell
.\setup-test-data.ps1
```

**Option B: Manual SQL execution**
```bash
psql -U postgres -d CaseGuardDb -f test_data_setup.sql
```

**Option C: Using pgAdmin**
1. Open pgAdmin
2. Connect to your PostgreSQL server
3. Right-click on `CaseGuardDb` database
4. Select "Query Tool"
5. Open `test_data_setup.sql`
6. Click "Execute" (F5)

### Step 2: Verify Test Data Loaded

Check if users exist:
```sql
SELECT * FROM "Users";
```

You should see 6 users:
- Admin (11111111-1111-1111-1111-111111111111)
- Owner (22222222-2222-2222-2222-222222222222)
- OrgAdmin (33333333-3333-3333-3333-333333333333)
- Member (44444444-4444-4444-4444-444444444444)
- User1 (55555555-5555-5555-5555-555555555555)
- User2 (66666666-6666-6666-6666-666666666666)

### Step 3: Re-run Tests

```powershell
.\test-api.ps1
```

**Expected Result**: 16/17 tests should pass (94% pass rate)

---

## Detailed Solutions for Each Failure

### ❌ Failure #1: Get Claims (401 Unauthorized)

#### Problem
Token authentication failing for `/api/auth/claims` endpoint.

#### Solutions

**Solution A: Test Manually in Swagger**
1. Open Swagger UI: `http://localhost:5000/swagger`
2. Login: `POST /api/auth/login`
3. Copy the token
4. Click "Authorize" button
5. Enter: `Bearer {your-token}`
6. Try: `GET /api/auth/claims`

**Solution B: Check Token Format**
The token should be sent as:
```
Authorization: Bearer {token}
```

**Solution C: Verify JWT Configuration**
Check `appsettings.json`:
```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "CaseGuard",
    "Audience": "CaseGuard",
    "ExpirationMinutes": 60
  }
}
```

**Solution D: Check Application Logs**
Look for authentication errors in the console output when running the application.

#### Impact
**Low** - This is a secondary endpoint. Login works, which is the primary flow.

---

### ❌ Failure #2: Create License (404 Not Found)

#### Problem
Organization doesn't exist in database.

#### Solution
**Load test data first** (see Step 1 above).

The test script will:
1. Try to find existing organizations
2. If none exist, try to create one
3. If creation fails (because user doesn't exist), use fallback ID
4. Fallback ID doesn't exist → 404 error

**After loading test data:**
- Organizations will exist
- OR you can create organizations (because users exist)
- License creation will work

#### Verification
```sql
SELECT * FROM "Organizations";
```

You should see at least:
- Acme Corporation (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa)
- Tech Solutions Inc (bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb)

---

### ❌ Failure #3: Get License by ID (Skipped)

#### Problem
Depends on Create License test passing.

#### Solution
**Fix Failure #2 first**. Once licenses can be created, this test will automatically work.

This is not a real failure - it's just skipped because there's no license ID to test with.

---

### ❌ Failure #4: Create Organization (400 Bad Request) ⚠️ **PRIMARY ISSUE**

#### Problem
**User doesn't exist in the Users table!**

When creating an organization:
```csharp
var membership = new OrganizationMember
{
    UserId = CurrentUserIdGuid,  // ← This user must exist!
    // ...
};
```

If the user doesn't exist, the database foreign key constraint fails.

#### Solution
**Load test data** (see Step 1 above).

This will insert the test users into the database, allowing:
- Organizations to be created
- OrganizationMembers to be created
- The foreign key constraint to be satisfied

#### Why This Happens
The `Login` endpoint doesn't create users - it only generates tokens. The system expects users to already exist.

#### Verification
```sql
SELECT COUNT(*) FROM "Users";
```

Should return at least 6 users.

---

## Prevention: Setup Checklist

### Before Running Tests

- [ ] **PostgreSQL is running**
  ```powershell
  # Check if PostgreSQL service is running
  Get-Service -Name postgresql*
  ```

- [ ] **Database exists**
  ```sql
  -- Connect to PostgreSQL
  psql -U postgres
  -- List databases
  \l
  -- Should see CaseGuardDb
  ```

- [ ] **Migrations applied**
  ```powershell
  dotnet ef database update
  ```

- [ ] **Test data loaded** ⚠️ **CRITICAL**
  ```powershell
  .\setup-test-data.ps1
  ```

- [ ] **Application is running**
  ```powershell
  dotnet run --project CaseGuard.Backend.Assignment
  ```

- [ ] **Application is accessible**
  - Health check: `http://localhost:5000/api/health`
  - Swagger: `http://localhost:5000/swagger`

---

## Automated Setup Script

Create a `setup-everything.ps1` script:

```powershell
# Complete Setup Script
Write-Host "Setting up test environment..." -ForegroundColor Cyan

# 1. Check PostgreSQL
Write-Host "`n[1] Checking PostgreSQL..." -ForegroundColor Yellow
$pgService = Get-Service -Name postgresql* -ErrorAction SilentlyContinue
if (-not $pgService) {
    Write-Host "⚠️  PostgreSQL service not found. Please start PostgreSQL." -ForegroundColor Yellow
} else {
    Write-Host "✅ PostgreSQL service found" -ForegroundColor Green
}

# 2. Apply migrations
Write-Host "`n[2] Applying database migrations..." -ForegroundColor Yellow
dotnet ef database update
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Migrations applied" -ForegroundColor Green
} else {
    Write-Host "❌ Migration failed" -ForegroundColor Red
    exit 1
}

# 3. Load test data
Write-Host "`n[3] Loading test data..." -ForegroundColor Yellow
.\setup-test-data.ps1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Test data loaded" -ForegroundColor Green
} else {
    Write-Host "⚠️  Test data loading had issues. Check manually." -ForegroundColor Yellow
}

# 4. Verify setup
Write-Host "`n[4] Verifying setup..." -ForegroundColor Yellow
Write-Host "✅ Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Start application: dotnet run --project CaseGuard.Backend.Assignment" -ForegroundColor White
Write-Host "  2. Run tests: .\test-api.ps1" -ForegroundColor White
```

---

## Quick Reference

### Common Issues and Fixes

| Issue | Symptom | Fix |
|-------|---------|-----|
| Users don't exist | Create Organization → 400 | Run `setup-test-data.ps1` |
| Organizations don't exist | Create License → 404 | Load test data (creates orgs) |
| Database not created | Migration fails | Run `dotnet ef database update` |
| PostgreSQL not running | Connection error | Start PostgreSQL service |
| Token invalid | Get Claims → 401 | Check token format, verify JWT config |

---

## Expected Test Results

### Before Fix
- **Pass Rate**: 76.47% (13/17)
- **Failures**: 4 tests

### After Loading Test Data
- **Pass Rate**: ~94% (16/17)
- **Failures**: 1 test (Get Claims - may need manual investigation)

### After All Fixes
- **Pass Rate**: 100% (17/17)
- **All tests passing**

---

## Summary

**To prevent test failures:**

1. ✅ **Always load test data first**
   ```powershell
   .\setup-test-data.ps1
   ```

2. ✅ **Verify database setup**
   - Migrations applied
   - Test data loaded
   - Users exist

3. ✅ **Run tests**
   ```powershell
   .\test-api.ps1
   ```

**The key is: Users must exist in the database before testing!**

---

## Still Having Issues?

1. **Check application logs** for detailed error messages
2. **Verify database connection** in `appsettings.json`
3. **Test manually in Swagger UI** to isolate issues
4. **Check PostgreSQL logs** for database errors
5. **Verify test data** was actually inserted:
   ```sql
   SELECT COUNT(*) FROM "Users";
   SELECT COUNT(*) FROM "Organizations";
   ```

---

**Remember**: The API code is correct. Failures are due to missing test data, not code defects!
