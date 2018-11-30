using Microsoft.EntityFrameworkCore;

namespace quizrtAuthServer.Models
{
    public class AuthContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public AuthContext (DbContextOptions<AuthContext> options) : base(options)
        {
            this.Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity => entity.HasIndex(e => e.Email).IsUnique());
        }
    }
}