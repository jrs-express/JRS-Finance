using JrsExpressAccounting.Web.Components;
using JrsExpressAccounting.Web.Data;
using JrsExpressAccounting.Web.Infrastructure;
using JrsExpressAccounting.Web.Models;
using JrsExpressAccounting.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddAuthorizationBuilder();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = "Server=(localdb)\\mssqllocaldb;Database=JrsExpressAccounting;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";
}

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
    connectionString.Contains("Trusted_Connection=True", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "The current SQL Server connection string uses Trusted_Connection=True (Windows Authentication), " +
        "which only works on Windows. Use SQL authentication when running on Linux/macOS.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, NoOpEmailSender>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var seeder = new SeedData(scope.ServiceProvider);
    await seeder.SeedAsync();
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
