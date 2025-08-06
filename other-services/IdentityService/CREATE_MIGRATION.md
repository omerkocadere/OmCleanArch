# Creating Initial Entity Framework Migration

To create the initial Entity Framework migration, run the following command in the `IdentityService` project directory:

dotnet ef migrations add "Initial create" -o Data/Migrations

```

Before running the migration command, ensure you have the latest Entity Framework Core CLI tools installed or updated globally:

```

dotnet tool update dotnet-ef -g

```

Then run the migration command:

```

dotnet ef migrations add "Initial create" -o Data/Migrations

```

- This will scaffold a new migration named `Initial create` and place it in the `Data/Migrations` folder.
- Make sure you have the `Microsoft.EntityFrameworkCore.Design` package installed and your `DbContext` is properly configured.
- Run this command from the root of the `IdentityService` project or specify the project path with `-p` if running from elsewhere.

---
**Reference:**
- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
```
