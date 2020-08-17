using Microsoft.EntityFrameworkCore;
using TobyBlazor.Models;

namespace TobyBlazor.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Preferences> Preferences { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=videos.db");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
