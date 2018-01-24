using Microsoft.EntityFrameworkCore;

namespace tellick_admin.Repository {
    public class TellickAdminContext : DbContext {
        public TellickAdminContext(DbContextOptions<TellickAdminContext> options) : base(options) {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Project>().HasOne(l => l.Customer).WithMany().HasForeignKey(l => l.CustomerId);
            modelBuilder.Entity<Log>().HasOne(l => l.Project).WithMany().HasForeignKey(l => l.ProjectId);
        }
    }
}