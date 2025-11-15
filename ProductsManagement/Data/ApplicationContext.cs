using Microsoft.EntityFrameworkCore;
using ProductsManagement.Features.Products;

namespace ProductsManagement.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
    }
}
