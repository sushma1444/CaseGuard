# Quick script to verify test data exists
# This uses the application's database connection

Write-Host "Verifying test data..." -ForegroundColor Cyan
Write-Host ""

# Check if application is running
try {
    $healthCheck = Invoke-WebRequest -Uri "http://localhost:5000/api/health" -UseBasicParsing -ErrorAction Stop
    Write-Host "[OK] Application is running" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Application is not running!" -ForegroundColor Red
    Write-Host "Start it with: dotnet run --project CaseGuard.Backend.Assignment" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "To verify test data, check your database directly:" -ForegroundColor Cyan
Write-Host ""
Write-Host "In pgAdmin or psql, run:" -ForegroundColor Yellow
Write-Host "  SELECT COUNT(*) FROM ""Users"";" -ForegroundColor White
Write-Host "  SELECT * FROM ""Users"" WHERE ""Id"" = '22222222-2222-2222-2222-222222222222';" -ForegroundColor White
Write-Host "  SELECT * FROM ""OrganizationMembers"" WHERE ""UserId"" = '22222222-2222-2222-2222-222222222222';" -ForegroundColor White
Write-Host ""
Write-Host "Expected:" -ForegroundColor Cyan
Write-Host "  - At least 6 users" -ForegroundColor White
Write-Host "  - Owner user (22222222-...) should exist" -ForegroundColor White
Write-Host "  - Owner should have at least 1 membership" -ForegroundColor White
Write-Host ""
Write-Host "If data is missing, run:" -ForegroundColor Yellow
Write-Host "  .\setup-test-data.ps1" -ForegroundColor Green
