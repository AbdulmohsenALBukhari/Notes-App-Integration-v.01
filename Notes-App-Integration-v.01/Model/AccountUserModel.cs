using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Notes_App_Integration_v._01.Model
{
    public class AccountUserModel : IdentityUser
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        [Compare("password")]
        public string ConfirmPassword { get; set; }
    }
}
