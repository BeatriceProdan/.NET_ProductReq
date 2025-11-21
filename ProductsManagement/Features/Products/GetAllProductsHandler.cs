using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductsManagement.Features.Products.DTOs;
using ProductsManagement.Persistence;

namespace ProductsManagement.Features.Products;

public class GetAllProductsHandler(ProductsManagementContext context, IMapper mapper)
{
    public async Task<IResult> Handle(GetAllProductsRequest request, CancellationToken cancellationToken = default)
    {
        var products = await context.Products.ToListAsync(cancellationToken);
        var dtos = mapper.Map<List<ProductProfileDto>>(products);

        return Results.Ok(dtos);
    }
}