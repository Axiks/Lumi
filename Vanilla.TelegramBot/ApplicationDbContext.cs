using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.TelegramBot.Entityes;

namespace Vanilla.TelegramBot
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserEntity> Users {  get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        { }
    }
}
