using Microsoft.EntityFrameworkCore;
using TiTools_backend.Models;


namespace TiTools_backend.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Equipment>? Equipments { get; set; }
    }
}
