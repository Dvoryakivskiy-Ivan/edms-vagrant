using System.ComponentModel.DataAnnotations;

namespace EDMS.IdentityServer.Models;

public class ProfileViewModel
{
   
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";

    [Required]
    [StringLength(500)]
    public string FullName { get; set; } = "";

    [Required]
    [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Format: +380XXXXXXXXX")]
    public string Phone { get; set; } = "+380";
}
