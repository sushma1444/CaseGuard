# Setup Test Data Script
# This script helps set up test data for the API tests
# Note: This requires PostgreSQL command-line tools (psql) to be installed

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Data Setup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$dbName = "CaseGuardDb"
$dbUser = "postgres"
$dbPassword = "postgres"
$dbHost = "localhost"
$dbPort = "5432"

Write-Host "This script will set up test data in your PostgreSQL database." -ForegroundColor Yellow
Write-Host "Database: $dbName" -ForegroundColor Cyan
Write-Host "User: $dbUser" -ForegroundColor Cyan
Write-Host ""

# Check if psql is available
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlPath) {
    Write-Host "❌ ERROR: psql command not found!" -ForegroundColor Red
    Write-Host "Please install PostgreSQL client tools or use pgAdmin to run test_data_setup.sql manually." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To run manually:" -ForegroundColor Cyan
    Write-Host "  psql -U $dbUser -d $dbName -f test_data_setup.sql" -ForegroundColor White
    exit 1
}

Write-Host "✅ psql found at: $($psqlPath.Source)" -ForegroundColor Green
Write-Host ""

# Set PGPASSWORD environment variable
$env:PGPASSWORD = $dbPassword

Write-Host "Running test_data_setup.sql..." -ForegroundColor Yellow

try {
    $result = & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -f test_data_setup.sql 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Test data setup completed successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Test users created:" -ForegroundColor Cyan
        Write-Host "  - Admin: 11111111-1111-1111-1111-111111111111" -ForegroundColor White
        Write-Host "  - Owner: 22222222-2222-2222-2222-222222222222" -ForegroundColor White
        Write-Host "  - OrgAdmin: 33333333-3333-3333-3333-333333333333" -ForegroundColor White
        Write-Host "  - Member: 44444444-4444-4444-4444-444444444444" -ForegroundColor White
        Write-Host ""
        Write-Host "You can now run: .\test-api.ps1" -ForegroundColor Green
    } else {
        Write-Host "❌ Error running test_data_setup.sql" -ForegroundColor Red
        Write-Host $result -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    exit 1
}
finally {
    # Clear password from environment
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}
