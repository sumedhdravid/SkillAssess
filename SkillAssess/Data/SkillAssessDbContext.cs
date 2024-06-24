using Microsoft.EntityFrameworkCore;
using SkillAssess.Models;

namespace SkillAssess.Data
{
    public class SkillAssessDbContext : DbContext
    {
        public SkillAssessDbContext(DbContextOptions<SkillAssessDbContext> options) : base(options)
        {
        }

        // Define DbSet for each model (table) in the database
        public DbSet<Employee> Employees { get; set; }
        public DbSet<SkillAssessment> SkillAssessment { get; set; }
        public DbSet<AuthUser> AuthUsers { get; set; }
    }
}
