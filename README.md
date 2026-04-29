# HealthInsurance
It is a Health Insurance App which keep record the customer policy , Payment , Renew 
# Health Insurance App - Onion Architecture

## Structure
- Domain: Entities (Policy, Claim, Customer), Enums
- Application: Interfaces, DTOs
- Infrastructure: EF Core SQL Server, Repositories, Migrations, Background Services
- Presentation: ASP.NET Core Web API with Swagger & Static Files

## Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- SQL Server (LocalDB is used by default) or update `appsettings.json` with your SQL Server connection string.

---

## Setup on a New PC

### 1. Install EF Core CLI tool
This is required to create/update the database from migrations.

```bash
dotnet tool install --global dotnet-ef
```

If you already have it installed but it's an older version, update it:
```bash
dotnet tool update --global dotnet-ef
```

### 2. Restore dependencies
From the solution root folder:

```bash
dotnet restore
```

### 3. Create / Update Database
The project uses EF Core Migrations. Apply them to create the database schema.

**From the solution root:**
```bash
dotnet ef database update --project Infrastructure --startup-project Presentation
```

### 4. Run the API

**Option A: HTTP only (default profile)**
```bash
cd Presentation
dotnet run
# Opens: http://localhost:5249
```

**Option B: HTTPS + HTTP**
```bash
cd Presentation
dotnet run --launch-profile https
# Opens: https://localhost:7176 and http://localhost:5249
```

### 5. Access the App
- Swagger UI: `http://localhost:5249/swagger` (or `https://localhost:7176/swagger` if using HTTPS profile)
- Static UI: `http://localhost:5249/index.html` or `http://localhost:5249/docs.html`

---

## Database Details
- **Provider:** SQL Server
- **Default Connection String:**
  ```
  Server=(localdb)\mssqllocaldb;Database=HealthInsuranceDb;Trusted_Connection=true;TrustServerCertificate=True;
  ```
- **Connection String Location:** `Presentation/appsettings.json` under `ConnectionStrings:DefaultConnection`
- The database (`HealthInsuranceDb`) will be created automatically by the migration command above.

---

## Run Summary

| Action | Command |
|---|---|
| Restore packages | `dotnet restore` |
| Update database | `dotnet ef database update --project Infrastructure --startup-project Presentation` |
| Run (HTTP only) | `cd Presentation && dotnet run` |
| Run (HTTPS + HTTP) | `cd Presentation && dotnet run --launch-profile https` |

