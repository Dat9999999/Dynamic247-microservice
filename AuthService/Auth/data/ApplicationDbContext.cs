using Microsoft.EntityFrameworkCore;
using NewsPage.Models.entities;

namespace NewsPage.data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var adminEmail = _configuration["AdminSettings:Email"];
            var adminPasswordHash = _configuration["AdminSettings:PasswordHash"];

            //create  admin account
            modelBuilder.Entity<UserAccounts>().HasData(
                new UserAccounts
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    Email = adminEmail,
                    Password = adminPasswordHash, // Lấy từ appsettings.json
                    Role = "Admin",
                    CreatedAt = new DateTime(2024, 3, 20, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Enable",
                    IsVerified = true,
                }
            );



            //config unique email
            modelBuilder.Entity<UserAccounts>()
                .HasIndex(u => u.Email)
                .IsUnique();

            //define relation 1 to 1 in UserAccounts and UserDetails 
            modelBuilder.Entity<UserAccounts>()
                .HasOne<UserDetails>()
                .WithOne()
                .HasForeignKey<UserDetails>(ud => ud.UserAccountId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<UserAccounts> UserAccounts { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }
    }
}
