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
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Angular (separate client project)

## Database Migration Notes

Next steps for database updates:

- Run `Add-Migration SignalREntities` to create migration for Group and Connection tables
- Apply migration with `Update-Database` command
