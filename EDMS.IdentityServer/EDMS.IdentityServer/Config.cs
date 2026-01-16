using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace EDMS.IdentityServer;

public static class Config
{

    private const string MvcBaseUrl = "https://localhost:7219";


    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),

            new IdentityResource(
                name: "roles",
                displayName: "User roles",
                userClaims: new[] { "role" }
            )
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("edms_api", "EDMS API")
        };

 
    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("edms_api", "EDMS API")
            {
                Scopes = { "edms_api" },
                UserClaims = { "role" }
            }
        };


    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client
            {
                ClientId = "edms_mvc",
                ClientName = "EDMS MVC Client",

                AllowedGrantTypes = GrantTypes.Code,

                RequirePkce = true,
                AllowPlainTextPkce = false,

                RequireClientSecret = true,
                ClientSecrets =
                {
                    new Secret("mvc_secret".Sha256())
                },

              RedirectUris =
              {
                  "https://localhost:7219/signin-oidc"
              },
              PostLogoutRedirectUris =
               {
               "https://localhost:7219/signout-callback-oidc"
                  },
              AllowedCorsOrigins =
               {
                MvcBaseUrl
               },

                RequireConsent = false,

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "roles",
                    "edms_api"
                },

                AlwaysIncludeUserClaimsInIdToken = true
            }
        };
}
