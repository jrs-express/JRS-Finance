using JrsExpressAccounting.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace JrsExpressAccounting.Web.Data;

public class SeedData(IServiceProvider services)
{
    public async Task SeedAsync()
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await EnsureSchemaAsync(db);

        foreach (var role in new[] { "Admin", "AccountingAdmin", "Accounting" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (!await db.Branches.AnyAsync())
        {
            db.Branches.AddRange(
                new Branch { Code = "MNL-HQ", Name = "Manila Head Office", Address = "Makati City", Tin = "000-123-456-000" },
                new Branch { Code = "CEB-01", Name = "Cebu Branch", Address = "Cebu City", Tin = "000-123-456-001" });
        }

        if (!await db.ChartOfAccounts.AnyAsync())
        {
            db.ChartOfAccounts.AddRange(
                new ChartOfAccount { AccountCode = "1000", AccountName = "Cash on Hand", AccountType = "Asset" },
                new ChartOfAccount { AccountCode = "1010", AccountName = "Cash in Bank", AccountType = "Asset" },
                new ChartOfAccount { AccountCode = "4000", AccountName = "Service Income", AccountType = "Revenue" },
                new ChartOfAccount { AccountCode = "5000", AccountName = "Operating Expense", AccountType = "Expense" },
                new ChartOfAccount { AccountCode = "2100", AccountName = "Output VAT Payable", AccountType = "Liability" },
                new ChartOfAccount { AccountCode = "2200", AccountName = "Withholding Tax Payable", AccountType = "Liability" });
        }

        await db.SaveChangesAsync();

        if (!await db.Banks.AnyAsync())
        {
            var bankCoa = await db.ChartOfAccounts.FirstAsync(x => x.AccountCode == "1010");
            db.Banks.Add(new Bank
            {
                BankName = "BDO",
                AccountNumber = "1234-5678-90",
                ChartOfAccountId = bankCoa.Id
            });
        }

        if (!await db.BusinessParties.AnyAsync())
        {
            db.BusinessParties.AddRange(
                new BusinessParty { PartyType = "Customer", Name = "ABC Retail Corp", Tin = "123-555-777-000", RegisteredAddress = "Quezon City", ContactPerson = "Ana Cruz", VatType = "VAT" },
                new BusinessParty { PartyType = "Vendor", Name = "Fast Supplies Inc", Tin = "222-888-000-000", RegisteredAddress = "Pasig City", ContactPerson = "Leo Tan", VatType = "NON-VAT" });
        }

        await db.SaveChangesAsync();

        var adminEmail = "admin@jrs.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Admin",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, "Admin@12345");
            await userManager.AddToRolesAsync(admin, ["Admin", "AccountingAdmin"]);
        }

        var accountingEmail = "accounting@jrs.local";
        var accounting = await userManager.FindByEmailAsync(accountingEmail);
        if (accounting is null)
        {
            accounting = new ApplicationUser
            {
                UserName = accountingEmail,
                Email = accountingEmail,
                FullName = "Accounting User",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(accounting, "Accounting@12345");
            await userManager.AddToRoleAsync(accounting, "Accounting");
        }
    }

    private static async Task EnsureSchemaAsync(ApplicationDbContext db)
    {
        // This starter template ships with an intentionally empty first migration.
        // In that state, Migrate() can create only __EFMigrationsHistory, which then
        // blocks EnsureCreated() and causes runtime errors such as missing AspNetRoles.
        var roleTableExists = await TableExistsAsync(db, "AspNetRoles");

        if (!roleTableExists)
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
            return;
        }

        await db.Database.EnsureCreatedAsync();
    }

    private static async Task<bool> TableExistsAsync(ApplicationDbContext db, string tableName)
    {
        await using var conn = db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT 1
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @tableName
            """;
        cmd.Parameters.Add(new SqlParameter("@tableName", tableName));
        var result = await cmd.ExecuteScalarAsync();
        return result is not null and not DBNull;
    }
}
