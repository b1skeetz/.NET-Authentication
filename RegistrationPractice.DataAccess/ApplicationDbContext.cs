using Microsoft.EntityFrameworkCore;
using RegistrationPractice.Domain;

namespace RegistrationPractice.DataAccess;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
}