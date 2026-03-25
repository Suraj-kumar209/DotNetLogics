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
        public OrderService(OrderManagementDBContext dbContext,ILogger<OrderService> logger,ICorrelationIdAccessor correlationIdAccessor)
        {
            _dbContext = dbContext;
            _logger = logger;
            _correlationIdAccessor = correlationIdAccessor;
        }
        public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO dto)
        {
            var correlationId = _correlationIdAccessor.GetCorrelationId();
            _logger.LogInformation($"[{correlationId}] Creating order for CustomerId {dto.CustomerId} with {dto.CreateOrderItemDTOs?.Count ?? 0} items.");
            var customer = await _dbContext.Customers.FindAsync(dto.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning($"[{correlationId}] Cannot create order: CustomerId {dto.CustomerId} not found.");
                throw new ArgumentException($"Customer with id {dto.CustomerId} not found.");
            }
            if (dto.CreateOrderItemDTOs == null || dto.CreateOrderItemDTOs.Count == 0)
            {
                _logger.LogWarning($"[{correlationId}] Cannot create order: no items provided for CustomerId {dto.CustomerId}.");
                throw new ArgumentException("Order must contain at least one item.");
            }
            var productIds = dto.CreateOrderItemDTOs.Select(i => i.ProductId).Distinct().ToList();
            var products = await _dbContext.Products.Where(p => productIds.Contains(p.Id) && p.IsActive).ToListAsync();
            if (products.Count != productIds.Count)
            {
                var missingIds = productIds.Except(products.Select(p => p.Id)).ToList();
                var missingIdsString = string.Join(", ", missingIds);
                _logger.LogWarning($"[{correlationId}] Cannot create order: some products not found or inactive. Missing IDs: {missingIdsString}.");
                throw new ArgumentException("One or more products are invalid or not active.");
            }

            var order = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.UtcNow
            };
            decimal total = 0;
            foreach (var itemDto in dto.CreateOrderItemDTOs)
            {
                var product = products.Single(p => p.Id == itemDto.ProductId);
                var unitPrice = product.Price;
                var lineTotal = unitPrice * itemDto.Quantity;
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal
                };
                order.OrderItems.Add(orderItem);
                total += lineTotal;
                _logger.LogDebug($"[{correlationId}] Added item: ProductId={product.Id}, Quantity={itemDto.Quantity}, UnitPrice={unitPrice}, LineTotal={lineTotal}.");
            }
            order.TotalAmount = total;
            _logger.LogDebug($"[{correlationId}] Total amount for CustomerId {dto.CustomerId} calculated as {order.TotalAmount}.");

            try
            {
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"[{correlationId}] Order {order.Id} created successfully for CustomerId {dto.CustomerId}.");
                await _dbContext.Entry(order).Reference(o => o.Customer).LoadAsync();
                await _dbContext.Entry(order).Collection(o => o.OrderItems).LoadAsync();
                foreach (var item in order.OrderItems)
                {
                    await _dbContext.Entry(item).Reference(i => i.Product).LoadAsync();
                }
                return MapToOrderDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{correlationId}] Error occurred while saving order for CustomerId {dto.CustomerId}.");
                throw;
            }
        }
        public async Task<OrderDTO?> GetOrderByIdAsync(int id)
        {
            var correlationId = _correlationIdAccessor.GetCorrelationId();
            _logger.LogInformation($"[{correlationId}] Fetching order with OrderId {id}.");
            var order = await _dbContext.Orders.Include(o => o.Customer)
                                               .Include(o => o.OrderItems)
                                               .ThenInclude(i => i.Product)
                                               .AsNoTracking().SingleOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                _logger.LogWarning($"[{correlationId}] Order with OrderId {id} not found.");
                return null;
            }
            _logger.LogDebug($"[{correlationId}] Order {order.Id} found for CustomerId {order.CustomerId}.");
            return MapToOrderDto(order);
        }
        public async Task<IEnumerable<OrderDTO>> GetOrdersForCustomerAsync(int customerId)
        {
            var correlationId = _correlationIdAccessor.GetCorrelationId();
            _logger.LogInformation($"[{correlationId}] Fetching orders for CustomerId {customerId}.");
            var orders = await _dbContext.Orders.Include(o => o.Customer)
                                                .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                                                .AsNoTracking().Where(o => o.CustomerId == customerId).ToListAsync();
            _logger.LogInformation($"[{correlationId}] Found {orders.Count} orders for CustomerId {customerId}.");
            return orders.Select(MapToOrderDto);
        }

        private static OrderDTO MapToOrderDto(Order order)
        {
            return new OrderDTO
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName ?? string.Empty,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Items = order.OrderItems.Select(i => new OrderItemDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? string.Empty,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToList()
            };
        }
    }
}

