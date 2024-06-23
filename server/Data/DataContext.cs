using Microsoft.EntityFrameworkCore;
using TobyBlazor.Models;

namespace TobyBlazor.Data;

public class DataContext : DbContext
{
    public DbSet<Video> Videos { get; init; }
    public DbSet<Group> Groups { get; init; }
    public DbSet<Preferences> Preferences { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Filename=videos.db");
        base.OnConfiguring(optionsBuilder);
    }
}