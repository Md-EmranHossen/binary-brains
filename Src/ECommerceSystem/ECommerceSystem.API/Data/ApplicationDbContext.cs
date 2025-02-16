using ECommerceSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceSystem.API.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }
    }
}
