using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EDMS.IdentityServer.Models;

public class ApplicationUser : IdentityUser
{
    // Full name (max 500)
    [Required]
    [StringLength(500)]
    public string FullName { get; set; } = string.Empty;

   
}
