using System.ComponentModel.DataAnnotations;

namespace Notes_App_Integration_v._01.ModelViews
{
    public class LoginViewModel
    {
        [StringLength(256), Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public bool RememberMe { get; set; }
    }
}
