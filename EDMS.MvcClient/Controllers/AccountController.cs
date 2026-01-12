using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace EDMS.MvcClient.Controllers;

public class AccountController : Controller
{
    private const string DefaultAfterLogin = "/";

    // LOGIN (OIDC Challenge -> IdentityServer)
    // /Account/Login?returnUrl=/Documents/Browse
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var target = NormalizeReturnUrl(returnUrl);

        if (User.Identity?.IsAuthenticated == true)
            return Redirect(target);

        return Challenge(
            new AuthenticationProperties { RedirectUri = target },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    // REGISTER on IdentityServer
   
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        var target = NormalizeReturnUrl(returnUrl);
        var url = $"https://localhost:7007/Account/Register?returnUrl={Uri.EscapeDataString(target)}";
        return Redirect(url);
    }

    // PROFILE on IdentityServer
    [HttpGet]
    public IActionResult Profile()
        => Redirect("https://localhost:7007/Account/Profile");

   
    [HttpGet]
    public IActionResult LocalLogout()
    {
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme
        );
    }

    private string NormalizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return DefaultAfterLogin;

        return Url.IsLocalUrl(returnUrl) ? returnUrl : DefaultAfterLogin;
    }
}
