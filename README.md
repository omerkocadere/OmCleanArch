# CleanArchitecture Project

**First-time setup:**
Before running the project for the first time, you must create the initial database migration and apply it. Otherwise, the application will not work correctly.

Run the command:

```sh
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Web.Api --output-dir Data/Migrations
```

## Caching Configuration

The application supports two caching providers: **Memory Cache** (default) and **Redis** for distributed caching scenarios.

### Memory Cache (Default)
- In-memory caching for single-instance deployments
- Zero external dependencies
- Automatic cleanup and memory management

### Redis Cache
- Distributed caching for multi-instance deployments
- Shared cache across multiple application instances
- Persistent cache across application restarts
- Advanced features like cache versioning and invalidation

#### Setting up Redis

1. **Using Docker Compose (Recommended)**:
   ```sh
   docker-compose up redis redis-commander -d
   ```
   - Redis will be available at `localhost:6379`
   - Redis Commander (Web UI) at `http://localhost:8083`

2. **Manual Redis Installation**:
   ```sh
   # Ubuntu/Debian
   sudo apt update
   sudo apt install redis-server
   sudo systemctl start redis-server
   
   # macOS
   brew install redis
   brew services start redis
   
   # Windows
   # Use Redis for Windows or Docker
   ```

3. **Configuration**:
   Update your `appsettings.json` or environment variables:
   ```json
   {
     "Cache": {
       "Provider": "Redis",
       "DefaultTimeoutMinutes": 30,
       "RedisConnectionString": "localhost:6379"
     }
   }
   ```

   Or use environment variables:
   ```bash
   CACHE_PROVIDER=Redis
   CACHE_REDIS_CONNECTION_STRING=localhost:6379
   ```

#### Production Redis Configuration

For production environments, consider:

```json
{
  "Cache": {
    "Provider": "Redis",
    "DefaultTimeoutMinutes": 60,
    "RedisConnectionString": "your-redis-cluster-endpoint:6379,password=your-password,ssl=true"
  }
}
```

#### Cache Provider Features

| Feature | Memory Cache | Redis Cache |
|---------|-------------|-------------|
| Cross-instance sharing | ❌ | ✅ |
| Persistence | ❌ | ✅ |
| Distributed invalidation | ❌ | ✅ |
| Memory efficiency | ⚠️ Limited | ✅ |
| Network dependency | ✅ None | ⚠️ Required |
| Setup complexity | ✅ Simple | ⚠️ Moderate |

#### Cache Versioning System

Both providers support advanced cache versioning for efficient bulk invalidation:

```csharp
// Invalidate all user-related cache entries
await cacheService.InvalidateVersionAsync("users");

// Build versioned cache keys
var key = await cacheService.BuildVersionedKeyAsync("users", "profile:123");
// Result: "users:profile:123:v2"
```

## Environment Configuration

Copy `.env.example` to `.env` and configure according to your environment:

```bash
cp .env.example .env
```

Key configuration options:
- `CACHE_PROVIDER`: Memory or Redis
- `DATABASE_PROVIDER`: Sqlite or Postgres  
- `AUTH_PROVIDER`: Jwt or IdentityServer
- `BACKGROUND_JOBS_ENABLED`: true or false



## Install Aspire CLI

To use Aspire orchestration and dashboard features, install the Aspire CLI tool globally:

```sh
 dotnet tool install -g aspire.cli --prerelease
 aspire publish -o infra
 cd infra
 docker compose up -d
```

## Run Azure SQL Edge container

docker run -e "ACCEPT_EULA=1" -e "MSSQL_SA_PASSWORD=Aa123456!" -p 1433:1433 --name azure-sql -d mcr.microsoft.com/azure-sql-edge
