# CleanArchitecture Project

**First-time setup:**
Before running the project for the first time, you must create the initial database migration and apply it. Otherwise, the application will not work correctly.

Run the command:

```sh
dotnet ef migrations add InitialCreate --project backend/Infrastructure --startup-project backend/Web --output-dir Data/Migrations
```

---

# Database Migration Steps

1. To add a migration:

   ```sh
   dotnet ef migrations add InitialCreate --project backend/Infrastructure --startup-project backend/Web --output-dir Data/Migrations
   ```

2. To apply the migration to the database:
   ```sh
   dotnet ef database update --project backend/Infrastructure --startup-project backend/Web
   ```

> Note: Migrations are kept in the Infrastructure layer, and the application's entry point is the Web project.
