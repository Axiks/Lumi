using Microsoft.EntityFrameworkCore;
using Vanilla.OAuth.Entities;

namespace Vanilla.OAuth
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        { }
    }
}
