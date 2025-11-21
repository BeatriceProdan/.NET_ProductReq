using ProductsManagement.Persistence;

namespace ProductsManagement.Features.Products;

public class UpdateProductHandler(ProductsManagementContext dbContext)
{
    public async Task<IResult> Handle(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var existingProduct = await dbContext.Products
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (existingProduct is null)
        {
            return Results.NotFound();
        }

        existingProduct.Name = request.Name;
        existingProduct.Brand = request.Brand;
        existingProduct.SKU = request.SKU;
        existingProduct.Category = request.Category;
        existingProduct.Price = request.Price;
        existingProduct.ReleaseDate = request.ReleaseDate;
        existingProduct.ImageUrl = request.ImageUrl;
        existingProduct.StockQuantity = request.StockQuantity;
        existingProduct.UpdateAvailability();
        existingProduct.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}