using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.Data.Entities;

namespace Vanilla.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<ProjectEntity> Projects { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        { }
    }
}
