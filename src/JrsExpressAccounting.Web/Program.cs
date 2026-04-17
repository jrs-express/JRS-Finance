using JrsExpressAccounting.Web.Components;
using JrsExpressAccounting.Web.Data;
using JrsExpressAccounting.Web.Infrastructure;
using JrsExpressAccounting.Web.Models;
using JrsExpressAccounting.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=JrsExpressAccounting;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
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

await ApplyMigrationsAndSeedAsync(app.Services);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static async Task ApplyMigrationsAndSeedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    await ResetSchemaIfIdentityTablesAreMissingAsync(dbContext, environment);

    await dbContext.Database.MigrateAsync();

    var seeder = new SeedData(scope.ServiceProvider);
    await seeder.SeedAsync();
}

static async Task ResetSchemaIfIdentityTablesAreMissingAsync(
    ApplicationDbContext dbContext,
    IWebHostEnvironment environment)
{
    if (!environment.IsDevelopment())
    {
        return;
    }

    var database = dbContext.Database;

    if (!await database.CanConnectAsync())
    {
        return;
    }

    var connection = database.GetDbConnection();

    await connection.OpenAsync();
    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT OBJECT_ID(N'[dbo].[AspNetRoles]', N'U')";

    var aspNetRolesTableExists = await command.ExecuteScalarAsync() is not DBNull and not null;
    await connection.CloseAsync();

    if (aspNetRolesTableExists)
    {
        return;
    }

    await database.EnsureDeletedAsync();
}
