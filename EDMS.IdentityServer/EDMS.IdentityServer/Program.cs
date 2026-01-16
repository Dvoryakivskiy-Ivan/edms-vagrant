using Duende.IdentityServer;
using EDMS.IdentityServer;
using EDMS.IdentityServer.Data;
using EDMS.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Connection string
// Provider is selected via appsettings.json: Database:Provider
// Supports: SqlServer / Postgres / Sqlite / InMemory

var provider = builder.Configuration["Database:Provider"]?.Trim() ?? "SqlServer";

string? connectionString = provider.ToLowerInvariant() switch
{
    "sqlserver" or "mssql" => builder.Configuration.GetConnectionString("SqlServer"),
    "postgres" or "postgresql" => builder.Configuration.GetConnectionString("Postgres"),
    "sqlite" => builder.Configuration.GetConnectionString("Sqlite"),
    "inmemory" or "memory" => builder.Configuration.GetConnectionString("InMemory") ?? "EDMS_Identity_InMemory",
    _ => throw new InvalidOperationException(
        $"Unknown Database:Provider '{provider}'. Use: SqlServer | Postgres | Sqlite | InMemory")
};


// DbContext (ASP.NET Identity)

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    switch (provider.ToLowerInvariant())
    {
        case "sqlserver":
        case "mssql":
            options.UseSqlServer(connectionString);
            break;

        case "postgres":
        case "postgresql":
            options.UseNpgsql(connectionString);
            break;

        case "sqlite":
            options.UseSqlite(connectionString);
            break;

        case "inmemory":
        case "memory":
            options.UseInMemoryDatabase(
                builder.Configuration.GetConnectionString("InMemory")?.Trim() ?? "EDMS_Identity_InMemory");
            break;

    }
});


// ASP.NET Identity (Users + Roles) 
// Password: 8-16, 1 digit, 1 sign, 1 capital

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();




builder.Services.AddAuthentication()
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = IdentityConstants.ExternalScheme;

        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "PUT_CLIENT_ID_HERE";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "PUT_CLIENT_SECRET_HERE";
    });


// Duende IdentityServer (OAuth2 / OIDC)

builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
})
.AddAspNetIdentity<ApplicationUser>()
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiScopes(Config.ApiScopes)
.AddInMemoryApiResources(Config.ApiResources)
.AddInMemoryClients(Config.Clients)
.AddDeveloperSigningCredential();                      // DEV only


// UI 
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();


// Create DB (Code First) + Seed Roles + Admin user
// Runs at startup for all providers.

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


    db.Database.Migrate();


    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // default Admin 
    var adminEmail = "admin@edms.local";
    var adminPassword = "Admin!1234"; // 8-16, 1 digit, 1 sign, 1 capital

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "System Administrator",
            PhoneNumber = "+380501112233"
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
    else
    {
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}


// Middleware pipeline

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseIdentityServer();   // endpoints OAuth2/OIDC
app.UseAuthorization();

app.MapDefaultControllerRoute();
app.MapRazorPages();

app.Run();
