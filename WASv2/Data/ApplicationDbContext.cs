using Microsoft.EntityFrameworkCore;
using WASv2.Models;

namespace WASv2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<PRModel> PRs { get; set; }
        public DbSet<PRItemModel> PRItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PRModel>()
                .HasMany(p => p.Items)
                .WithOne(i => i.PR)
                .HasForeignKey(i => i.PRId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PRModel>()
                .HasIndex(p => p.PRNumber)
                .IsUnique();

            modelBuilder.Entity<PRItemModel>()
                .Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PRItemModel>()
                .Property(i => i.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PRModel>()
                .Property(p => p.TotalAmount)
                .HasPrecision(18, 2);
        }
    }
}