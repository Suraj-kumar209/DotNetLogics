using OrderManagementAPI.Dtos;

namespace OrderManagementAPI.Services
{
    public interface IOrderService
    {
        Task<OrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDTO);
        Task<OrderDTO> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDTO>> GetOrdersForCustomerAsync(int customerId);
    }
}
