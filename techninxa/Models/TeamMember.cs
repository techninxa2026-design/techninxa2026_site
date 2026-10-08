using System.ComponentModel.DataAnnotations;

namespace techninxa.Models
{
    public class TeamMember
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Role { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Image { get; set; }

        [StringLength(500)]
        public string? AltText { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
