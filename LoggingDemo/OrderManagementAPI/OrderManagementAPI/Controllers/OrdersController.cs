using Microsoft.AspNetCore.Mvc;
using OrderManagementAPI.Dtos;
using OrderManagementAPI.Services;
using System.Diagnostics;
using System.Text.Json;

namespace OrderManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IOrderService _orderService;
        private readonly ICorrelationIdAccessor _correlationIdAccessor;
        public OrdersController(ILogger<OrdersController> logger,
                    IOrderService orderService,
                    ICorrelationIdAccessor correlationIdAccessor)
        {
            _logger = logger;
            _orderService = orderService;
            _correlationIdAccessor = correlationIdAccessor;
            _logger.LogInformation("OrdersController instantiated.");
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO dto)
        {
            var stopwatch = Stopwatch.StartNew();

            var correlationId = _correlationIdAccessor.GetCorrelationId();


            var requestJson = dto is null
                ? "null"
                : JsonSerializer.Serialize(dto, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            _logger.LogInformation($"[{correlationId}] HTTP POST /api/orders called. Payload: {requestJson}");


            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"[{correlationId}] Model validation failed for CreateOrder request.");
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _orderService.CreateOrderAsync(dto!);
                stopwatch.Stop();


                var responseJson = JsonSerializer.Serialize(created, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                _logger.LogInformation(
                    $"[{correlationId}] Order {created.Id} created successfully via API in {stopwatch.ElapsedMilliseconds} ms. Response: {responseJson}");

                return CreatedAtAction(nameof(GetOrderById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning($"[{correlationId}] Validation error while creating order: {ex.Message}. Time taken: {stopwatch.ElapsedMilliseconds} ms.");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError($"[{correlationId}] Unexpected error while creating order: {ex.Message}. Time taken: {stopwatch.ElapsedMilliseconds} ms.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = _correlationIdAccessor.GetCorrelationId();

            _logger.LogInformation($"[{correlationId}] HTTP GET /api/orders/{id} called.");

            var order = await _orderService.GetOrderByIdAsync(id);
            stopwatch.Stop();

            if (order == null)
            {
                _logger.LogWarning($"[{correlationId}] Order with ID {id} not found. Execution time: {stopwatch.ElapsedMilliseconds} ms.");
                return NotFound(new { message = $"Order with id {id} not found." });
            }


            var responseJson = JsonSerializer.Serialize(order, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            _logger.LogInformation($"[{correlationId}] Order {id} fetched successfully. Execution time: {stopwatch.ElapsedMilliseconds} ms. Response: {responseJson}");
            return Ok(order);
        }


        [HttpGet("customer/{customerId:int}")]
        public async Task<IActionResult> GetOrdersForCustomer(int customerId)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = _correlationIdAccessor.GetCorrelationId();

            _logger.LogInformation($"[{correlationId}] HTTP GET /api/orders/customer/{customerId} called.");

            var orders = await _orderService.GetOrdersForCustomerAsync(customerId);
            stopwatch.Stop();

            _logger.LogInformation($"[{correlationId}] {orders.Count()} orders returned for CustomerId {customerId} in {stopwatch.ElapsedMilliseconds} ms.");
            return Ok(orders);
        }
    }
}