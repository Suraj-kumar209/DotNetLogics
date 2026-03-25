using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderManagementAPI.Services;

namespace OrderManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IOrderService _orderService;
        private readonly ICorrelationIdAccessor _correlationIdAccessor;
        public OrdersController(ILogger<OrdersController> logger,IOrderService orderService,ICorrelationIdAccessor correlationIdAccessor)
        {
            _correlationIdAccessor=correlationIdAccessor;
            _logger = logger;
            _orderService = orderService;
        }
    }
}
