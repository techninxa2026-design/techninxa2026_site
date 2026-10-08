using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Techninxa.Models
{
    public class ProjectCreateViewModel
    {
        // ==========================
        // BASIC INFORMATION
        // ==========================

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


        // ==========================
        // HERO IMAGE
        // ==========================

        [Required]
        public IFormFile? HeroImage { get; set; }


        // ==========================
        // PROJECT MEDIA
        // ==========================

        public List<IFormFile> Images { get; set; }
            = new List<IFormFile>();

        public List<IFormFile> Videos { get; set; }
            = new List<IFormFile>();


        // ==========================
        // TECHNOLOGIES
        // ==========================

        public List<string> Technologies { get; set; }
            = new List<string>();


        // ==========================
        // FEATURES
        // ==========================

        public List<string> FeatureTitles { get; set; }
            = new List<string>();

        public List<string> FeatureDescriptions { get; set; }
            = new List<string>();


        // ==========================
        // NAVIGATION
        // ==========================

        public int? NextProject { get; set; }

        [MaxLength(200)]
        public string Slug { get; set; } = string.Empty;


        // ==========================
        // STATUS
        // ==========================

        public string Status { get; set; } = "draft";
    }
}