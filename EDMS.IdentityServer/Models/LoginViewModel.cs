using System.ComponentModel.DataAnnotations;

namespace EDMS.IdentityServer.Models;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberLogin { get; set; } = true;

    public string ReturnUrl { get; set; } = "/";
}
