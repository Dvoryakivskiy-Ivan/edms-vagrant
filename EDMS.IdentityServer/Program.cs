using Duende.IdentityServer;
using EDMS.IdentityServer;
using EDMS.IdentityServer.Data;
using EDMS.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Connection string

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


// DbContext (ASP.NET Identity)

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


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

// External Authentication (Google)

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


// Middleware pipeline

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.UseAuthentication();   
app.UseIdentityServer();   // endpoints OAuth2/OIDC
app.UseAuthorization();

app.MapDefaultControllerRoute();
app.MapRazorPages();


// Seed Roles + Admin user

using (var scope = app.Services.CreateScope())
{
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

app.Run();
