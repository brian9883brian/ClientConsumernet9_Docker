using ClientConsumerOrder.Events;
using ClientConsumerOrder.Hubs;
using ClientConsumerOrder.Models;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace ClientConsumerOrder.Consumers
{

    public class OrderDeletedConsumer : IConsumer<OrderDeletedEvent>
    {
        private readonly ILogger<OrderDeletedConsumer> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public OrderDeletedConsumer(ILogger<OrderDeletedConsumer> logger, IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<OrderDeletedEvent> context)
        {
            var orderEvent = context.Message;

            _logger.LogInformation("🗑️ ORDEN ELIMINADA - OrderID: {OrderId}", orderEvent.OrderId);
            _logger.LogInformation("   📝 Razón: {Reason}, Por: {DeletedBy}",
                orderEvent.Reason, orderEvent.DeletedBy);

            // Eliminar del almacenamiento
            var deleted = OrderStorage.DeleteOrder(orderEvent.OrderId);

            if (deleted)
            {
                // Notificar a Vue
                await _hubContext.Clients.All.SendAsync("OrderDeleted",
                    orderEvent.OrderId,
                    orderEvent.Reason);

                _logger.LogInformation("✅ Orden {OrderId} eliminada del sistema", orderEvent.OrderId);
            }
            else
            {
                _logger.LogWarning("⚠️ Orden {OrderId} no encontrada para eliminar", orderEvent.OrderId);
            }
        }
    }
}
