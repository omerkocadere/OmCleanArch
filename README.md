# CleanArchitecture Project

**First-time setup:**
Before running the project for the first time, you must create the initial database migration and apply it. Otherwise, the application will not work correctly.

Run the command:

```sh
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Web.Api --output-dir Data/Migrations
```

---

## Install Aspire CLI

To use Aspire orchestration and dashboard features, install the Aspire CLI tool globally:

```sh
 dotnet tool install -g aspire.cli --prerelease
 aspire publish -o infra
 cd infra
 docker compose up -d
```



docker cp infra-web-api-1:/app ./local-directory


For more info: https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-cli
