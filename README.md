# CleanArchitecture Project

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=omerkocadere_OmCleanArch&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=omerkocadere_OmCleanArch)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=omerkocadere_OmCleanArch&metric=bugs)](https://sonarcloud.io/summary/new_code?id=omerkocadere_OmCleanArch)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=omerkocadere_OmCleanArch&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=omerkocadere_OmCleanArch)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=omerkocadere_OmCleanArch&metric=coverage)](https://sonarcloud.io/summary/new_code?id=omerkocadere_OmCleanArch)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=omerkocadere_OmCleanArch&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=omerkocadere_OmCleanArch)

**First-time setup:**
Before running the project for the first time, you must create the initial database migration and apply it. Otherwise, the application will not work correctly.

Run the command:

```sh
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Web.Api --output-dir Data/Migrations
```

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

```sh
docker build -f src/Web.Api/Dockerfile -t testing123 .
```

```sh
dotnet dev-certs https --trust
```

dotnet publish -c Release -o ./bin/Publish
