using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Techninxa.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Client { get; set; }

        public int? Year { get; set; }

        [MaxLength(500)]
        public string? ProjectUrl { get; set; }

        [MaxLength(300)]
        public string? ShortDescription { get; set; }

        public string? Overview { get; set; }

        [Required]
        [MaxLength(200)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "draft";

        // Hero image path / URL
        [MaxLength(500)]
        public string? HeroImage { get; set; }

        // Navigation
        public int? NextProjectId { get; set; }

        [ForeignKey(nameof(NextProjectId))]
        public Project? NextProject { get; set; }

        // Relationships
        public ICollection<ProjectMedia> Media { get; set; }
            = new List<ProjectMedia>();

        public ICollection<ProjectTechnology> Technologies { get; set; }
            = new List<ProjectTechnology>();

        public ICollection<ProjectFeature> Features { get; set; }
            = new List<ProjectFeature>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? PublishedAt { get; set; }
    }
}