using System.ComponentModel.DataAnnotations;

namespace techninxa.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
    public class CompanyChatLoginViewModel
    {
        [Required]
        public string Username { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}
