# JRS Express Accounting (BIR CAS-Oriented)

Visual Studio solution for a Blazor + EF Core + SQL Server accounting system for multi-branch JRS Express operations.

## Implemented modules

- Authentication + role-based authorization (Admin, AccountingAdmin, Accounting)
- Branch maintenance
- Chart of accounts with sub-account hierarchy
- Bank maintenance linked to chart of accounts
- Customer/vendor maintenance with BIR fields (TIN, VAT type, address)
- Journal entries (with VAT and withholding tax columns)
- Income/deposit, expense/withdrawal, and cash disbursement views
- Check printing register
- Dashboard
- General Ledger and Income Statement reports
- BIR CAS report summary page with filters-ready structure
- Audit log entity and service for user/action/timestamp change capture
- Pagination/filter/sorting sample on branch list
- Seed data for roles, users, master data

## Default users

- `admin@jrs.local` / `Admin@12345`
- `accounting@jrs.local` / `Accounting@12345`

## Tech stack

- Blazor Server (.NET 8)
- EF Core (code-first)
- SQL Server

Connection string in `appsettings.json`:

```json
"Server=LAPTOP-9NVM87DO\\SQLEXPRESS;Database=JrsExpressAccounting;Trusted_Connection=True;TrustServerCertificate=True;"
```

## Run in Visual Studio

1. Open `JrsExpressAccounting.sln` in Visual Studio 2022.
2. Restore NuGet packages.
3. Run EF migration update (or regenerate migration from current models):
   - `dotnet ef migrations add InitialCreate`
   - `dotnet ef database update`
4. Run the web project.
