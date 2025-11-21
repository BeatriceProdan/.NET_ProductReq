using Microsoft.EntityFrameworkCore;
using ProductsManagement.Persistence;

namespace ProductsManagement.Features.Products;

public class DeleteProductHandler(ProductsManagementContext context)
{
    public async Task<IResult> Handle(DeleteProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
        {
            return Results.NotFound();
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}