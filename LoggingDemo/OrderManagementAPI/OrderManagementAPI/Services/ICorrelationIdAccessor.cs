namespace OrderManagementAPI.Services
{
    public interface ICorrelationIdAccessor
    {
        string? GetCorrelationId();
    }
}