using EDMS.MvcClient.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// MVC + API controllers
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();


// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<EDMS.MvcClient.Swagger.ConfigureSwaggerOptions>();

// Database provider switch
var provider = builder.Configuration["Database:Provider"]?.Trim() ?? "SqlServer";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    switch (provider.ToLowerInvariant())
    {
        case "sqlserver":
        case "mssql":
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
            break;

        case "postgres":
        case "postgresql":
            options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
            break;

        case "sqlite":
            options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
            break;

        case "inmemory":
        case "memory":
            options.UseInMemoryDatabase(builder.Configuration.GetConnectionString("InMemory") ?? "EDMS_InMemory");
            break;

        default:
            throw new InvalidOperationException(
                $"Unknown Database:Provider '{provider}'. Use: SqlServer | Postgres | Sqlite | InMemory");
    }
});

// Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.SlidingExpiration = true;
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = "https://localhost:7007";
    options.RequireHttpsMetadata = true;

    options.ClientId = "edms_mvc";
    options.ClientSecret = "mvc_secret";

    options.ResponseType = "code";
    options.UsePkce = true;

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    options.CallbackPath = "/signin-oidc";
    options.SignedOutCallbackPath = "/signout-callback-oidc";

    options.MapInboundClaims = false;

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("roles");
    options.Scope.Add("edms_api");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };

    options.ClaimActions.DeleteClaim("role");
    options.ClaimActions.MapJsonKey("role", "role");
});

builder.Services.AddAuthorization();
var serviceName = "EDMS.MvcClient";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation(o => o.RecordException = true)
            .AddHttpClientInstrumentation()
            .AddZipkinExporter(o =>
            {
                o.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
            });
    });


var app = builder.Build();

// DB migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

// Swagger (dev only)
if (app.Environment.IsDevelopment())
{
    var apiVersionProvider =
        app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        foreach (var desc in apiVersionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{desc.GroupName}/swagger.json",
                $"EDMS.MvcClient {desc.GroupName.ToUpperInvariant()}");
        }
    });
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // API controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseHttpMetrics();      // adds default http metrics
app.MapMetrics("/metrics"); // exposes /metrics


app.Run();

// Needed for WebApplicationFactory integration tests
public partial class Program { }
