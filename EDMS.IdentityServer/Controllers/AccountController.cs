using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using EDMS.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace EDMS.IdentityServer.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IIdentityServerInteractionService interaction)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
    }

    
    // LOGIN
   
    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid credentials.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberLogin,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Invalid credentials.");
            return View(model);
        }

       
        var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
        if (context != null)
            return Redirect(model.ReturnUrl);

       
        if (Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return Redirect("~/");
    }

    
    // REGISTER
  
    [HttpGet]
    public IActionResult Register(string returnUrl = "/")
    {
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.Phone,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "User");
        await _signInManager.SignInAsync(user, isPersistent: true);

        var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
        if (context != null)
            return Redirect(model.ReturnUrl);

        if (Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return Redirect("~/");
    }

   
    // PROFILE
 
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction(nameof(Login));

        var vm = new ProfileViewModel
        {
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            FullName = user.FullName ?? "",
            Phone = user.PhoneNumber ?? ""
        };

        return View(vm);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction(nameof(Login));

        user.FullName = model.FullName;
        user.PhoneNumber = model.Phone;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);

            return View(model);
        }

        ViewData["Saved"] = true;
        return View(model);
    }
  
    // LOGOUT
    

    
    [HttpGet]
    public IActionResult Logout(string? logoutId = null, string? backUrl = null, string? postLogoutUrl = null)
    {
       
        backUrl ??= "https://localhost:7219/";
        postLogoutUrl ??= "https://localhost:7219/Account/LocalLogout";

        return View(new LogoutViewModel
        {
            LogoutId = logoutId,
            BackUrl = backUrl,
            PostLogoutUrl = postLogoutUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(LogoutViewModel model)
    {
       
        await _signInManager.SignOutAsync();

        await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);

        
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

      
        if (!string.IsNullOrWhiteSpace(model.PostLogoutUrl))
            return Redirect(model.PostLogoutUrl);

        return Redirect("https://localhost:7219/");
    }




}
