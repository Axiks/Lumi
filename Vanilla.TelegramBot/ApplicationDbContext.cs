using Microsoft.EntityFrameworkCore;
using Vanilla.TelegramBot.Entityes;

namespace Vanilla.TelegramBot
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        { }
    }
}
