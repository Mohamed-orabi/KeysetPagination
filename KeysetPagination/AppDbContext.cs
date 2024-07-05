using Microsoft.EntityFrameworkCore;

namespace KeysetPagination
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=ORABI;Initial Catalog=Test;Integrated Security=true;Trusted_Connection=Yes;TrustServerCertificate=True");
        }
    }
}
