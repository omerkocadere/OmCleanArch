# Clean Architecture - Project Memory

## Key Architectural Decisions Made

### 1. Permission Constants Placement (Aug 29, 2025)

**Decision**: Moved PermissionNames.cs from `Infrastructure/Authentication` to `Application/Common/Security`

**Research Sources**:

- Uncle Bob's Clean Architecture: Dependency Rule mandates inner circles cannot depend on outer circles
- Microsoft .NET Documentation: Application Core contains business rules and cross-cutting concerns
- Jason Taylor Template: Uses Application/Common/Security for authorization concerns
- StackOverflow DDD Discussion: Constants should respect domain isolation

**Rationale**:

- Authorization is a cross-cutting concern for all use cases
- Permission constants are business logic, not infrastructure concerns
- Application layer can be accessed by both Infrastructure and UI layers
- Maintains Clean Architecture dependency direction
- Improves testability and maintainability

**Files Affected**:

- Created: `src/Application/Common/Security/PermissionNames.cs`
- Updated: `src/Web.Api/Endpoints/MembersVersioned.cs`
- Updated: `src/Infrastructure/Data/SeedData/RolePermissionSeeder.cs`
- Removed: `src/Infrastructure/Authentication/PermissionNames.cs`

**Build Status**: ✅ All projects build successfully
**Dependency Verification**: ✅ Clean Architecture compliance maintained

### 2. KISS Principle Application

**Previous State**: Complex factory methods and nested responsibilities in Permission entities
**Current State**: Simple, focused entities with clear single responsibilities
**Rejected**: Over-engineered patterns for hypothetical future needs

## Domain Model Analysis

### Role Entity Status

- **Location**: `src/Domain/Roles/Role.cs`
- **Status**: ✅ **OPTIMIZED following KISS and Clean Architecture best practices**
- **Final Improvements Made**:
  - **Encapsulation**: Private backing fields for collections
  - **Read-only Access**: Public properties return IReadOnlyCollection
  - **EF Core Support**: ✅ **REMOVED internal properties - unnecessary complexity**
  - **Business Logic**: Simple, focused methods with proper validation
  - **KISS Application**: Removed unnecessary factory method complexity AND internal properties
  - **Null Safety**: ArgumentNullException.ThrowIfNull usage
  - **Validation**: Added IsValidRole() for business rule checking
  - **Clean Code**: Public collections work perfectly with EF Core Fluent API

### Research-Based Decision: Internal Properties Removed

**Research Sources**: Microsoft Clean Architecture docs, Medium best practices, EF Core documentation
**Finding**: Internal properties are NOT required for EF Core navigation - public IReadOnlyCollection works
**Result**: Simpler, cleaner domain entity without ORM pollution

### Key Architectural Improvements

1. **Collection Encapsulation**: `_permissions` and `_users` private fields
2. **Invariant Protection**: Controlled access through business methods
3. **ORM Compatibility**: Internal properties for Entity Framework
4. **Business Rules**: Proper null checking and validation
5. **Single Responsibility**: Each method has clear, focused purpose

### Build Status After Changes

- ✅ Domain.csproj build successful
- ✅ Infrastructure.csproj build successful (EF Core mappings compatible)
- ✅ Application layer unaffected

## Patterns and Conventions Established

### Naming Conventions

- Permission constants: `{resource}:{action}` format (e.g., "members:read")
- Categories: PascalCase nested classes
- Helper methods: Simple, direct naming

### Architectural Patterns

- **Dependency Direction**: Always inward-pointing
- **Cross-Cutting Concerns**: Application/Common/\* placement
- **Entity Design**: KISS principle over complex abstractions
- **Collection Encapsulation**: Private backing fields with read-only public access
- **ORM Mapping**: Internal properties for Entity Framework navigation

## Best Practices Established

### Domain Entity Guidelines

1. **Encapsulation First**: Use private fields with controlled access
2. **Read-Only Collections**: Expose IReadOnlyCollection for immutability
3. **Business Logic**: Keep domain methods simple and focused
4. **Validation**: Use ArgumentNullException.ThrowIfNull for null safety
5. **Factory Patterns**: Avoid unless absolutely necessary (KISS)

### Clean Architecture Compliance Checklist

- ✅ No Infrastructure dependencies in Domain
- ✅ Permission constants in Application/Common/Security
- ✅ Proper encapsulation in entities
- ✅ Business logic within domain boundaries
- ✅ Simple, testable methods
- ✅ EF Core compatibility maintained

## Final Status (Sep 2, 2025)

### JWT Token Expiration Issue Fixed ✅

**Problem**: JWT token süresi dolduğunda CORS hatası alınıyordu, 401 dönmüyordu.

**Root Cause**: Program.cs'de `UseAuthentication()` ve `UseAuthorization()` middleware'leri eksikti.

**Solution Applied**:

1. **Added Authentication Middleware**: Program.cs'e `UseAuthentication()` ve `UseAuthorization()` eklendi
2. **Enhanced Error Interceptor**: 401 durumunda otomatik logout ve redirect implementasyonu
3. **Verified JWT Configuration**: ExpirationInMinutes = 1 (test amaçlı)

**Files Modified**:

- `src/Web.Api/Program.cs`: Authentication middleware'leri eklendi
- `client/src/core/interceptors/error-interceptor.ts`: 401 handling iyileştirildi

**Result**: ✅ Expired JWT token'lar artık proper 401 response döner, CORS error çıkmaz.

---

**Permission System**: ✅ Fully migrated to Application layer
**Role Entity**: ✅ Refactored to Clean Architecture standards  
**JWT Authentication**: ✅ Fixed expired token handling
**User Activity Tracking**: ✅ Implemented with CQRS and middleware pattern
**Build Status**: ✅ All projects compile successfully
**Architecture**: ✅ Uncle Bob's dependency rule respected
**Code Quality**: ✅ KISS principle applied throughout

### User Activity Tracking Implementation - BEST PRACTICE VERSION (Sep 4, 2025)

**Problem**: Need to track user activity (LastActive) for authenticated users on every API request.

**Research Conducted**:

- Microsoft official documentation on middleware order
- Stack Overflow discussions on middleware vs filters
- ASP.NET Core endpoint filter documentation

**Key Findings**:

1. **Microsoft's Official Middleware Order**: Authentication → Authorization → Endpoints
2. **Middleware vs Filters Best Practice**:
   - Middleware: For cross-cutting concerns affecting ALL requests
   - Endpoint Filters: For logic specific to certain endpoints only
   - Filters have access to MVC context, middleware works at HttpContext level

**Final Solution - Endpoint Filter Approach (BEST PRACTICE)**:

1. **CQRS Command**: `UpdateUserActivityCommand` in Application layer

   - Uses `IUserContext` to get current user ID
   - Updates `Member.LastActive` using efficient `ExecuteUpdateAsync`
   - Graceful handling when user is not authenticated

2. **Endpoint Filter Implementation**: `UserActivityEndpointFilter`

   - Implements `IEndpointFilter` interface
   - Only runs on specific endpoints where registered
   - Has access to DI container for `IMediator` and `ILogger`
   - Uses fire-and-forget pattern to avoid blocking responses
   - Better performance than middleware (doesn't run on all requests)

3. **Clean Architecture Compliance**:
   - Business logic in Application layer
   - Web concerns in Web.Api layer
   - Proper dependency direction maintained

**Files Created/Modified**:

- **Kept**: `src/Application/Members/Commands/UpdateUserActivity/UpdateUserActivityCommand.cs`
- **Created**: `src/Web.Api/Filters/UserActivityEndpointFilter.cs` (BEST PRACTICE)
- **Modified**: `src/Web.Api/Extensions/MiddlewareExtensions.cs` (endpoint filter extensions)
- **Modified**: `src/Web.Api/Endpoints/Members.cs` (added filter to member endpoints)
- **Modified**: `src/Web.Api/Program.cs` (removed middleware, fixed auth order)
- **Removed**: `src/Web.Api/Middleware/UserActivityMiddleware.cs` (replaced with filter)

**Technical Improvements Over Middleware**:

- ✅ **Performance**: Only runs on specific endpoints, not all requests
- ✅ **Precision**: Only applies to authenticated endpoints
- ✅ **Context Access**: Has full access to endpoint context and DI
- ✅ **Best Practice**: Follows Microsoft recommended patterns
- ✅ **Maintainability**: Easier to test and modify per endpoint

**Authentication Middleware Order Fixed**:

- **Before**: `Authentication → UserActivity → Authorization` (WRONG)
- **After**: `Authentication → Authorization` (CORRECT per Microsoft docs)

**Result**: ✅ Best practice implementation that only tracks activity on authenticated API endpoints with optimal performance and clean architecture compliance.
