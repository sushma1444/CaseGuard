# Automated API Test Script for CaseGuard Backend API
# This script tests all endpoints and generates a comprehensive test report

$baseUrl = "http://localhost:5000"
$testResults = @()
$adminToken = $null
$ownerToken = $null
$memberToken = $null
$createdLicenseId = $null
$createdOrgId = $null
$createdMemberId = $null
$createdInvitationId = $null
$createdAssignmentId = $null

function Write-TestResult {
    param(
        [string]$TestName,
        [string]$Endpoint,
        [string]$Method,
        [int]$StatusCode,
        [bool]$Passed,
        [string]$Message = ""
    )
    
    $result = [PSCustomObject]@{
        TestName = $TestName
        Endpoint = $Endpoint
        Method = $Method
        StatusCode = $StatusCode
        Passed = $Passed
        Message = $Message
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    }
    
    $script:testResults += $result
    
    $status = if ($Passed) { "PASS" } else { "FAIL" }
    Write-Host "$status - $TestName ($Method $Endpoint) - Status: $StatusCode" -ForegroundColor $(if ($Passed) { "Green" } else { "Red" })
    if ($Message) {
        Write-Host "  Message: $Message" -ForegroundColor Yellow
    }
}

function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [string]$Token = $null,
        [hashtable]$QueryParams = @{}
    )
    
    $url = "$baseUrl$Endpoint"
    
    # Add query parameters
    if ($QueryParams.Count -gt 0) {
        $queryString = ($QueryParams.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join "&"
        $url += "?$queryString"
    }
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }
    
    try {
        $params = @{
            Uri = $url
            Method = $Method
            Headers = $headers
            UseBasicParsing = $true
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-WebRequest @params -ErrorAction Stop
        return @{
            StatusCode = $response.StatusCode
            Content = $response.Content | ConvertFrom-Json
            Success = $true
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $errorContent = $null
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errorContent = $reader.ReadToEnd() | ConvertFrom-Json
        }
        catch {
            $errorContent = $_.Exception.Message
        }
        
        return @{
            StatusCode = $statusCode
            Content = $errorContent
            Success = $false
            Error = $_.Exception.Message
        }
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CaseGuard API Automated Test Suite" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ============================================
# 0. SETUP - Ensure test users exist
# ============================================
Write-Host "`n[0] Checking test data..." -ForegroundColor Yellow

# Quick check if users exist (optional - won't fail if psql not available)
try {
    $env:PGPASSWORD = "sushma"
    $userCount = & psql -h localhost -U postgres -d CaseGuardDb -t -c "SELECT COUNT(*) FROM \"Users\";" 2>&1 | Where-Object { $_ -match '^\s*\d+\s*$' }
    $userCount = ($userCount -replace '\s', '').Trim()
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
    
    if ([int]$userCount -ge 6) {
        Write-Host "  [OK] Found $userCount users in database" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] Only $userCount users found (expected at least 6)" -ForegroundColor Yellow
        Write-Host "  [WARN] Some tests may fail. Run: .\setup-test-data.ps1" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  [WARN] Could not verify test data. If tests fail, run:" -ForegroundColor Yellow
    Write-Host "     .\setup-test-data.ps1" -ForegroundColor White
    Write-Host "  OR: .\setup-everything.ps1 (complete setup)" -ForegroundColor White
}
Write-Host ""

# ============================================
# 1. AUTHENTICATION TESTS
# ============================================
Write-Host "`n[1] Testing Authentication..." -ForegroundColor Yellow

# Test 1.1: Login as Admin
$loginBody = @{
    userId = "11111111-1111-1111-1111-111111111111"
    email = "admin@example.com"
    role = "Admin"
}
$response = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/login" -Body $loginBody
if ($response.Success -and $response.StatusCode -eq 200 -and $response.Content.token) {
    $adminToken = $response.Content.token
    Write-TestResult -TestName "Login as Admin" -Endpoint "/api/auth/login" -Method "POST" -StatusCode $response.StatusCode -Passed $true
} else {
    Write-TestResult -TestName "Login as Admin" -Endpoint "/api/auth/login" -Method "POST" -StatusCode $response.StatusCode -Passed $false -Message "Failed to get token"
}

    # Test 1.2: Get Claims - expects 200 with valid token and existing user
if ($adminToken) {
    $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/Auth/claims" -Token $adminToken
    if (-not $response.Success -or $response.StatusCode -ne 200) {
        $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/auth/claims" -Token $adminToken
    }
    $passed = $response.StatusCode -eq 200
    Write-TestResult -TestName "Get Claims" -Endpoint "/api/Auth/claims" -Method "GET" -StatusCode $response.StatusCode -Passed $passed
}

# Test 1.3: Login as Owner
$loginBody = @{
    userId = "22222222-2222-2222-2222-222222222222"
    email = "owner@example.com"
    role = "Owner"
}
$response = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/login" -Body $loginBody
if ($response.Success -and $response.StatusCode -eq 200 -and $response.Content.token) {
    $ownerToken = $response.Content.token
    Write-TestResult -TestName "Login as Owner" -Endpoint "/api/auth/login" -Method "POST" -StatusCode $response.StatusCode -Passed $true
}

# Test 1.4: Login as Member
$loginBody = @{
    userId = "44444444-4444-4444-4444-444444444444"
    email = "member@example.com"
    role = "Member"
}
$response = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/login" -Body $loginBody
if ($response.Success -and $response.StatusCode -eq 200 -and $response.Content.token) {
    $memberToken = $response.Content.token
    Write-TestResult -TestName "Login as Member" -Endpoint "/api/auth/login" -Method "POST" -StatusCode $response.StatusCode -Passed $true
}

# ============================================
# 2. HEALTH CHECK
# ============================================
Write-Host "`n[2] Testing Health Check..." -ForegroundColor Yellow
$response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/health"
Write-TestResult -TestName "Health Check" -Endpoint "/api/health" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)

# ============================================
# 3. ADMIN ENDPOINTS (LicenseController)
# ============================================
Write-Host "`n[3] Testing Admin Endpoints (LicenseController)..." -ForegroundColor Yellow

if (-not $adminToken) {
    Write-Host "  [WARN] Skipping admin tests - no admin token" -ForegroundColor Yellow
} else {
    # Test 3.1: Create License
    # First, try to get an existing organization or use test data organization
    $orgId = $null
    # Try to use test data organization first (most reliable)
    $orgId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
    Write-Host "  [INFO] Using test data organization: $orgId" -ForegroundColor Cyan
    
    # Verify organization exists by trying to get it
    $verifyResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/Organization/$orgId" -Token $adminToken
    if (-not $verifyResponse.Success -or $verifyResponse.StatusCode -eq 404) {
        # Organization doesn't exist, try to get any existing organization
        $orgResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/Organization" -Token $adminToken -QueryParams @{page=1; pageSize=1}
        if ($orgResponse.Success -and $orgResponse.Content.Items -and $orgResponse.Content.Items.Count -gt 0) {
            $orgId = $orgResponse.Content.Items[0].id
            Write-Host "  [OK] Using existing organization: $orgId" -ForegroundColor Green
        } else {
            Write-Host "  [WARN] No organizations found. License creation may fail." -ForegroundColor Yellow
        }
    } else {
        Write-Host "  [OK] Test data organization exists: $orgId" -ForegroundColor Green
    }
    
    $timestamp = Get-Date -Format 'yyyyMMddHHmmssfff'
    $random = Get-Random -Minimum 1000 -Maximum 9999
    $licenseBody = @{
        organizationId = $orgId
        name = "AutoTest License $timestamp-$random"
        startDate = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
        expirationDate = (Get-Date).AddYears(1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
        autoRenewalEnabled = $true
    }
    $response = Invoke-ApiRequest -Method "POST" -Endpoint "/api/License" -Body $licenseBody -Token $adminToken
    $passed = $response.Success -and $response.StatusCode -eq 201
    if ($passed -and $response.Content.id) {
        $createdLicenseId = $response.Content.id
        Write-Host "  [OK] Created license: $createdLicenseId" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] Error creating license: $($response.Error)" -ForegroundColor Yellow
        if ($response.Content) {
            $errorDetail = if ($response.Content.detail) { $response.Content.detail } else { ($response.Content | ConvertTo-Json) }
            Write-Host "  Detail: $errorDetail" -ForegroundColor Yellow
        }
    }
    Write-TestResult -TestName "Create License" -Endpoint "/api/License" -Method "POST" -StatusCode $response.StatusCode -Passed $passed
    
    # Test 3.2: Get All Licenses
    $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/License" -Token $adminToken -QueryParams @{page=1; pageSize=10}
    Write-TestResult -TestName "Get All Licenses" -Endpoint "/api/License" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    
    # Test 3.3: Get License by ID
    if ($createdLicenseId) {
        $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/License/$createdLicenseId" -Token $adminToken
        Write-TestResult -TestName "Get License by ID" -Endpoint "/api/License/{id}" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    } else {
        Write-TestResult -TestName "Get License by ID" -Endpoint "/api/License/{id}" -Method "GET" -StatusCode 0 -Passed $false -Message "Skipped - no license ID"
    }
    
    # Test 3.4: Update License
    if ($createdLicenseId) {
        $updateBody = @{
            name = "Updated License Name"
            autoRenewalEnabled = $false
        }
        $response = Invoke-ApiRequest -Method "PUT" -Endpoint "/api/License/$createdLicenseId" -Body $updateBody -Token $adminToken
        Write-TestResult -TestName "Update License" -Endpoint "/api/License/{id}" -Method "PUT" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    }
    
    # Test 3.5: Authorization - Non-Admin should fail
    if ($memberToken) {
        $response = Invoke-ApiRequest -Method "POST" -Endpoint "/api/License" -Body $licenseBody -Token $memberToken
        Write-TestResult -TestName "Admin Only - Non-Admin Access" -Endpoint "/api/License" -Method "POST" -StatusCode $response.StatusCode -Passed ($response.StatusCode -eq 403)
    }
}

# ============================================
# 4. ORGANIZATION ENDPOINTS
# ============================================
Write-Host "`n[4] Testing Organization Endpoints..." -ForegroundColor Yellow

if ($ownerToken) {
    # Test 4.1: Create Organization
    # Use a highly unique name with timestamp and random number to avoid conflicts
    $timestamp = Get-Date -Format 'yyyyMMddHHmmssfff'
    $random = Get-Random -Minimum 1000 -Maximum 9999
    $uniqueName = "AutoTest Org $timestamp-$random"
    $orgBody = @{
        name = $uniqueName
        description = "Created by automated test script at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    }
    $response = Invoke-ApiRequest -Method "POST" -Endpoint "/api/Organization" -Body $orgBody -Token $ownerToken
    $passed = $response.Success -and $response.StatusCode -eq 201
    if ($passed -and $response.Content.id) {
        $createdOrgId = $response.Content.id
        Write-Host "  [OK] Created organization: $createdOrgId" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] Error creating organization: $($response.Error)" -ForegroundColor Yellow
        if ($response.Content) {
            $errorDetail = if ($response.Content.detail) { $response.Content.detail } else { ($response.Content | ConvertTo-Json) }
            Write-Host "  Detail: $errorDetail" -ForegroundColor Yellow
            # If it's a name conflict, try with a different name (shouldn't happen with timestamp+random, but just in case)
            if ($errorDetail -like "*already exists*" -or $errorDetail -like "*duplicate*") {
                $timestamp = Get-Date -Format 'yyyyMMddHHmmssfff'
                $random = Get-Random -Minimum 10000 -Maximum 99999
                $uniqueName = "AutoTest Org $timestamp-$random"
                $orgBody.name = $uniqueName
                Write-Host "  🔄 Retrying with new name: $uniqueName" -ForegroundColor Cyan
                $response = Invoke-ApiRequest -Method "POST" -Endpoint "/api/Organization" -Body $orgBody -Token $ownerToken
                $passed = $response.Success -and $response.StatusCode -eq 201
                if ($passed -and $response.Content.id) {
                    $createdOrgId = $response.Content.id
                    Write-Host "  [OK] Retry successful: Created organization: $createdOrgId" -ForegroundColor Green
                }
            }
        }
    }
    Write-TestResult -TestName "Create Organization" -Endpoint "/api/Organization" -Method "POST" -StatusCode $response.StatusCode -Passed $passed
    
    # Test 4.2: Get All Organizations - expects 200 for successful retrieval
    $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/organization" -Token $ownerToken -QueryParams @{page=1; pageSize=10}
    $passed = $response.Success -and $response.StatusCode -eq 200
    Write-TestResult -TestName "Get All Organizations" -Endpoint "/api/organization" -Method "GET" -StatusCode $response.StatusCode -Passed $passed
    
    # Test 4.3: Get Organization by ID
    if ($createdOrgId) {
        $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/organization/$createdOrgId" -Token $ownerToken
        Write-TestResult -TestName "Get Organization by ID" -Endpoint "/api/organization/{id}" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    }
    
    # Test 4.4: Update Organization
    if ($createdOrgId) {
        $updateBody = @{
            name = "Updated Organization Name"
            description = "Updated description"
        }
        $response = Invoke-ApiRequest -Method "PUT" -Endpoint "/api/organization/$createdOrgId" -Body $updateBody -Token $ownerToken
        Write-TestResult -TestName "Update Organization" -Endpoint "/api/organization/{id}" -Method "PUT" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    }
}

# ============================================
# 5. MEMBER ENDPOINTS
# ============================================
Write-Host "`n[5] Testing Member Endpoints..." -ForegroundColor Yellow

if ($ownerToken -and $createdOrgId) {
    # Test 5.1: Invite Member
    $inviteBody = @{
        email = "newmember@example.com"
        role = "Member"
    }
    $response = Invoke-ApiRequest -Method "POST" -Endpoint "/api/member/$createdOrgId/invite" -Body $inviteBody -Token $ownerToken
    $passed = $response.Success -and $response.StatusCode -eq 201
    if ($passed -and $response.Content.id) {
        $createdInvitationId = $response.Content.id
    }
    Write-TestResult -TestName "Invite Member" -Endpoint "/api/member/{orgId}/invite" -Method "POST" -StatusCode $response.StatusCode -Passed $passed
    
    # Test 5.2: Get All Members
    $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/member/$createdOrgId" -Token $ownerToken -QueryParams @{page=1; pageSize=10}
    Write-TestResult -TestName "Get All Members" -Endpoint "/api/member/{orgId}" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    
    # Test 5.3: Get Member by ID (if we have a member)
    if ($response.Success -and $response.Content.items -and $response.Content.items.Count -gt 0) {
        $memberId = $response.Content.items[0].id
        $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/member/$createdOrgId/$memberId" -Token $ownerToken
        Write-TestResult -TestName "Get Member by ID" -Endpoint "/api/member/{orgId}/{memberId}" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    }
}

# ============================================
# 6. INVITATION ENDPOINTS
# ============================================
Write-Host "`n[6] Testing Invitation Endpoints..." -ForegroundColor Yellow

if ($ownerToken -and $createdOrgId) {
    # Test 6.1: Get All Invitations
    $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/invitation/$createdOrgId" -Token $ownerToken -QueryParams @{page=1; pageSize=10}
    Write-TestResult -TestName "Get All Invitations" -Endpoint "/api/invitation/{orgId}" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    
    # Test 6.2: Get Invitation by ID
    if ($createdInvitationId) {
        $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/invitation/$createdOrgId/$createdInvitationId" -Token $ownerToken
        Write-TestResult -TestName "Get Invitation by ID" -Endpoint "/api/invitation/{orgId}/{invitationId}" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    }
}

# ============================================
# 7. LICENSE ASSIGNMENT ENDPOINTS
# ============================================
Write-Host "`n[7] Testing License Assignment Endpoints..." -ForegroundColor Yellow

if ($ownerToken -and $createdLicenseId -and $createdOrgId) {
    # Test 7.1: Assign License
    $assignBody = @{
        licenseId = $createdLicenseId
        userId = "44444444-4444-4444-4444-444444444444"
    }
    $response = Invoke-ApiRequest -Method "POST" -Endpoint "/api/licenseassignment" -Body $assignBody -Token $ownerToken
    $passed = $response.Success -and $response.StatusCode -eq 201
    if ($passed -and $response.Content.id) {
        $createdAssignmentId = $response.Content.id
    }
    Write-TestResult -TestName "Assign License" -Endpoint "/api/licenseassignment" -Method "POST" -StatusCode $response.StatusCode -Passed $passed
    
    # Test 7.2: Get All Assignments
    $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/licenseassignment" -Token $ownerToken -QueryParams @{page=1; pageSize=10}
    Write-TestResult -TestName "Get All Assignments" -Endpoint "/api/licenseassignment" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    
    # Test 7.3: Get Assignment by ID
    if ($createdAssignmentId) {
        $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/licenseassignment/$createdAssignmentId" -Token $ownerToken
        Write-TestResult -TestName "Get Assignment by ID" -Endpoint "/api/licenseassignment/{id}" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    }
}

# ============================================
# 8. USER ENDPOINTS
# ============================================
Write-Host "`n[8] Testing User Endpoints..." -ForegroundColor Yellow

if ($memberToken) {
    # Test 8.1: Get User Organizations - expects 200 for successful retrieval
    $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/user/organizations" -Token $memberToken -QueryParams @{page=1; pageSize=10}
    $passed = $response.Success -and $response.StatusCode -eq 200
    Write-TestResult -TestName "Get User Organizations" -Endpoint "/api/user/organizations" -Method "GET" -StatusCode $response.StatusCode -Passed $passed
    
    # Test 8.2: Get Organization Details (if user has orgs)
    if ($response.Success -and $response.Content.items -and $response.Content.items.Count -gt 0) {
        $orgId = $response.Content.items[0].id
        $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/user/organizations/$orgId" -Token $memberToken
        Write-TestResult -TestName "Get User Organization Details" -Endpoint "/api/user/organizations/{id}" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    }
}

# ============================================
# 9. PAGINATION, FILTERING, SORTING TESTS
# ============================================
Write-Host "`n[9] Testing Pagination, Filtering, Sorting..." -ForegroundColor Yellow

if ($adminToken) {
    # Test 9.1: Pagination
    $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/License" -Token $adminToken -QueryParams @{page=1; pageSize=5}
    $hasPagination = $response.Success -and $response.Content -ne $null -and ($response.Content.Page -ne $null -or $response.Content.page -ne $null)
    Write-TestResult -TestName "Pagination Support" -Endpoint "/api/License" -Method "GET" -StatusCode $response.StatusCode -Passed $hasPagination
    
    # Test 9.2: Filtering
    $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/License" -Token $adminToken -QueryParams @{isActive=$true; expirationStatus="active"}
    Write-TestResult -TestName "Filtering Support" -Endpoint "/api/License" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
    
    # Test 9.3: Sorting
    $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/License" -Token $adminToken -QueryParams @{sortBy="expirationdate"; sortDirection="asc"}
    Write-TestResult -TestName "Sorting Support" -Endpoint "/api/License" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.Success -and $response.StatusCode -eq 200)
}

# ============================================
# 10. ERROR HANDLING TESTS
# ============================================
Write-Host "`n[10] Testing Error Handling..." -ForegroundColor Yellow

# Test 10.1: Unauthorized Access
$response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/License"
Write-TestResult -TestName "Unauthorized Access" -Endpoint "/api/License" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.StatusCode -eq 401)

# Test 10.2: Resource Not Found
if ($adminToken) {
    $response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/License/00000000-0000-0000-0000-000000000000" -Token $adminToken
    Write-TestResult -TestName "Resource Not Found" -Endpoint "/api/License/{id}" -Method "GET" -StatusCode $response.StatusCode -Passed ($response.StatusCode -eq 404)
}

# ============================================
# GENERATE TEST REPORT
# ============================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$totalTests = $testResults.Count
$passedTests = ($testResults | Where-Object { $_.Passed }).Count
$failedTests = $totalTests - $passedTests
$passRate = if ($totalTests -gt 0) { [math]::Round(($passedTests / $totalTests) * 100, 2) } else { 0 }

Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $failedTests" -ForegroundColor $(if ($failedTests -eq 0) { "Green" } else { "Red" })
Write-Host "Pass Rate: $passRate%" -ForegroundColor $(if ($passRate -ge 80) { "Green" } else { "Yellow" })

# Generate detailed report
$report = "# Automated Test Report - CaseGuard Backend API`n"
$report += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n`n"
$report += "## Summary`n"
$report += "- Total Tests: $totalTests`n"
$report += "- Passed: $passedTests`n"
$report += "- Failed: $failedTests`n"
$report += "- Pass Rate: $passRate%`n`n"
$report += "## Test Results`n`n"

foreach ($result in $testResults) {
    $status = if ($result.Passed) { "PASS" } else { "FAIL" }
    $report += "### $status - $($result.TestName)`n"
    $report += "- Endpoint: $($result.Method) $($result.Endpoint)`n"
    $report += "- Status Code: $($result.StatusCode)`n"
    $report += "- Time: $($result.Timestamp)`n"
    if ($result.Message) {
        $report += "- Message: $($result.Message)`n"
    }
    $report += "`n"
}

$report += "`n## Endpoints Tested`n`n"

$endpoints = $testResults | Select-Object -Unique Endpoint, Method
foreach ($endpoint in $endpoints) {
    $report += "- $($endpoint.Method) $($endpoint.Endpoint)`n"
}

$reportFile = "AUTOMATED_TEST_REPORT.md"
$report | Out-File -FilePath $reportFile -Encoding UTF8

Write-Host "`nDetailed report saved to: $reportFile" -ForegroundColor Green
Write-Host "`nTest execution completed!" -ForegroundColor Cyan