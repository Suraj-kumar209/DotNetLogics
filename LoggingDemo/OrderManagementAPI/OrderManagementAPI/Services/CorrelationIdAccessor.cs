namespace OrderManagementAPI.Services
{
    public class CorrelationIdAccessor : ICorrelationIdAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CorrelationIdItemKey = "X-Correlation-ID";
        public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string? GetCorrelationId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }
            if (httpContext.Items.TryGetValue(CorrelationIdItemKey, out var value) && value is string cidFromItems)
            {
                return cidFromItems;
            }
            return httpContext.TraceIdentifier;
        }
    }
}
