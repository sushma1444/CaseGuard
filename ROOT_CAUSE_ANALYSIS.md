# Root Cause Analysis - Why Tests Are Failing

## The Problem

The tests are failing because **`UnauthorizedException` was being caught and converted to `BadRequestException`**.

### What Was Happening:

1. **Get Claims (401)** - This is actually working correctly! The 401 means the token isn't being validated. This could be:
   - Token not being sent correctly
   - Token expired
   - Token validation failing

2. **Create Organization (400)** - When `CurrentUserIdGuid` is accessed, if the user claims are missing/invalid, it throws `UnauthorizedException`. This was being caught by the generic `catch (Exception ex)` and converted to `BadRequestException`.

3. **Get All Organizations (400)** - Same issue - `CurrentUserIdGuid` throws `UnauthorizedException`, which gets converted to `BadRequestException`.

4. **Get User Organizations (400)** - Same issue.

---

## The Fix Applied

I've added `catch (UnauthorizedException) { throw; }` to all three methods:
- `CreateOrganization`
- `GetOrganizations`  
- `GetUserOrganizations`

This ensures that `UnauthorizedException` is properly re-thrown instead of being converted to `BadRequestException`.

---

## Why This Happens

The `CurrentUserIdGuid` property in `BaseController` calls:
```csharp
ClaimsHelper.GetUserIdAsGuid(User)
```

This method throws `UnauthorizedException` if:
- User ID claim is not found in token
- User ID claim is in invalid format (not a valid Guid)

When this exception was thrown, it was caught by the generic exception handler and converted to a generic `BadRequestException`, hiding the real issue.

---

## What You Need to Do

1. **Stop the application** (Ctrl+C)
2. **Rebuild:**
   ```powershell
   dotnet build CaseGuard.Backend.Assignment/CaseGuard.Backend.Assignment.csproj
   ```
3. **Restart:**
   ```powershell
   dotnet run --project CaseGuard.Backend.Assignment
   ```
4. **Run tests:**
   ```powershell
   .\test-api.ps1
   ```

---

## Expected Results After Fix

- **Get Claims** - May still fail with 401 if token validation is the issue (this is a different problem)
- **Create Organization** - Should now return 401 (not 400) if user claims are invalid, or 201 if successful
- **Get All Organizations** - Should now return 401 (not 400) if user claims are invalid, or 200 if successful
- **Get User Organizations** - Should now return 401 (not 400) if user claims are invalid, or 200 if successful

If you still get 401 errors, it means the **token isn't being validated correctly**, which is a different issue (likely token format or JWT configuration).

---

## Next Steps if Still Failing

If tests still fail with 401:
1. Check if tokens are being generated correctly
2. Check if tokens are being sent in the Authorization header
3. Check JWT configuration in `appsettings.json`
4. Check application console for token validation errors
