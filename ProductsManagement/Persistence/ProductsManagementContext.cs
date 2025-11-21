using Microsoft.EntityFrameworkCore;
using ProductsManagement.Features.Products;

namespace ProductsManagement.Persistence;

public class ProductsManagementContext(DbContextOptions<ProductsManagementContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.SKU).IsUnique();
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Brand).IsRequired().HasMaxLength(100);
            entity.Property(p => p.SKU).IsRequired().HasMaxLength(20);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
        });

        base.OnModelCreating(modelBuilder);
    }
}
