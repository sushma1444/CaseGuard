# How to Execute the Automated Tests

## Quick Start Guide

### Step 1: Start the Application

**Open Terminal 1** (PowerShell or Command Prompt):

```powershell
cd C:\Users\divya\Desktop\Sushma
dotnet run --project CaseGuard.Backend.Assignment
```

**Wait for this message:**
```
Now listening on: http://localhost:5000
```

**Keep this terminal open** - the application must stay running!

---

### Step 2: Run the Test Script

**Open Terminal 2** (NEW PowerShell window):

```powershell
cd C:\Users\divya\Desktop\Sushma
.\test-api.ps1
```

**That's it!** The script will:
- ✅ Test all API endpoints automatically
- ✅ Show pass/fail results in real-time
- ✅ Generate a detailed report

---

## What You'll See

### During Execution:
```
========================================
CaseGuard API Automated Test Suite
========================================

[0] Checking test data...
[1] Testing Authentication...
PASS - Login as Admin (POST /api/auth/login) - Status: 200
...

========================================
Test Summary
========================================
Total Tests: 18
Passed: 14
Failed: 4
Pass Rate: 77.78%

Detailed report saved to: AUTOMATED_TEST_REPORT.md
Test execution completed!
```

### After Execution:
- Check `AUTOMATED_TEST_REPORT.md` for detailed results
- Review which tests passed/failed
- Fix any issues and re-run

---

## Prerequisites Checklist

Before running tests, make sure:

- [ ] **PostgreSQL is running**
  - Check: `psql --version` (should show version)
  
- [ ] **Database exists and has migrations**
  ```powershell
  dotnet ef database update --project CaseGuard.Backend.Assignment
  ```

- [ ] **Test data is loaded** (optional but recommended)
  ```powershell
  .\setup-test-data.ps1
  ```
  OR run the complete setup:
  ```powershell
  .\setup-everything.ps1
  ```

---

## Troubleshooting

### Error: "Connection refused" or Status: 0
**Solution:** Application is not running. Go to Step 1 and start it.

### Error: "401 Unauthorized"
**Solution:** Token expired. The script will automatically login, but if it fails, check:
- Database has test users
- Application is running
- Database connection is working

### Error: "400 Bad Request" for Create Organization
**Solution:** This might be a duplicate name issue. The script now generates unique names automatically, but if it persists:
- Check application console logs for actual error
- Verify database constraints

### Error: "Cannot find path to test-api.ps1"
**Solution:** Make sure you're in the correct directory:
```powershell
cd C:\Users\divya\Desktop\Sushma
```

---

## Quick Commands Reference

```powershell
# Start application (Terminal 1)
dotnet run --project CaseGuard.Backend.Assignment

# Run tests (Terminal 2)
.\test-api.ps1

# Setup everything (one-time)
.\setup-everything.ps1

# Load test data only
.\setup-test-data.ps1

# View test report
notepad AUTOMATED_TEST_REPORT.md
```

---

## Testing Workflow

1. **Start application** → Terminal 1
2. **Run tests** → Terminal 2
3. **Check results** → `AUTOMATED_TEST_REPORT.md`
4. **Fix issues** → If any tests failed
5. **Re-run tests** → Verify fixes

---

## What Gets Tested

The script automatically tests:
- ✅ Authentication (Login, Claims)
- ✅ Health Check
- ✅ Admin Endpoints (Licenses)
- ✅ Organization Endpoints
- ✅ Member Endpoints
- ✅ Invitation Endpoints
- ✅ License Assignment Endpoints
- ✅ User Endpoints
- ✅ Pagination, Filtering, Sorting
- ✅ Error Handling

---

**Happy Testing! 🚀**
