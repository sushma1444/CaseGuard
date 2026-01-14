# PowerShell Scripts Guide

## Overview

You have **4 PowerShell scripts** in your project. Here's what each one does:

---

## 1. `test-api.ps1` - Main Test Script ⭐

**Purpose:** Runs automated tests against your API

**When to use:** After the application is running and you want to test all endpoints

**Usage:**
```powershell
.\test-api.ps1
```

**What it does:**
- Tests all API endpoints (Authentication, Licenses, Organizations, Members, etc.)
- Generates a test report (`AUTOMATED_TEST_REPORT.md`)
- Shows pass/fail status for each test

**Requirements:**
- Application must be running (`dotnet run`)
- Test data should be loaded (optional, but recommended)

---

## 2. `setup-test-data.ps1` - Load Test Data

**Purpose:** Loads test users, organizations, and memberships into the database

**When to use:** 
- First time setup
- After clearing the database
- When test data is missing

**Usage:**
```powershell
.\setup-test-data.ps1
```

**What it does:**
- Executes `test_data_setup.sql` to insert test data
- Creates 6 test users (Admin, Owner, OrgAdmin, Member, etc.)
- Creates 2 test organizations
- Creates organization memberships

**Requirements:**
- PostgreSQL must be running
- Database must exist
- `psql` must be in PATH (or use pgAdmin)

---

## 3. `setup-everything.ps1` - Complete Setup

**Purpose:** One-stop setup script that does everything

**When to use:** 
- First time setting up the project
- When you want to ensure everything is configured

**Usage:**
```powershell
.\setup-everything.ps1
```

**What it does:**
- Checks if PostgreSQL is running
- Checks if database exists
- Applies database migrations
- Loads test data
- Verifies everything is set up correctly

**Requirements:**
- PostgreSQL must be running
- `psql` must be in PATH

---

## 4. `verify-test-data.ps1` - Verify Data

**Purpose:** Checks if test data exists in the database

**When to use:** 
- To verify test data was loaded correctly
- To troubleshoot test failures

**Usage:**
```powershell
.\verify-test-data.ps1
```

**What it does:**
- Checks if application is running
- Provides SQL queries to verify data manually
- Shows instructions for checking data in pgAdmin

**Requirements:**
- Application should be running (optional)

---

## Quick Reference

| Script | Purpose | When to Use |
|--------|---------|-------------|
| `test-api.ps1` | Run tests | **Main script** - Use this to test your API |
| `setup-test-data.ps1` | Load test data | When test data is missing |
| `setup-everything.ps1` | Complete setup | First time setup or full reset |
| `verify-test-data.ps1` | Check data | To verify data exists |

---

## Typical Workflow

### First Time Setup:
```powershell
# 1. Setup everything
.\setup-everything.ps1

# 2. Start application (in one terminal)
dotnet run --project CaseGuard.Backend.Assignment

# 3. Run tests (in another terminal)
.\test-api.ps1
```

### Daily Testing:
```powershell
# 1. Start application (if not running)
dotnet run --project CaseGuard.Backend.Assignment

# 2. Run tests
.\test-api.ps1
```

### If Tests Fail:
```powershell
# 1. Verify test data exists
.\verify-test-data.ps1

# 2. If data is missing, load it
.\setup-test-data.ps1

# 3. Run tests again
.\test-api.ps1
```

---

## Which Script Should You Use?

**For testing:** Use `test-api.ps1` ⭐ (This is the main one!)

**For setup:** Use `setup-everything.ps1` (first time) or `setup-test-data.ps1` (just data)

**For verification:** Use `verify-test-data.ps1` (when troubleshooting)

---

## Summary

- **`test-api.ps1`** = The main test script you'll use most often
- **`setup-test-data.ps1`** = Loads test data into database
- **`setup-everything.ps1`** = Complete setup (database + data)
- **`verify-test-data.ps1`** = Verifies data exists

You only need to worry about **`test-api.ps1`** for regular testing! The others are for setup/verification.
