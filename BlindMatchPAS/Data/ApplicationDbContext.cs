using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Project -> StudentProfile relationship
            builder.Entity<Project>()
                .HasOne(p => p.StudentProfile)
                .WithMany(u => u.StudentProjects)
                .HasForeignKey(p => p.StudentId)
                .HasPrincipalKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Project -> SupervisorProfile relationship
            builder.Entity<Project>()
                .HasOne(p => p.SupervisorProfile)
                .WithMany(u => u.SupervisorProjects)
                .HasForeignKey(p => p.SupervisorId)
                .HasPrincipalKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}