using JrsExpressAccounting.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JrsExpressAccounting.Web.Data;

public class SeedData(IServiceProvider services)
{
    public async Task SeedAsync()
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await SeedRolesAsync(roleManager);
        await SeedMasterDataAsync(db);
        await SeedUsersAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "AccountingAdmin", "Accounting" })
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var createResult = await roleManager.CreateAsync(new IdentityRole(role));
            EnsureSuccess(createResult, $"creating role '{role}'");
        }
    }

    private static async Task SeedMasterDataAsync(ApplicationDbContext db)
    {
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
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        await EnsureUserAsync(
            userManager,
            email: "admin@jrs.local",
            fullName: "System Admin",
            password: "Admin@12345",
            roles: ["Admin", "AccountingAdmin"]);

        await EnsureUserAsync(
            userManager,
            email: "accounting@jrs.local",
            fullName: "Accounting User",
            password: "Accounting@12345",
            roles: ["Accounting"]);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        string[] roles)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            EnsureSuccess(createResult, $"creating user '{email}'");
        }

        var missingRoles = roles.Except(await userManager.GetRolesAsync(user)).ToArray();
        if (missingRoles.Length > 0)
        {
            var addRoleResult = await userManager.AddToRolesAsync(user, missingRoles);
            EnsureSuccess(addRoleResult, $"assigning roles to '{email}'");
        }
    }

    private static void EnsureSuccess(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(x => x.Description));
        throw new InvalidOperationException($"Error while {operation}: {errors}");
    }
}
