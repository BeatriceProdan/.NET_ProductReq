using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ProductsManagement.Common.Middleware
{
    public class CorrelationMiddleware
    {
        private const string HeaderName = "X-Correlation-ID";
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationMiddleware> _logger;

        public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId;

            // Dacă clientul trimite deja X-Correlation-ID, îl păstrăm
            if (context.Request.Headers.TryGetValue(HeaderName, out var existing))
            {
                correlationId = existing!;
            }
            else
            {
                // Dacă nu, generăm unul nou
                correlationId = Guid.NewGuid().ToString()[..8];
                context.Request.Headers.Append(HeaderName, correlationId);
            }

            // Adăugăm ID în răspuns
            context.Response.Headers.Append(HeaderName, correlationId);

            using (_logger.BeginScope("{CorrelationId}", correlationId))
            {
                await _next(context);
            }
        }
    }

    public static class CorrelationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationMiddleware>();
        }
    }
}
