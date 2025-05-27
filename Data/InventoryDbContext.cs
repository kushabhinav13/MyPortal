using Microsoft.EntityFrameworkCore;
using Myportal.Models;
using Myportal.Data;
namespace Myportal.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) 
            : base(options) { }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Asset)
                .WithMany(a => a.Assignments)
                .HasForeignKey(a => a.AssetId);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Assignments)
                .HasForeignKey(a => a.EmployeeId);

            modelBuilder.Entity<MaintenanceLog>()
                .HasOne(m => m.Asset)
                .WithMany(a => a.MaintenanceLogs)
                .HasForeignKey(m => m.AssetId);
        }
    }
}