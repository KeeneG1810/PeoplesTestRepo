# PeopleAccountsManager

Simple ASP.NET Core 9 MVC application to manage Persons, Accounts and Transactions.
This repository uses Entity Framework Core and Bootstrap 5. Update the SQL connection string in `appsettings.json` and run the project.

---

## Quick summary

- Framework: .NET 9 (ASP.NET Core MVC)
- Database access: Entity Framework Core (DbContext + migrations included)
- UI: Bootstrap 5, Razor views
- Authentication: Simple cookie-based login
- Reminder: Replace your connection string in `appsettings.json` and run the project.

---

## Prerequisites

- .NET 9 SDK installed: https://dotnet.microsoft.com
- Visual Studio 2022 or later (with ASP.NET and web development workload)
- SQL Server (or SQL Server Express / Azure SQL) accessible from your machine
- (Optional) dotnet-ef CLI tool for migrations:
  - Install globally: `dotnet tool install --global dotnet-ef`
  - Or use the Package Manager Console in Visual Studio.

---

## What to do after cloning (step-by-step)

1. Open the solution
   - In Visual Studio: __File > Open > Project/Solution__ and open `PeopleAccountsManager.csproj`
   - Or using CLI: `git clone <repo-url>` then `cd PeopleAccountsManager`

2. Update the connection string
   - Open `appsettings.json`
   - Set `ConnectionStrings:DefaultConnection` to point to your SQL Server instance. Example:
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;Trusted_Connection=True;TrustServerCertificate=True;"
       }
     }
     ```
   - Save the file.

3. Restore packages & build
   - Visual Studio: use __Build > Restore NuGet Packages__ then __Build > Build Solution__
   - CLI:
     - `dotnet restore`
     - `dotnet build`

4. Database: use existing database or run migrations
   - If your SQL database already has the three tables (`Persons`, `Accounts`, `Transactions`) and optional `Users`, you only need to point the connection string and run the app.
   - If you want to create the database from the migrations included in the project:
     - Ensure `dotnet-ef` is installed (see Prerequisites).
     - From project folder (where `.csproj` is located):
       - `dotnet ef database update`
     - This will apply migrations in `Migrations/` and create the database/tables.

5. Run the application
   - Visual Studio: press __Debug > Start Debugging__ (F5) or __Debug > Start Without Debugging__ (Ctrl+F5).
   - CLI: `dotnet run` from project folder.
   - The app should open in your browser (https) at the configured URL.
     
---

## If you want to temporarily bypass login (for testing)

Two simple options:

- Remove or comment the `[Authorize]` attribute on these controllers:
  - `Controllers/PersonsController.cs`
  - `Controllers/AccountsController.cs`
  - `Controllers/TransactionsController.cs`

- Or disable authentication in `Program.cs` by commenting out the authentication service and middleware registration:
  - Comment out `builder.Services.AddAuthentication(...).AddCookie(...)`
  - Comment out `app.UseAuthentication();`
  - Keep `app.UseAuthorization();` if you prefer.

Re-enable those lines when ready.

---

## Project structure (high level)

- `Controllers/` — MVC controllers (Home, Account, Persons, Accounts, Transactions)
- `Models/` — EF Core entity classes and `AppDbContext`
- `Views/` — Razor views organized per controller and `Shared` layout
- `Migrations/` — EF Core migrations (if you want to recreate DB)
- `appsettings.json` — update connection string here
- `Program.cs` — service registration (DbContext, authentication, session)

---

## Useful CLI commands

- Restore: `dotnet restore`
- Build: `dotnet build`
- Run: `dotnet run`
- Migrations (after installing `dotnet-ef`):
  - List migrations: `dotnet ef migrations list`
  - Apply migrations / create DB: `dotnet ef database update`
  - Add new migration (if you change models): `dotnet ef migrations add NameOfMigration`

---

## Troubleshooting / Tips

- If EF complains about incompatible schema, either:
  - Point to the existing database with the expected schema, or
  - Use the provided migrations to create the database (`dotnet ef database update`).
- If you get authentication redirect loops, check `Program.cs` and ensure cookie authentication is configured correctly.
- If Visual Studio does not restore packages automatically, run __Tools > NuGet Package Manager > Package Manager Console__ or run `dotnet restore` in the project folder.
- Logs: look at the terminal or Visual Studio Output window under __Show output from: Build__ or __Debug__.
