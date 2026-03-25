using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.Dtos;
using OrderManagementAPI.Entities;

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

        public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDTO)
        {
            var correlationId= _correlationIdAccessor.GetCorrelationId();
            _logger.LogInformation($"[{correlationId}] Creating order for CustomerId {createOrderDTO.CustomerId} with {createOrderDTO.CreateOrderItemDTOs?.Count ?? 0} items.");
            var customer = await _dbContext.Customers.FindAsync(createOrderDTO.CustomerId);
            if( customer == null )
            {
                _logger.LogWarning($"[{correlationId}] cannot create order:CustomerId {createOrderDTO.CustomerId} not found.");
                throw new ArgumentException($"Customer with id {createOrderDTO.CustomerId} not found.");
            }
            if(createOrderDTO.CreateOrderItemDTOs==null || createOrderDTO.CreateOrderItemDTOs.Count == 0)
            {

            }
            return "";
        }

        public async Task<OrderDTO> GetOrderByIdAsync(int id)
        {
            var correlationId = _correlationIdAccessor.GetCorrelationId();
            _logger.LogInformation($"[{correlationId}] Fetching order with OrderId {id}.");
            var order = await _dbContext.Orders
                                        .Include(o => o.CustomerId)
                                        .Include(o => o.OrderItems)
                                        .ThenInclude(i => i.Product)
                                        .AsNoTracking()
                                        .SingleOrDefaultAsync(o=>o.Id==id);
            if(order == null)
            {
                _logger.LogWarning($"[{correlationId}] Order with OrderId {id} not found.");
                return null;
            }
            _logger.LogDebug($"[{correlationId}] Order {order.Id} found for CustomerId {order.CustomerId}.");
            return MapToOrderDto(order);
            
            
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrderDTO>> GetOrdersForCustomerAsync(int customerId)
        {
            throw new NotImplementedException();
        }
        private static OrderDTO MapToOrderDto(Order order)
        {
            return new OrderDTO
            {
                Id = order.Id,
                //CustomerId=order.CustomerId,
                //CustomerName = order.Customer?.FullName ?? string.Empty,
                OrderDate = order.OrderDate,
                TotalAmount= order.TotalAmount,
                Status = order.Status,
                Items=order.OrderItems.Select(i=>new OrderItemDTO
                {
                    Id=i.Id,
                    ProductId=i.ProductId,
                    ProductName=i.Product?.Name??string.Empty,
                    Quantity=i.Quantity,
                    UnitPrice=i.UnitPrice,
                    LineTotal=i.LineTotal
                }).ToList()
            };
        }
    }
}
