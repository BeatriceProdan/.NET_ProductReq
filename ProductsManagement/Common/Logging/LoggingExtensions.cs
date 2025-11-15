using Microsoft.Extensions.Logging;

namespace ProductsManagement.Common.Logging
{
    public static class LoggingExtensions
    {
        public static void LogProductCreationMetrics(
            this ILogger logger,
            ProductCreationMetrics metrics)
        {
            var eventId = new EventId(LogEvents.ProductCreationCompleted, "ProductCreationCompleted");

            logger.LogInformation(eventId,
                "Product creation metrics | OperationId={OperationId} | Name={ProductName} | SKU={SKU} | Category={Category} | " +
                "Validation={ValidationDuration}ms | DB={DatabaseSaveDuration}ms | Total={TotalDuration}ms | Success={Success} | Error={ErrorReason}",
                metrics.OperationId,
                metrics.ProductName,
                metrics.SKU,
                metrics.Category,
                metrics.ValidationDuration.TotalMilliseconds,
                metrics.DatabaseSaveDuration.TotalMilliseconds,
                metrics.TotalDuration.TotalMilliseconds,
                metrics.Success,
                metrics.ErrorReason ?? "None");
        }
    }
}
