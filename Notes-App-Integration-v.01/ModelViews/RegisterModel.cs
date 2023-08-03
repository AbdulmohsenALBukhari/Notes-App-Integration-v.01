using System.ComponentModel.DataAnnotations;

namespace Notes_App_Integration_v._01.ModelViews
{
    public class RegisterModel
    {
        [StringLength(256), Required,EmailAddress]
        public string Email { get; set; }
        [StringLength(256), Required]
        public string UserName { get; set; }
        [Required]
        public string PasswordHash { get; set; }
    }
}
