# CleanArchitecture Project

**First-time setup:**
Before running the project for the first time, you must create the initial database migration and apply it. Otherwise, the application will not work correctly.

Run the command:

```sh
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Web.Api --output-dir Data/Migrations
```

---

## Testing

This project includes comprehensive unit tests for the Application layer using:

- **XUnit** for test framework
- **Moq** for mocking dependencies
- **MockQueryable.Moq** for mocking Entity Framework async operations
- **FluentAssertions** for readable test assertions

### Running Tests

```sh
# Run all tests
dotnet test

# Run only Application layer tests
dotnet test tests/Application.Tests

# Run tests with verbose output
dotnet test tests/Application.Tests --verbosity normal
```

### Test Structure

Tests are organized by feature and follow the AAA (Arrange-Act-Assert) pattern. See `tests/Application.Tests/README.md` for detailed documentation about the testing approach.

---

## Install Aspire CLI

To use Aspire orchestration and dashboard features, install the Aspire CLI tool globally:

```sh
 dotnet tool install -g aspire.cli --prerelease
 aspire publish -o infra
 cd infra
 docker compose up -d
```

## Run Azure SQL Edge container

docker run -e "ACCEPT_EULA=1" -e "MSSQL_SA_PASSWORD=Aa123456!" \
 -p 1433:1433 --name azure-sql \
 -d mcr.microsoft.com/azure-sql-edge

docker cp infra-web-api-1:/app ./local-directory

For more info: https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-cli
