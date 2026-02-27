using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
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

        
    }
}
