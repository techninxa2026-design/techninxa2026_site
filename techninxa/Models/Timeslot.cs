using System.ComponentModel.DataAnnotations;

namespace techninxa.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
