using System.ComponentModel.DataAnnotations;

namespace techninxa.Models
{
    public class Meeting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ProjectType { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }

        [Required]
        public DateTime MeetingDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public bool IsCancelled { get; set; } = false;
    }
}