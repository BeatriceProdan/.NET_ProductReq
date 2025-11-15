using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ProductsManagement.Data;
using ProductsManagement.Features.Products.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductsManagement.Features.Products.Handlers
{
    public class CreateProductHandler : IRequestHandler<CreateProductProfileRequest, ProductProfileDto>
    {
        private readonly ApplicationContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductHandler> _logger;
        private readonly IMemoryCache _cache;

        public CreateProductHandler(
            ApplicationContext context,
            IMapper mapper,
            ILogger<CreateProductHandler> logger,
            IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<ProductProfileDto> Handle(CreateProductProfileRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting product creation for SKU={SKU}, Name={Name}, Brand={Brand}, Category={Category}",
                request.SKU, request.Name, request.Brand, request.Category);

            var exists = await _context.Products
                .AnyAsync(p => p.SKU == request.SKU, cancellationToken);

            if (exists)
            {
                _logger.LogWarning("Product with SKU={SKU} already exists.", request.SKU);
                throw new InvalidOperationException($"Product with SKU '{request.SKU}' already exists.");
            }

            var product = _mapper.Map<Product>(request);

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            _cache.Remove("all_products");

            var dto = _mapper.Map<ProductProfileDto>(product);

            _logger.LogInformation("Product created successfully: {Id}", product.Id);

            return dto;
        }
    }
}
