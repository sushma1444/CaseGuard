# Task 7: Implement System Jobs (License Auto-Renewal) - Documentation

## Overview
Complete implementation of automated license management system with background jobs for license auto-renewal and expiration checking.

---

## System Architecture

### **Components**

```
LicenseRenewalBackgroundService (Hosted Service)
    ↓ (Every 1 hour)
    ├── LicenseExpirationService
    │   └── Invalidates expired licenses
    │
    └── LicenseRenewalService
        └── Renews eligible licenses
```

### **Business Requirements**

1. **License Expiration:**
   - Default expiration: 10 minutes (for testing)
   - Expired licenses become invalid
   - IsActive set to false when expired

2. **Auto-Renewal:**
   - Licenses with AutoRenewalEnabled = true
   - Automatically renewed before expiration
   - Extended by original license duration
   - Renewal window: 7 days before expiration

3. **Background Processing:**
   - Checks run automatically every hour
   - No manual intervention required
   - Continues on errors
   - Comprehensive logging

---

## Background Service

### **LicenseRenewalBackgroundService**

**File:** `Services/LicenseRenewalBackgroundService.cs`

**Type:** `BackgroundService` (Hosted Service)

**Purpose:** Orchestrates periodic license maintenance tasks

### **Configuration**

```csharp
public class LicenseRenewalBackgroundService : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);
    
    // Runs automatically when application starts
    // Stops when application shuts down
}
```

**Check Interval:** 1 hour
- Configurable via constructor
- Balance between responsiveness and resource usage
- Can be changed to minutes for testing

### **Execution Flow**

```
Application Starts
    ↓
Background Service Starts
    ↓
    ┌─────────────────────────────────────┐
    │ Every 1 Hour:                       │
    │                                     │
    │ 1. Create service scope             │
    │ 2. Get LicenseExpirationService     │
    │ 3. Get LicenseRenewalService        │
    │ 4. Invalidate expired licenses      │
    │ 5. Renew eligible licenses          │
    │ 6. Log results                      │
    │ 7. Wait for next interval           │
    └─────────────────────────────────────┘
    ↓
Application Stops → Background Service Stops
```

### **Service Lifecycle**

#### **Startup**
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("License Renewal Background Service started.");
    
    while (!stoppingToken.IsCancellationRequested)
    {
        // Perform checks
        await PerformRenewalCheckAsync(stoppingToken);
        
        // Wait for next interval
        await Task.Delay(_checkInterval, stoppingToken);
    }
    
    _logger.LogInformation("License Renewal Background Service stopped.");
}
```

#### **Shutdown**
- Gracefully stops when application shuts down
- CancellationToken signals shutdown
- Current operation completes before stopping
- Logged for audit trail

### **Scoped Services**

**Why Scoped Services?**
Background service is a singleton, but EF Core DbContext is scoped. We must create a scope for each operation.

```csharp
private async Task PerformRenewalCheckAsync(CancellationToken cancellationToken)
{
    // Create scope for scoped services
    using var scope = _serviceProvider.CreateScope();
    
    // Get scoped services
    var renewalService = scope.ServiceProvider.GetRequiredService<ILicenseRenewalService>();
    var expirationService = scope.ServiceProvider.GetRequiredService<ILicenseExpirationService>();
    
    // Use services
    await expirationService.InvalidateExpiredLicensesAsync();
    await renewalService.RenewEligibleLicensesAsync();
    
    // Scope disposed automatically (DbContext disposed)
}
```

### **Error Handling**

```csharp
while (!stoppingToken.IsCancellationRequested)
{
    try
    {
        await PerformRenewalCheckAsync(stoppingToken);
    }
    catch (Exception ex)
    {
        // Log error but continue running
        _logger.LogError(ex, "Error in license renewal background service");
    }
    
    // Always wait for next interval, even after error
    await Task.Delay(_checkInterval, stoppingToken);
}
```

**Error Recovery:**
- Errors logged but don't stop service
- Next check happens on schedule
- Failed licenses attempted again next cycle
- No silent failures

### **Registration**

**File:** `Program.cs`

```csharp
// Register as hosted service
builder.Services.AddHostedService<LicenseRenewalBackgroundService>();

// Register dependencies
builder.Services.AddScoped<ILicenseRenewalService, LicenseRenewalService>();
builder.Services.AddScoped<ILicenseExpirationService, LicenseExpirationService>();
```

**Lifecycle:**
- Starts automatically with application
- Runs in background thread
- Doesn't block application startup
- Stops gracefully on shutdown

---

## License Expiration Service

### **LicenseExpirationService**

**File:** `Services/LicenseExpirationService.cs`

**Interface:** `ILicenseExpirationService`

**Purpose:** Manages license expiration logic

### **Primary Method: InvalidateExpiredLicensesAsync**

```csharp
public async Task<int> InvalidateExpiredLicensesAsync()
```

**Purpose:** Find and invalidate all expired licenses

**Logic:**
```csharp
1. Get current UTC time
2. Query licenses where:
   - IsActive = true
   - ExpirationDate <= now
   - CancelledAt is null
3. Set IsActive = false for each
4. Update UpdatedAt timestamp
5. Save changes
6. Return count of invalidated licenses
```

**Returns:** Number of licenses invalidated

**Example:**
```csharp
var count = await _expirationService.InvalidateExpiredLicensesAsync();
// count = 3 (3 licenses were expired and invalidated)
```

### **Query Details**

```csharp
var expiredLicenses = await _dbContext.Licenses
    .Where(l => l.IsActive &&              // Only active licenses
               l.ExpirationDate <= now &&  // Past expiration
               l.CancelledAt == null)      // Not manually cancelled
    .ToListAsync();
```

**Why These Filters?**
- `IsActive` - Already invalidated licenses skip
- `ExpirationDate <= now` - Only truly expired
- `CancelledAt == null` - Cancelled licenses already inactive

### **Invalidation Process**

```csharp
foreach (var license in expiredLicenses)
{
    license.IsActive = false;       // Mark as inactive
    license.UpdatedAt = now;        // Record when invalidated
    
    _logger.LogInformation(
        "License {LicenseId} (Organization: {OrganizationId}, Name: {LicenseName}) " +
        "expired and has been invalidated. Expiration date: {ExpirationDate}",
        license.Id, license.OrganizationId, license.Name, license.ExpirationDate);
}

await _dbContext.SaveChangesAsync();
```

### **Utility Methods**

#### **IsLicenseExpired**
```csharp
public bool IsLicenseExpired(DateTime expirationDate)
{
    return expirationDate <= DateTime.UtcNow;
}
```

**Usage:**
```csharp
if (_expirationService.IsLicenseExpired(license.ExpirationDate))
{
    // License is expired
}
```

#### **GetDaysUntilExpiration**
```csharp
public double GetDaysUntilExpiration(DateTime expirationDate)
{
    var now = DateTime.UtcNow;
    var timeSpan = expirationDate - now;
    return timeSpan.TotalDays;
}
```

**Usage:**
```csharp
var daysLeft = _expirationService.GetDaysUntilExpiration(license.ExpirationDate);
// daysLeft = 2.5 (2.5 days until expiration)
// daysLeft = -1.0 (expired 1 day ago)
```

### **Logging**

```csharp
// When licenses found
_logger.LogInformation(
    "License {LicenseId} (Organization: {OrganizationId}, Name: {LicenseName}) " +
    "expired and has been invalidated. Expiration date: {ExpirationDate}",
    license.Id, license.OrganizationId, license.Name, license.ExpirationDate);

// Summary
_logger.LogInformation("Invalidated {Count} expired license(s).", expiredLicenses.Count);

// When none found
_logger.LogDebug("No expired licenses found to invalidate.");
```

---

## License Renewal Service

### **LicenseRenewalService**

**File:** `Services/LicenseRenewalService.cs`

**Interface:** `ILicenseRenewalService`

**Purpose:** Handles automatic license renewal

### **Primary Method: RenewEligibleLicensesAsync**

```csharp
public async Task<int> RenewEligibleLicensesAsync(int renewalWindowDays = 7)
```

**Parameters:**
- `renewalWindowDays` - Days before expiration to trigger renewal (default: 7)

**Purpose:** Find and renew licenses eligible for auto-renewal

**Returns:** Number of licenses renewed

### **Eligibility Criteria**

A license is eligible for renewal if:

1. ✅ `AutoRenewalEnabled = true`
2. ✅ `IsActive = true`
3. ✅ `CancelledAt = null` (not manually cancelled)
4. ✅ `ExpirationDate > now` (not yet expired)
5. ✅ `ExpirationDate <= now + renewalWindowDays` (within renewal window)

**Renewal Window:**
```
Now                    Expiration
 |------- 7 days --------|
         ^
    Renewal Window
    
Licenses expiring within this window are renewed
```

### **Query Details**

```csharp
var now = DateTime.UtcNow;
var renewalThreshold = now.AddDays(renewalWindowDays);  // 7 days from now

var eligibleLicenses = await _dbContext.Licenses
    .Where(l => l.AutoRenewalEnabled &&
               l.IsActive &&
               l.CancelledAt == null &&
               l.ExpirationDate > now &&
               l.ExpirationDate <= renewalThreshold)
    .ToListAsync();
```

**Example Timeline:**
```
Current Time: Jan 15, 10:00 AM
Renewal Window: 7 days
Renewal Threshold: Jan 22, 10:00 AM

License A: Expires Jan 16 → Within window → RENEW ✅
License B: Expires Jan 20 → Within window → RENEW ✅
License C: Expires Jan 25 → Beyond window → Skip ⏭️
License D: Expires Jan 10 → Already expired → Skip ⏭️
License E: Expires Jan 18, AutoRenewal=false → Skip ⏭️
```

### **Renewal Process**

```csharp
foreach (var license in eligibleLicenses)
{
    try
    {
        // Calculate original duration
        var originalDuration = license.ExpirationDate - license.StartDate;
        
        // Extend by same duration
        var newExpirationDate = license.ExpirationDate.Add(originalDuration);
        
        // Update license
        license.ExpirationDate = newExpirationDate;
        license.UpdatedAt = now;
        
        renewedCount++;
        
        _logger.LogInformation(
            "License {LicenseId} (Organization: {OrganizationId}, Name: {LicenseName}) " +
            "auto-renewed. Old expiration: {OldExpiration}, New expiration: {NewExpiration}",
            license.Id, license.OrganizationId, license.Name,
            license.ExpirationDate.Subtract(originalDuration), newExpirationDate);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,
            "Error renewing license {LicenseId} (Organization: {OrganizationId})",
            license.Id, license.OrganizationId);
        // Continue with other licenses
    }
}

if (renewedCount > 0)
{
    await _dbContext.SaveChangesAsync();
}
```

### **Renewal Duration Calculation**

**Example 1: 10-minute license**
```
StartDate:      Jan 15, 10:00 AM
ExpirationDate: Jan 15, 10:10 AM
Duration:       10 minutes

Renewed:
Old Expiration: Jan 15, 10:10 AM
New Expiration: Jan 15, 10:20 AM (extended by 10 minutes)
```

**Example 2: 30-day license**
```
StartDate:      Jan 1, 2026
ExpirationDate: Jan 31, 2026
Duration:       30 days

Renewed:
Old Expiration: Jan 31, 2026
New Expiration: Mar 2, 2026 (extended by 30 days)
```

**Example 3: 1-year license**
```
StartDate:      Jan 1, 2026
ExpirationDate: Jan 1, 2027
Duration:       365 days

Renewed:
Old Expiration: Jan 1, 2027
New Expiration: Jan 1, 2028 (extended by 365 days)
```

### **Error Handling**

Individual license renewal failures don't stop the process:

```csharp
foreach (var license in eligibleLicenses)
{
    try
    {
        // Renew this license
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error renewing license {LicenseId}", license.Id);
        // Continue with next license
    }
}
```

**Benefits:**
- One failed renewal doesn't affect others
- All eligible licenses get attempted
- Errors logged for investigation
- Failed licenses retried next cycle

### **Logging**

```csharp
// Per license
_logger.LogInformation(
    "License {LicenseId} (Organization: {OrganizationId}, Name: {LicenseName}) " +
    "auto-renewed. Old expiration: {OldExpiration}, New expiration: {NewExpiration}",
    license.Id, license.OrganizationId, license.Name, oldExpiration, newExpiration);

// Summary
_logger.LogInformation("Auto-renewed {Count} license(s).", renewedCount);

// When none found
_logger.LogDebug("No licenses found eligible for auto-renewal.");

// Errors
_logger.LogError(ex, "Error renewing license {LicenseId}", license.Id);
```

---

## Integration & Workflow

### **Complete Cycle Flow**

```
Hour 0: Background Service Starts
    ↓
Hour 1: First Check
    ↓
    ├─ Expiration Service
    │   └─ Find expired licenses
    │   └─ Set IsActive = false
    │   └─ Log: "Invalidated 2 expired license(s)"
    │
    └─ Renewal Service
        └─ Find eligible licenses (within 7 days)
        └─ Extend expiration dates
        └─ Log: "Auto-renewed 5 license(s)"
    ↓
Wait 1 hour
    ↓
Hour 2: Second Check
    └─ Repeat...
```

### **License Lifecycle**

```
License Created
│ StartDate: Jan 1, 10:00 AM
│ ExpirationDate: Jan 1, 10:10 AM (10 minutes)
│ AutoRenewalEnabled: true
│ IsActive: true
│
├─ [9 minutes later] Renewal Window Opens
│   Background Job: "License eligible for renewal"
│   Action: Extend to 10:20 AM
│
├─ [9 minutes later] Renewal Window Opens Again
│   Background Job: "License eligible for renewal"
│   Action: Extend to 10:30 AM
│
├─ [If AutoRenewal disabled]
│   Background Job: "License not eligible (AutoRenewal=false)"
│   Action: None
│
└─ [After expiration without renewal]
    Background Job: "License expired"
    Action: Set IsActive = false
```

### **Coordination Between Services**

**Why Expiration First, Then Renewal?**

```csharp
// 1. Invalidate expired licenses first
var invalidatedCount = await expirationService.InvalidateExpiredLicensesAsync();

// 2. Then renew eligible licenses
var renewedCount = await renewalService.RenewEligibleLicensesAsync();
```

**Reason:**
- Expired licenses with AutoRenewal disabled get invalidated
- Expired licenses with AutoRenewal enabled won't be caught (already expired)
- Renewal catches licenses BEFORE they expire
- Clean separation of concerns

**Better Approach:**
Licenses should be renewed before expiration (within renewal window), so expiration check is a safety net for licenses that weren't renewed.

---

## Configuration & Tuning

### **Check Interval**

**Current:** 1 hour
```csharp
private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);
```

**For Testing:**
```csharp
// Check every 1 minute for faster testing
private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
```

**For Production:**
```csharp
// Check every 6 hours for less frequent checks
private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);
```

### **Renewal Window**

**Current:** 7 days before expiration
```csharp
var renewedCount = await renewalService.RenewEligibleLicensesAsync(renewalWindowDays: 7);
```

**Adjust as needed:**
```csharp
// Renew 1 day before expiration
await renewalService.RenewEligibleLicensesAsync(renewalWindowDays: 1);

// Renew 30 days before expiration
await renewalService.RenewEligibleLicensesAsync(renewalWindowDays: 30);
```

**Considerations:**
- Shorter window: More precise, but less buffer for failures
- Longer window: More buffer, but earlier renewal
- Balance between responsiveness and resource usage

### **License Expiration Duration**

**Current:** 10 minutes (for testing)
```csharp
var expirationDate = startDate.AddMinutes(10);
```

**For Production:**
```csharp
// 30-day license
var expirationDate = startDate.AddDays(30);

// 1-year license
var expirationDate = startDate.AddYears(1);
```

---

## Manual Triggers

### **Admin Endpoint for Manual Check**

**Endpoint:** `POST /api/license/check-expiration`

**Authorization:** Admin only

```csharp
[HttpPost("check-expiration")]
public async Task<IActionResult> CheckAndInvalidateExpiredLicenses()
{
    var invalidatedCount = await _expirationService.InvalidateExpiredLicensesAsync();
    
    return Ok(new { 
        InvalidatedCount = invalidatedCount, 
        Message = $"Invalidated {invalidatedCount} expired license(s)." 
    });
}
```

**Use Cases:**
- Immediate expiration check needed
- Testing expiration logic
- After bulk license operations
- Troubleshooting license issues

**Example:**
```bash
curl -X POST "https://localhost:5001/api/license/check-expiration" \
  -H "Authorization: Bearer {admin-token}"

Response:
{
  "invalidatedCount": 3,
  "message": "Invalidated 3 expired license(s)."
}
```

---

## Logging & Monitoring

### **Log Levels**

#### **Information Level**
- Service start/stop
- Licenses renewed (per license + summary)
- Licenses invalidated (per license + summary)

#### **Debug Level**
- No licenses found to renew
- No licenses found to invalidate

#### **Error Level**
- Service errors
- Individual license renewal failures
- Database errors

### **Sample Log Output**

```
[10:00:00] Information: License Renewal Background Service started.

[11:00:00] Information: Invalidated 0 expired license(s).
[11:00:00] Information: License 123e4567-e89b-12d3-a456-426614174000 (Organization: abc-org, Name: Premium License) auto-renewed. Old expiration: 2026-01-16T10:00:00Z, New expiration: 2026-01-16T10:10:00Z
[11:00:00] Information: License 789e4567-e89b-12d3-a456-426614174000 (Organization: xyz-org, Name: Basic License) auto-renewed. Old expiration: 2026-01-17T10:00:00Z, New expiration: 2026-01-17T10:10:00Z
[11:00:00] Information: Auto-renewed 2 license(s).

[12:00:00] Information: License 456e4567-e89b-12d3-a456-426614174000 (Organization: test-org, Name: Expired License) expired and has been invalidated. Expiration date: 2026-01-15T11:50:00Z
[12:00:00] Information: Invalidated 1 expired license(s).
[12:00:00] Debug: No licenses found eligible for auto-renewal.

[13:00:00] Error: Error renewing license 999e4567-e89b-12d3-a456-426614174000 (Organization: error-org)
System.InvalidOperationException: Database connection failed
   at ...
```

### **Monitoring Metrics**

**Key Metrics to Track:**
- Number of licenses renewed per cycle
- Number of licenses expired per cycle
- Background service uptime
- Renewal success rate
- Time taken per cycle
- Error frequency

**Alert Conditions:**
- Background service stopped
- High error rate (>10%)
- No renewals for extended period (if expected)
- Excessive cycle duration (>1 minute)

---

## Testing Scenarios

### **Test 1: Basic Auto-Renewal**

**Setup:**
```csharp
// Create license with auto-renewal
var license = new License
{
    Name = "Test License",
    StartDate = DateTime.UtcNow,
    ExpirationDate = DateTime.UtcNow.AddMinutes(10),
    AutoRenewalEnabled = true,
    IsActive = true
};
```

**Expected:**
- License renewed after 3 minutes (within 7-day window)
- ExpirationDate extended by 10 minutes
- IsActive remains true

### **Test 2: Expiration Without Renewal**

**Setup:**
```csharp
var license = new License
{
    Name = "Test License",
    StartDate = DateTime.UtcNow.AddMinutes(-15),
    ExpirationDate = DateTime.UtcNow.AddMinutes(-5), // Already expired
    AutoRenewalEnabled = false,
    IsActive = true
};
```

**Expected:**
- License invalidated on next check
- IsActive set to false
- UpdatedAt timestamp updated

### **Test 3: Multiple Renewals**

**Setup:**
```csharp
var license = new License
{
    Name = "Test License",
    StartDate = DateTime.UtcNow,
    ExpirationDate = DateTime.UtcNow.AddMinutes(10),
    AutoRenewalEnabled = true,
    IsActive = true
};
```

**Expected Cycle:**
```
00:00 - Created (expires 00:10)
00:03 - Renewed (expires 00:20)
00:13 - Renewed (expires 00:30)
00:23 - Renewed (expires 00:40)
... continues indefinitely
```

### **Test 4: Disable Auto-Renewal Mid-Cycle**

**Setup:**
```csharp
// Start with auto-renewal
license.AutoRenewalEnabled = true;

// After first renewal, disable
license.AutoRenewalEnabled = false;
await _dbContext.SaveChangesAsync();
```

**Expected:**
- First renewal happens
- Subsequent renewals don't happen
- License expires after last renewal period
- IsActive set to false after expiration

---

## Performance Considerations

### **Database Queries**

**Optimized Queries:**
```csharp
// Expiration: Single query with filters
var expired = await _dbContext.Licenses
    .Where(l => l.IsActive && l.ExpirationDate <= now && l.CancelledAt == null)
    .ToListAsync();

// Renewal: Single query with filters
var eligible = await _dbContext.Licenses
    .Where(l => l.AutoRenewalEnabled && l.IsActive && ...)
    .ToListAsync();
```

**Index Requirements:**
- Index on `IsActive`
- Index on `ExpirationDate`
- Index on `AutoRenewalEnabled`
- Composite index on `(AutoRenewalEnabled, IsActive, ExpirationDate)`

### **Batch Operations**

```csharp
// Update all licenses in memory
foreach (var license in licenses)
{
    license.ExpirationDate = newDate;
}

// Single database round-trip
await _dbContext.SaveChangesAsync();
```

### **Scalability**

**Current Approach:** Good for up to ~10,000 licenses

**For Larger Scale:**
- Batch processing (process 1000 at a time)
- Distributed processing (multiple workers)
- Queue-based architecture
- Separate database for jobs

---

## File Locations

```
CaseGuard.Backend.Assignment/
└── Services/
    ├── ILicenseExpirationService.cs        # Interface
    ├── LicenseExpirationService.cs         # Implementation
    ├── ILicenseRenewalService.cs           # Interface
    ├── LicenseRenewalService.cs            # Implementation
    └── LicenseRenewalBackgroundService.cs  # Background worker
```

---

## Related Documentation

- [Task 1: Database Schema](Task1_Database_Schema_Design.md) - License entity
- [Task 3: Admin Endpoints](Task3_Admin_Endpoints.md) - Manual license management
- [Entity: License](../CaseGuard.Backend.Assignment/Entities/License.cs) - License properties

---

**Status**: ✅ Task 7 Complete - Fully automated license renewal system with background jobs, expiration checking, and comprehensive error handling implemented
