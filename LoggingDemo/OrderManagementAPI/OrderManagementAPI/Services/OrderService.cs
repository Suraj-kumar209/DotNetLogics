using OrderManagementAPI.Data;
using OrderManagementAPI.Dtos;

namespace OrderManagementAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderManagementDBContext _dbContext;
        private readonly ILogger<OrderService> _logger;
        private readonly ICorrelationIdAccessor _correlationIdAccessor;
        public OrderService(
            OrderManagementDBContext dbContext,
            ILogger<OrderService> logger,
            ICorrelationIdAccessor correlationIdAccessor)
        {
            _dbContext = dbContext;
            _logger = logger;
            _correlationIdAccessor = correlationIdAccessor;
        }

        public Task<OrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDTO)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDTO> GetOrderByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrderDTO>> GetOrdersForCustomerAsync(int customerId)
        {
            throw new NotImplementedException();
        }
    }
}
