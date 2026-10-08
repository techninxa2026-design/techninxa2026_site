using Microsoft.EntityFrameworkCore;
using techninxa.Models;
using Techninxa.Models;

namespace Techninxa
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // PROJECT → MEDIA
            // ============================================

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Media)
                .WithOne(m => m.Project)
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================
            // PROJECT → TECHNOLOGIES
            // ============================================

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Technologies)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================
            // PROJECT → FEATURES
            // ============================================

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Features)
                .WithOne(f => f.Project)
                .HasForeignKey(f => f.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================
            // PROJECT → NEXT PROJECT
            // Self-referencing relationship
            // ============================================

            modelBuilder.Entity<Project>()
                .HasOne(p => p.NextProject)
                .WithMany()
                .HasForeignKey(p => p.NextProjectId)
                .OnDelete(DeleteBehavior.NoAction);
        }


        // ============================================
        // DATABASE TABLES
        // ============================================
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Meeting> Meetings { get; set; }

        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        public DbSet<AdminUser> User { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<ProjectMedia> ProjectMedia { get; set; }

        public DbSet<ProjectTechnology> ProjectTechnologies { get; set; }

        public DbSet<ProjectFeature> ProjectFeatures { get; set; }
        public DbSet<HolidaySetting> HolidaySettings { get; set; }
        
    }
}