# OmCleanArch Project Memory

This file maintains a record of architectural decisions, technical choices, and project evolution.

### 2. PowerShell Development Script Interactive Startup (Sep 8, 2025)

**Decision**: Implement two-stage interactive startup in `OmCleanArch.ps1` script.

**Strategy**: Start core development services immediately (Angular Client + Web.Api), then prompt user for additional services.

**Implementation Changes**:

1. **Default Startup**: Angular Client (port 4201) + Web.Api automatically start
2. **Interactive Prompt**: "Diğer servisleri de başlatmak ister misiniz? (Y/N)"
3. **Conditional Services**: DummyApi, SearchService, IdentityService, GatewayService start only if user chooses "Y"
4. **Smart Messaging**: Different completion messages based on what was started

**Benefits**:

- ✅ **Faster Development Start**: Core environment ready immediately
- ✅ **Resource Efficient**: Only start what you need
- ✅ **Flexible**: Easy access to full stack when needed
- ✅ **User Friendly**: Clear Turkish prompt and feedback

**Technical Details**:

- **Core Services**: Angular (ng serve --port 4201) + Web.Api (dotnet watch)
- **Timing**: 500ms delay after core services before prompt
- **Input Handling**: Accepts both "Y" and "y" responses
- **Terminal Management**: Uses Windows Terminal (wt) with named tabs

**Result**: ✅ Streamlined development workflow with optional full-stack startup

---

## Domain Design Patterns

### 4. Auditable Entity Design Pattern (Sep 16, 2025)

**Decision**: Maintain both `IAuditableEntity` interface and `BaseAuditableEntity<T>` abstract class for optimal audit functionality.

**Architecture Analysis**:

**Interface (`IAuditableEntity`)**:
- **Purpose**: Contract for audit properties (Created, CreatedBy, LastModified, LastModifiedBy)
- **Used By**: `AuditableEntityInterceptor` for polymorphic handling
- **Benefit**: Enables `context.ChangeTracker.Entries<IAuditableEntity>()` to capture all auditable entities regardless of generic type

**Abstract Class (`BaseAuditableEntity<T>`)**:
- **Purpose**: Base implementation for domain entities with audit capabilities
- **Implements**: `IAuditableEntity` interface + inherits from `BaseEntity<T>`
- **Benefit**: Code reuse and proper EF Core entity mapping

**Inheritance Hierarchy**:
```csharp
BaseEntity<T>                    // Base functionality + Domain Events
    ↓
BaseAuditableEntity<T>          // Implements IAuditableEntity + audit properties
    ↓  
FullAuditableEntity<T>          // Adds soft delete functionality (ISoftDeletable)
```

**Entity Usage Examples**:
- `Member : BaseAuditableEntity<Guid>` (standard audit)
- `Auction : BaseAuditableEntity<Guid>` (standard audit)
- `TodoItem : FullAuditableEntity<int>` (audit + soft delete)

**Interceptor Integration**:
```csharp
// AuditableEntityInterceptor leverages interface for polymorphic behavior
context.ChangeTracker.Entries<IAuditableEntity>()
    .Where(e => e.State is EntityState.Added or EntityState.Modified)
```

**Why Both Are Required**:
- ✅ **Interface**: Enables polymorphic interceptor handling across different generic types
- ✅ **Abstract Class**: Provides concrete implementation and EF Core compatibility
- ✅ **Clean Separation**: Interface for infrastructure concerns, abstract class for domain modeling
- ✅ **Extensibility**: New auditable entities easily inherit from base classes

**Alternative Approaches Rejected**:
- ❌ **Interface Only**: No implementation reuse, repetitive code
- ❌ **Abstract Class Only**: Cannot handle multiple generic types in interceptor
- ❌ **Non-Generic Base**: Loses type safety and EF Core generic benefits

**Result**: ✅ Current dual approach (interface + abstract class) confirmed as optimal Clean Architecture pattern for audit functionality.

---

## Architectural Decisions

### 1. SignalR Implementation in Clean Architecture (Sep 8, 2025)

**Decision**: Implement SignalR Hubs in `Web.Api` project following Clean Architecture principles.

**Research Sources**:

- Jason Taylor Clean Architecture Template: SignalR placement in Web layer
- Ardalis Clean Architecture: Presentation layer for real-time communication
- Microsoft SignalR Documentation: Hub positioning as API endpoints

**Implementation Structure**:

```
src/Web.Api/
├── Hubs/
│   ├── PresenceHub.cs (online user tracking)
│   └── MessageHub.cs (real-time messaging)
└── Services/
    └── PresenceTracker.cs (connection management)

src/Domain/Messages/
├── Group.cs (chat group entity)
└── Connection.cs (SignalR connection entity)
```

**Architectural Benefits**:

- ✅ **Clean Architecture Compliance**: Hub'lar Web.Api katmanında (client interaction layer)
- ✅ **Dependency Direction**: Application → Domain bağımlılık kuralı korundu
- ✅ **CQRS Integration**: Hub'lar MediatR üzerinden Application layer'ı kullanıyor
- ✅ **Service Pattern**: PresenceTracker singleton service olarak implementasyonu
- ✅ **Entity Framework Integration**: Group ve Connection entity'leri Domain katmanında

**Technical Implementation**:

1. **PresenceHub**: Online user tracking ve connection management
2. **MessageHub**: Real-time messaging with persistence
3. **PresenceTracker**: Connection state management as singleton service
4. **Domain Entities**: Group ve Connection entity'leri message grouping için

**Integration Points**:

- **Authentication**: IUserContext kullanarak current user bilgisi
- **Persistence**: IApplicationDbContext ile message ve connection persistence
- **Commands**: MediatR CreateMessageCommand kullanımı
- **Hub Mapping**: `/hubs/presence` ve `/hubs/message` endpoints

**Service Registration**:

```csharp
// DependencyInjection.cs
services.AddSignalR();
services.AddSingleton<IPresenceTracker, PresenceTracker>();

// Program.cs
app.MapHub<PresenceHub>("/hubs/presence");
app.MapHub<MessageHub>("/hubs/message");
```

**Files Created/Modified**:

- **Created**: `src/Web.Api/Hubs/PresenceHub.cs`
- **Created**: `src/Web.Api/Hubs/MessageHub.cs`
- **Created**: `src/Web.Api/Services/PresenceTracker.cs`
- **Created**: `src/Domain/Messages/Group.cs`
- **Created**: `src/Domain/Messages/Connection.cs`
- **Modified**: `src/Application/Common/Interfaces/IApplicationDbContext.cs` (Groups, Connections DbSets)
- **Modified**: `src/Infrastructure/Data/ApplicationDbContext.cs` (Entity registration)
- **Modified**: `src/Web.Api/DependencyInjection.cs` (SignalR services)
- **Modified**: `src/Web.Api/Program.cs` (Hub mapping)

**Result**: ✅ Complete SignalR implementation following Clean Architecture best practices with real-time presence tracking and messaging capabilities.

---

## Technical Stack

- **Framework**: .NET 8 with ASP.NET Core
- **Architecture**: Clean Architecture (Jason Taylor Template Pattern)
- **Database**: Entity Framework Core
- **Messaging**: MediatR for CQRS
- **Real-time**: SignalR for WebSocket communication
- **Authentication**: AddIdentityCore + JWT (API-optimized)
- **Frontend**: Angular (separate client project)

---

## Authentication Architecture Decision

### 3. AddIdentityCore vs AddIdentity Choice (Sep 16, 2025)

**Decision**: Use `AddIdentityCore<ApplicationUser>()` instead of `AddIdentity<TUser, TRole>()` for JWT-based API authentication.

**Analysis Context**: 
- Current implementation uses JWT authentication with custom IIdentityService
- Clean Architecture with API-first approach
- Manual cookie management for refresh tokens
- Custom authentication flow requirements

**Technical Comparison**:

**AddIdentityCore Benefits for Our Scenario**:
- ✅ **Minimal Setup**: Only essential user management services
- ✅ **JWT Compatible**: No cookie auth interference
- ✅ **Custom Flow Friendly**: Doesn't impose SignInManager patterns
- ✅ **API Optimized**: Perfect for Web API scenarios
- ✅ **Performance**: Lighter service registration
- ✅ **Clean Separation**: Authentication logic stays in custom services

**AddIdentity Would Include (Unnecessary for Us)**:
- ❌ **SignInManager**: Not needed for JWT flow
- ❌ **Default Cookie Auth**: Conflicts with JWT strategy
- ❌ **Built-in Login Flow**: We have custom implementation
- ❌ **Razor Pages Integration**: API-only application

**Current Implementation (Optimal)**:
```csharp
services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

**Alternative Scenarios for AddIdentity**:
- Traditional MVC/Razor Pages apps
- Cookie-based authentication
- Built-in Identity UI usage
- SignInManager dependency requirements

**Result**: ✅ AddIdentityCore confirmed as optimal choice for JWT-based Clean Architecture API implementation.

---

## Database Migration Notes

Next steps for database updates:

- Run `Add-Migration SignalREntities` to create migration for Group and Connection tables
- Apply migration with `Update-Database` command
