using System.ComponentModel.DataAnnotations;

namespace EDMS.IdentityServer.Models;

public class RegisterViewModel
{
    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = "";

    [Required]
    [StringLength(500)]
    public string FullName { get; set; } = "";

    [Required]
    [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Format: +380XXXXXXXXX")]
    public string Phone { get; set; } = "+380";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [StringLength(16, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,16}$")]
    public string Password { get; set; } = "";

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";

    public string ReturnUrl { get; set; } = "/";
}
