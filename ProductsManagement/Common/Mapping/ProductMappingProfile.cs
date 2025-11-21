using AutoMapper;
using ProductsManagement.Features.Products;
using ProductsManagement.Features.Products.DTOs;

namespace ProductsManagement.Common.Mapping;

/// <summary>
/// Basic mapping profile for products without advanced logic.
/// </summary>
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Basic mappings
        CreateMap<CreateProductProfileRequest, Product>();

        CreateMap<Product, ProductProfileDto>();
    }
}