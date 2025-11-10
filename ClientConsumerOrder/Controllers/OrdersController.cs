using Microsoft.AspNetCore.Mvc;
using ClientConsumerOrder.Models;
using ClientConsumerOrder.Services;

namespace ClientConsumerOrder.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderStorageService _storage;
        private readonly RabbitMQPublisherService _publisher;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(OrderStorageService storage, RabbitMQPublisherService publisher, ILogger<OrdersController> logger)
        {
            _storage = storage;
            _publisher = publisher;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<OrderMessage>> GetAll()
        {
            return Ok(_storage.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<OrderMessage> GetById(Guid id)
        {
            var order = _storage.GetById(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderMessage order)
        {
            // Generar nuevo GUID en el backend (ignorar si viene en el request)
            order.Id = Guid.NewGuid();

            _storage.AddOrUpdate(order);
            await _publisher.PublishAsync(order, "order_created_queue");
            return Ok(order.Id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderMessage order)
        {
            order.Id = id;
            _storage.AddOrUpdate(order);
            await _publisher.PublishAsync(order, "order_updated_queue");
            return NoContent();
        }

        // En Client Consumer - OrdersController.cs (solo el DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Crear comando de eliminación
                var deleteCommand = new
                {
                    Action = "delete",
                    OrderId = id,
                    Timestamp = DateTime.UtcNow
                };

                // Publicar a exchange de comandos (no a la cola de eventos)
                await _publisher.PublishAsync(deleteCommand, "order_commands_exchange");

                _storage.Remove(id); // Eliminar localmente
                _logger.LogInformation("Delete command sent for order: {OrderId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending delete command for order: {OrderId}", id);
                return StatusCode(500, "Error sending delete command");
            }
        }
    }
}