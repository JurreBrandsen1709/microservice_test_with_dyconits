using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data
{
    public class AppDbContext : DbContext // inherit from DbContext
    {
        // constructor
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet<Platform> is a collection of Platform objects
        public DbSet<Platform> Platforms { get; set; }
    }
}