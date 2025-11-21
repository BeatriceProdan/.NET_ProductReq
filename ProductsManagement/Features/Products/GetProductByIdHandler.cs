using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductsManagement.Features.Products.DTOs;
using ProductsManagement.Persistence;

namespace ProductsManagement.Features.Products;

public class GetProductByIdHandler(ProductsManagementContext context, IMapper mapper)
{
    public async Task<IResult> Handle(GetProductByIdRequest request, CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
        {
            return Results.NotFound();
        }

        var dto = mapper.Map<ProductProfileDto>(product);
        return Results.Ok(dto);
    }
}