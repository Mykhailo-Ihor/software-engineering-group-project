# TaskForge.EntityFrameworkCore

This project contains configurations and migrations specific to EnitityFrameworkCore.

### To add a new migration (run from the root folder):

```
dotnet ef migrations add "MigrationName" --output-dir Migrations
```

### To list the migrations that are available (run from the root folder):

```
dotnet ef migrations list
```

### To update the database (run from the root folder):

```
dotnet ef database update
```
