using System.ComponentModel.DataAnnotations;

namespace techninxa.Models
{
    public class AdminUser
    {
        public int Id { get; set; }

        [Required, EmailAddress, MaxLength(150)]
        public string Username { get; set; } = default!;  

        [Required]
        public string Password { get; set; } = default!;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = default!;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = default!;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; } = false;
    }
}
