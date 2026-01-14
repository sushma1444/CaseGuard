# Complete Setup Script - Prevents Test Failures
# This script sets up everything needed for testing

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Complete Test Environment Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$errors = @()

# 1. Check PostgreSQL Connection
Write-Host "[1] Checking PostgreSQL connection..." -ForegroundColor Yellow
try {
    $env:PGPASSWORD = "postgres"
    $result = & psql -h localhost -U postgres -d postgres -c "SELECT 1;" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ PostgreSQL connection successful" -ForegroundColor Green
    } else {
        Write-Host "  ❌ PostgreSQL connection failed" -ForegroundColor Red
        $errors += "PostgreSQL connection failed"
    }
} catch {
    Write-Host "  ❌ PostgreSQL not accessible. Is it running?" -ForegroundColor Red
    $errors += "PostgreSQL not accessible"
} finally {
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

# 2. Check if database exists
Write-Host "`n[2] Checking database..." -ForegroundColor Yellow
try {
    $env:PGPASSWORD = "postgres"
    $result = & psql -h localhost -U postgres -d CaseGuardDb -c "SELECT 1;" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ Database 'CaseGuardDb' exists" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️  Database 'CaseGuardDb' not found. Run migrations first:" -ForegroundColor Yellow
        Write-Host "     dotnet ef database update" -ForegroundColor White
        $errors += "Database not found"
    }
} catch {
    Write-Host "  ❌ Cannot access database" -ForegroundColor Red
    $errors += "Cannot access database"
} finally {
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

# 3. Apply migrations (if needed)
Write-Host "`n[3] Checking migrations..." -ForegroundColor Yellow
Write-Host "  Note: Run 'dotnet ef database update' if migrations are not applied" -ForegroundColor Cyan

# 4. Load test data
Write-Host "`n[4] Loading test data..." -ForegroundColor Yellow
if (Test-Path "setup-test-data.ps1") {
    & .\setup-test-data.ps1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ Test data loaded successfully" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️  Test data loading had issues" -ForegroundColor Yellow
        $errors += "Test data loading failed"
    }
} else {
    Write-Host "  ❌ setup-test-data.ps1 not found" -ForegroundColor Red
    $errors += "Setup script not found"
}

# 5. Verify test data
Write-Host "`n[5] Verifying test data..." -ForegroundColor Yellow
try {
    $env:PGPASSWORD = "postgres"
    $userCount = & psql -h localhost -U postgres -d CaseGuardDb -t -c "SELECT COUNT(*) FROM \"Users\";" 2>&1
    $userCount = $userCount.Trim()
    
    if ([int]$userCount -ge 6) {
        Write-Host "  ✅ Found $userCount users in database" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️  Only found $userCount users (expected at least 6)" -ForegroundColor Yellow
        Write-Host "     Run: .\setup-test-data.ps1" -ForegroundColor White
        $errors += "Insufficient test users"
    }
    
    $orgCount = & psql -h localhost -U postgres -d CaseGuardDb -t -c "SELECT COUNT(*) FROM \"Organizations\";" 2>&1
    $orgCount = $orgCount.Trim()
    
    if ([int]$orgCount -ge 2) {
        Write-Host "  ✅ Found $orgCount organizations in database" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️  Only found $orgCount organizations (expected at least 2)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ❌ Cannot verify test data" -ForegroundColor Red
    $errors += "Cannot verify test data"
} finally {
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Setup Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($errors.Count -eq 0) {
    Write-Host "✅ Setup completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Start application:" -ForegroundColor White
    Write-Host "     dotnet run --project CaseGuard.Backend.Assignment" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  2. Run tests:" -ForegroundColor White
    Write-Host "     .\test-api.ps1" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Expected: 16-17 tests should pass (94-100%)" -ForegroundColor Green
} else {
    Write-Host "⚠️  Setup completed with $($errors.Count) issue(s):" -ForegroundColor Yellow
    foreach ($error in $errors) {
        Write-Host "  - $error" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "Please fix the issues above and run this script again." -ForegroundColor Cyan
}
