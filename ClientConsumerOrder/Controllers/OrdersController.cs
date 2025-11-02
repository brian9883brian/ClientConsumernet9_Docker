using ClientConsumerOrder.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClientConsumerOrder.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ILogger<OrdersController> logger)
        {
            _logger = logger;
        }

        // ✅ **SOLO GET** - El Client Consumer solo LEE lo que recibe de RabbitMQ

        // GET: api/orders - Todas las órdenes recibidas
        [HttpGet]
        public IActionResult GetAllOrders()
        {
            try
            {
                var orders = OrderStorage.Orders;
                _logger.LogInformation("📋 GET All - {Count} órdenes en memoria", orders.Count);

                return Ok(new
                {
                    success = true,
                    count = orders.Count,
                    data = orders,
                    source = "RabbitMQ Events",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener órdenes");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        // GET: api/orders/{id} - Orden específica
        [HttpGet("{id}")]
        public IActionResult GetOrderById(Guid id)
        {
            try
            {
                var order = OrderStorage.GetOrder(id);
                if (order == null)
                {
                    _logger.LogWarning("❌ Orden {OrderId} no encontrada en memoria", id);
                    return NotFound(new
                    {
                        success = false,
                        message = $"Orden no encontrada. Puede que no haya sido procesada aún."
                    });
                }

                _logger.LogInformation("✅ Orden {OrderId} encontrada", id);
                return Ok(new
                {
                    success = true,
                    data = order,
                    source = "RabbitMQ Event"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener orden {OrderId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        // GET: api/orders/count - Contador
        [HttpGet("count")]
        public IActionResult GetOrdersCount()
        {
            try
            {
                var count = OrderStorage.GetCount();
                _logger.LogInformation("🔢 Total de órdenes recibidas: {Count}", count);

                return Ok(new
                {
                    success = true,
                    count = count,
                    message = "Órdenes recibidas via RabbitMQ",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener contador");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        // GET: api/orders/recent/5 - Órdenes recientes
        [HttpGet("recent/{count?}")]
        public IActionResult GetRecentOrders(int count = 10)
        {
            try
            {
                var recentOrders = OrderStorage.GetRecentOrders(count);
                _logger.LogInformation("🕒 {Count} órdenes recientes", recentOrders.Count);

                return Ok(new
                {
                    success = true,
                    count = recentOrders.Count,
                    data = recentOrders,
                    source = "RabbitMQ Events"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener órdenes recientes");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        // GET: api/orders/stats - Estadísticas
        [HttpGet("stats")]
        public IActionResult GetOrdersStats()
        {
            try
            {
                var orders = OrderStorage.Orders;
                var stats = new
                {
                    TotalOrdersReceived = orders.Count,
                    TotalRevenue = orders.Sum(o => o.TotalPrice),
                    AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalPrice) : 0,
                    OrdersByStatus = orders.GroupBy(o => o.Status ?? "Unknown")
                                        .ToDictionary(g => g.Key, g => g.Count()),
                    RecentOrders = orders.Count(o => o.OrderDate > DateTime.UtcNow.AddHours(-1)),
                    LastOrderReceived = orders.Any() ? orders.Max(o => o.OrderDate) : (DateTime?)null
                };

                _logger.LogInformation("📊 Estadísticas de órdenes recibidas");

                return Ok(new
                {
                    success = true,
                    data = stats,
                    source = "RabbitMQ Events Consumption",
                    generatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar estadísticas");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        // GET: api/orders/health - Health check específico
        [HttpGet("health")]
        public IActionResult GetOrdersHealth()
        {
            try
            {
                var healthInfo = new
                {
                    Status = "Healthy",
                    Service = "Orders Consumer",
                    OrdersInMemory = OrderStorage.Orders.Count,
                    Storage = "In-Memory from RabbitMQ",
                    LastUpdate = OrderStorage.Orders.Any() ?
                        OrderStorage.Orders.Max(o => o.LastUpdated) : (DateTime?)null,
                    ConsumptionStatus = "Active"
                };

                return Ok(healthInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Health check falló");
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Error = ex.Message
                });
            }
        }
    }
}