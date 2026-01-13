# Backend Take-Home Assignment

## Overview

Welcome! This is a take-home assignment to build an **Organization and License Management System**. You'll be implementing a backend API that manages organizations, user memberships, licenses, and role-based authorization.

**Expected time**: 3-4 hours

**What you're building**: A system where:
- Admins can create and manage licenses for organizations
- Organization owners can invite users, manage members, and assign licenses
- Users can join organizations, accept invitations, and view their memberships
- Licenses have expiration and auto-renewal capabilities

## Business Context

### Domain Overview

You're building a multi-tenant SaaS platform where:

**Organizations**: Companies or teams that use the platform. Each organization can have multiple users.

**Users**: People who belong to one or more organizations. Users have roles within organizations (e.g., Owner, Member).

**Licenses**: Subscriptions that organizations purchase. Licenses control access and features. A license is tied to an organization and affects the users within it.

**Roles**: Users have different permission levels within organizations. Your task is to design how roles work and enforce proper authorization.

### License Management Rules

**License Expiration**:
- By default, licenses expire after **10 minutes** (for testing purposes)
- Once expired, the license should no longer be valid

**Auto-Renewal**:
- Licenses can have an **auto-renewal** feature
- When auto-renewal is enabled, a background job should automatically renew the license for another 10 minutes before it expires
- This should continue as long as auto-renewal remains enabled

**Your Task**: Design and implement the license data model, expiration logic, and auto-renewal mechanism using background jobs.

## User Stories

Your goal is to implement these user stories. These are the key flows that should work end-to-end. Edge cases, validation rules, and implementation details are up to you.

### Admin User Stories

As an **Admin**, I should be able to:

1. Create a license for an organization
2. View all licenses in the system
3. Update license properties (e.g., extend expiration, enable/disable auto-renewal)
4. Cancel or revoke a license

### Organization Owner/Admin Stories

As an **Organization Owner or Admin**, I should be able to:

1. Create a new organization
2. Update organization details
3. Delete my organization
4. Invite users to my organization (via email)
5. Remove users from my organization
6. Assign a license to a specific user in my organization
7. Unassign a license from a user
8. View all members in my organization (with their roles and license status)
9. View details of a specific member
10. Update a member's role within the organization
11. View all pending invitations for my organization
12. View details of a specific invitation
13. Cancel a pending invitation

### Regular User Stories

As a **Regular User**, I should be able to:

1. View all organizations I'm a member of
2. View details of a specific organization I belong to
3. Accept an invitation to join an organization
4. Leave an organization I'm part of

### System Stories

As the **System**, I should:

1. Automatically renew licenses when auto-renewal is enabled
2. Enforce proper authorization on all endpoints (users should only access what they're permitted to)

## What You Need to Do

### Implementation Tasks

1. **Implement All Endpoints**: Currently, all controller methods throw `NotImplementedException()`. Implement the business logic for each endpoint.

2. **Design Request and Response DTOs**: Create contracts/DTOs for all endpoints. The `AuthController` has examples you can reference (`LoginRequest`, `LoginResponse`, `ClaimsResponse`).

3. **Design Your Database Schema**:
   - Use Entity Framework Core to design your data model
   - Add `DbSet` properties to `ApplicationDbContext`
   - Configure relationships in `OnModelCreating`
   - Create and apply migrations

4. **Implement Authorization**:
   - Enforce role-based access control on endpoints
   - Ensure users can only access organizations they belong to
   - Ensure only admins can access admin endpoints
   - Use the existing JWT authentication setup

5. **Implement System Jobs**:
   - Set up a mechanism for license auto-renewal

6. **Error Handling**:
   - Use the existing custom exception classes (`BadRequestException`, `NotFoundException`, `UnauthorizedException`, `ForbiddenException`)
   - The global exception handler will automatically convert them to ProblemDetails responses

7. **Refactor as Needed**:
   - You have full freedom to refactor and restructure the project
   - Change the architecture if you prefer a different approach
   - This is **your** solution - make it production-ready from your perspective

### What "Completion" Means

Completion means the software works end-to-end from your perspective as if you were delivering it to a client. The user stories provide guidance on what features should exist, but you decide:
- How the data model is structured
- What validation rules apply
- How authorization is enforced
- How edge cases are handled
- What additional features or safeguards are needed

If you believe the software is complete and ready for use, that's what matters.

## Evaluation Criteria

Your solution will be evaluated on three levels:

### Primary Criteria (Most Important)

**Completion**: Are the user stories implemented and working end-to-end?

### Secondary Criteria

**Code Quality**: Is the code clean, readable, and well-organized?

### Extra Points

1. **Pagination, Filtering, Sorting**: Do GET list endpoints support pagination, filtering, and sorting?
2. **Extensibility**: How easy would it be to extend this codebase as business requirements evolve?

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL (or modify the connection string for your preferred database)
- An IDE (Visual Studio, Rider, or VS Code)

### Setup Instructions

1. **Clone the repository**

2. **Update the connection string** (if needed):
   - Open `appsettings.json`
   - Modify the `ConnectionStrings:DefaultConnection` to point to your PostgreSQL instance

3. **Create the database**:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run the project**:
   ```bash
   dotnet run --project CaseGuard.Backend.Assignment
   ```

5. **Test the API**:
   - The project uses Swagger for API documentation
   - Navigate to `https://localhost:<port>/swagger` to explore and test endpoints
   - Start with the `/api/auth/login` endpoint to get a JWT token

### Testing Authentication

The `AuthController` is fully implemented as a reference:

1. **Login** (`POST /api/auth/login`):
   ```json
   {
     "userId": "user123",
     "email": "user@example.com",
     "role": "Admin"
   }
   ```
   - Returns a JWT token

2. **Get Claims** (`GET /api/auth/claims`):
   - Requires Authorization header: `Bearer <token>`
   - Returns the claims from your JWT token

Use the JWT token from login for authenticated requests by adding the header:
```
Authorization: Bearer <your-token>
```

## Technical Notes

### Existing Infrastructure

The project already includes:

- **JWT Authentication**: Configured and working (see `JwtExtensions.cs` and `AuthController.cs`)
- **PostgreSQL + EF Core**: Database setup is ready (see `DatabaseExtensions.cs` and `ApplicationDbContext.cs`)
- **Global Exception Handling**: ProblemDetails format with custom exceptions (see `GlobalExceptionHandlingMiddleware.cs`)
- **Swagger Documentation**: Auto-generated API docs

### Reference Implementation

The `AuthController` is fully implemented and can serve as a reference for:
- How to structure request/response DTOs
- How to use custom exceptions
- How to work with JWT claims
- How to return `IResult` responses

### Project Structure

```
CaseGuard.Backend.Assignment/
├── Controllers/           # API endpoints (implement these)
├── Data/                  # EF Core DbContext
├── Exceptions/            # Custom exception classes
├── Extensions/            # Service configuration extensions
├── Middleware/            # Global exception handler
└── appsettings.json       # Configuration

CaseGuard.Backend.Assignment.Contracts/
└── Auth/                  # Example DTOs for Auth endpoints
    ├── Requests/
    └── Responses/
```

## Final Notes

- **Take Ownership**: This is your solution. Implement it the way you think is best.
- **Completion Matters**: Focus on getting the user stories working end-to-end.
- **Quality Matters**: Write code you'd be proud to ship to production.
- **Ask Questions**: If anything is unclear, feel free to make reasonable assumptions and document them.

Good luck!
