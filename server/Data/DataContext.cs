using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TobyBlazor.Models;

namespace TobyBlazor.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=videos.db");
            base.OnConfiguring(optionsBuilder);
        }        
    }
}
