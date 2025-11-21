using Microsoft.Extensions.Logging;

namespace ProductsManagement.Common.Logging;

public static class LoggingExtensions
{
    public static void LogProductCreationMetrics(this ILogger logger, ProductCreationMetrics metrics)
    {
        logger.LogInformation(
            new EventId(LogEvents.ProductCreationCompleted, nameof(LogEvents.ProductCreationCompleted)),
            "Product creation metrics for {OperationId}. Name: {ProductName}, SKU: {SKU}, Category: {Category}, " +
            "Validation: {ValidationDurationMs} ms, DbSave: {DatabaseSaveDurationMs} ms, Total: {TotalDurationMs} ms, " +
            "Success: {Success}, Error: {ErrorReason}",
            metrics.OperationId,
            metrics.ProductName,
            metrics.SKU,
            metrics.Category.ToString(),
            metrics.ValidationDuration.TotalMilliseconds,
            metrics.DatabaseSaveDuration.TotalMilliseconds,
            metrics.TotalDuration.TotalMilliseconds,
            metrics.Success,
            metrics.ErrorReason ?? string.Empty
        );
    }
}
