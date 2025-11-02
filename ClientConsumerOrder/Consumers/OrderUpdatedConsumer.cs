using ClientConsumerOrder.Events;
using ClientConsumerOrder.Hubs;
using ClientConsumerOrder.Models;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace ClientConsumerOrder.Consumers
{

    public class OrderUpdatedConsumer : IConsumer<OrderUpdatedEvent>
    {
        private readonly ILogger<OrderUpdatedConsumer> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public OrderUpdatedConsumer(ILogger<OrderUpdatedConsumer> logger, IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<OrderUpdatedEvent> context)
        {
            var orderEvent = context.Message;

            _logger.LogInformation("📝 ORDEN ACTUALIZADA - OrderID: {OrderId}", orderEvent.OrderId);
            _logger.LogInformation("   🔄 Estado: {Status}, Total: {TotalPrice:C}",
                orderEvent.Status, orderEvent.TotalPrice);

            // Buscar orden existente
            var existingOrder = OrderStorage.GetOrder(orderEvent.OrderId);
            if (existingOrder != null)
            {
                // Actualizar campos modificados
                var updatedOrder = new OrderDto
                {
                    OrderId = orderEvent.OrderId,
                    UserName = orderEvent.UserName,
                    TotalPrice = orderEvent.TotalPrice,
                    Status = orderEvent.Status,
                    LastUpdated = DateTime.UtcNow,

                    // Mantener datos existentes o actualizar si vienen en el evento
                    FirstName = orderEvent.FirstName ?? existingOrder.FirstName,
                    LastName = orderEvent.LastName ?? existingOrder.LastName,
                    EmailAddress = orderEvent.EmailAddress ?? existingOrder.EmailAddress,
                    AddressLine = orderEvent.AddressLine ?? existingOrder.AddressLine,
                    CardName = existingOrder.CardName,
                    PaymentMethod = existingOrder.PaymentMethod,
                    Country = existingOrder.Country,
                    State = existingOrder.State,
                    ZipCode = existingOrder.ZipCode,
                    OrderDate = existingOrder.OrderDate
                };

                OrderStorage.UpdateOrder(updatedOrder);

                // Notificar a Vue
                await _hubContext.Clients.All.SendAsync("OrderUpdated",
                    orderEvent.OrderId,
                    orderEvent.Status,
                    orderEvent.TotalPrice);

                _logger.LogInformation("✅ Orden {OrderId} actualizada en sistema", orderEvent.OrderId);
            }
            else
            {
                _logger.LogWarning("⚠️ Orden {OrderId} no encontrada para actualizar", orderEvent.OrderId);
            }
        }
    }
}
