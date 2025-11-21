using System.Diagnostics;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ProductsManagement.Common.Logging;
using ProductsManagement.Features.Products.DTOs;
using ProductsManagement.Persistence;

namespace ProductsManagement.Features.Products;

public class CreateProductHandler(
    ProductsManagementContext context,
    IMapper mapper,
    IValidator<CreateProductProfileRequest> validator,
    ILogger<CreateProductHandler> logger,
    IMemoryCache cache)
{
    public async Task<IResult> Handle(CreateProductProfileRequest request, CancellationToken cancellationToken = default)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];

        var totalStopwatch = Stopwatch.StartNew();
        var validationStopwatch = new Stopwatch();
        var dbStopwatch = new Stopwatch();

        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["OperationId"] = operationId,
            ["SKU"] = request.SKU,
            ["Category"] = request.Category.ToString()
        });

        logger.LogInformation(
            new EventId(LogEvents.ProductCreationStarted, nameof(LogEvents.ProductCreationStarted)),
            "Starting product creation for {Name}, Brand {Brand}, SKU {SKU}, Category {Category}",
            request.Name,
            request.Brand,
            request.SKU,
            request.Category.ToString());

        try
        {
            // VALIDATION
            validationStopwatch.Start();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            validationStopwatch.Stop();

            if (!validationResult.IsValid)
            {
                var errorReason = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));

                logger.LogWarning(
                    new EventId(LogEvents.ProductValidationFailed, nameof(LogEvents.ProductValidationFailed)),
                    "Product validation failed for SKU {SKU}. Errors: {Errors}",
                    request.SKU,
                    errorReason);

                var failedMetrics = new ProductCreationMetrics(
                    operationId,
                    request.Name,
                    request.SKU,
                    request.Category,
                    validationStopwatch.Elapsed,
                    TimeSpan.Zero,
                    totalStopwatch.Elapsed,
                    false,
                    errorReason);

                logger.LogProductCreationMetrics(failedMetrics);

                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            logger.LogInformation(
                new EventId(LogEvents.SKUValidationPerformed, nameof(LogEvents.SKUValidationPerformed)),
                "SKU validation performed for {SKU}", request.SKU);

            logger.LogInformation(
                new EventId(LogEvents.StockValidationPerformed, nameof(LogEvents.StockValidationPerformed)),
                "Stock validation performed for {SKU} with quantity {StockQuantity}",
                request.SKU,
                request.StockQuantity);

            // DATABASE SAVE
            dbStopwatch.Start();

            logger.LogInformation(
                new EventId(LogEvents.DatabaseOperationStarted, nameof(LogEvents.DatabaseOperationStarted)),
                "Starting database operation for SKU {SKU}", request.SKU);

            var product = mapper.Map<Product>(request);

            context.Products.Add(product);
            await context.SaveChangesAsync(cancellationToken);

            dbStopwatch.Stop();

            logger.LogInformation(
                new EventId(LogEvents.DatabaseOperationCompleted, nameof(LogEvents.DatabaseOperationCompleted)),
                "Database operation completed for ProductId {ProductId}", product.Id);

            // CACHE INVALIDATION
            cache.Remove("all_products");

            logger.LogInformation(
                new EventId(LogEvents.CacheOperationPerformed, nameof(LogEvents.CacheOperationPerformed)),
                "Cache invalidated for key {CacheKey}", "all_products");

            // METRICS SUCCESS
            totalStopwatch.Stop();

            var successMetrics = new ProductCreationMetrics(
                operationId,
                product.Name,
                product.SKU,
                product.Category,
                validationStopwatch.Elapsed,
                dbStopwatch.Elapsed,
                totalStopwatch.Elapsed,
                true,
                null);

            logger.LogProductCreationMetrics(successMetrics);

            logger.LogInformation(
                new EventId(LogEvents.ProductCreationCompleted, nameof(LogEvents.ProductCreationCompleted)),
                "Product creation completed successfully for ProductId {ProductId}", product.Id);

            var dto = mapper.Map<ProductProfileDto>(product);
            return Results.Created($"/products/{product.Id}", dto);
        }
        catch (Exception ex) when (ex is FluentValidation.ValidationException or DbUpdateException or InvalidOperationException)
        {
            totalStopwatch.Stop();

            var errorMetrics = new ProductCreationMetrics(
                operationId,
                request.Name,
                request.SKU,
                request.Category,
                validationStopwatch.Elapsed,
                dbStopwatch.Elapsed,
                totalStopwatch.Elapsed,
                false,
                ex.Message);

            logger.LogProductCreationMetrics(errorMetrics);

            logger.LogError(
                new EventId(LogEvents.ProductValidationFailed, nameof(LogEvents.ProductValidationFailed)),
                ex,
                "Error during product creation for SKU {SKU}", request.SKU);

            throw;
        }
    }
}
