using System.ComponentModel.DataAnnotations;

namespace Techninxa.Models
{
    public class CompanyChatLoginViewModel
    {
        [Required]
        public string Username { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
        [Required]
        public string DisplayName { get; set; } = "";
    }
}