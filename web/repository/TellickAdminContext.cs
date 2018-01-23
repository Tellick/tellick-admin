using Microsoft.EntityFrameworkCore;

namespace tellick_admin.Repository {
    public class TellickAdminContext : DbContext {
        public TellickAdminContext(DbContextOptions<TellickAdminContext> options) : base(options) {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Project> Projects { get; set; }
    }
}