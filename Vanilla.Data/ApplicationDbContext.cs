using Microsoft.EntityFrameworkCore;
using Vanilla.Data.Entities;

namespace Vanilla.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<ProjectEntity> Projects { get; set; }
        public DbSet<UserEntity> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        { }
    }
}
