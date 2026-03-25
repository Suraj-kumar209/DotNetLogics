namespace OrderManagementAPI.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";
        public CorrelationIdMiddleware(RequestDelegate next)
        {
                _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext,ILogger<CorrelationIdMiddleware> logger)
        {
            var correlationId =Guid.NewGuid().ToString().ToUpper();
            httpContext.Items[CorrelationIdHeader] = correlationId; 
            httpContext.Response.Headers[CorrelationIdHeader]=correlationId;
            await _next(httpContext);
        }


    }
    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
