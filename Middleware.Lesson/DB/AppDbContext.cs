using Microsoft.EntityFrameworkCore;
using Middleware.Lesson.Models;

namespace Middleware.Lesson.DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Aggiungi DbSet per le tue entità qui
        public DbSet<User> Users { get; set; }
    }


}
