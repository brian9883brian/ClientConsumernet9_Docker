using MassTransit;
using ClientConsumerOrder.Events;
using ClientConsumerOrder.Models;
using ClientConsumerOrder.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ClientConsumerOrder.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IHubContext<NotificationHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        try
        {
            var orderEvent = context.Message;

            _logger.LogInformation("🎉 ORDEN CREADA - OrderID: {OrderId}", orderEvent.OrderId);
            _logger.LogInformation("   👤 Cliente: {UserName}, Total: {TotalPrice:C}",
                orderEvent.UserName, orderEvent.TotalPrice);

            // Convertir a DTO y almacenar
            var orderDto = new OrderDto
            {
                OrderId = orderEvent.OrderId,
                UserName = orderEvent.UserName,
                TotalPrice = orderEvent.TotalPrice,
                FirstName = orderEvent.FirstName,
                LastName = orderEvent.LastName,
                EmailAddress = orderEvent.EmailAddress,
                AddressLine = orderEvent.AddressLine,
                Country = orderEvent.Country,
                State = orderEvent.State,
                ZipCode = orderEvent.ZipCode,
                CardName = orderEvent.CardName,
                PaymentMethod = orderEvent.PaymentMethod,
                Status = "Created",
                OrderDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            OrderStorage.AddOrder(orderDto);

            // Notificar a Vue via SignalR
            await _hubContext.Clients.All.SendAsync("OrderCreated",
                orderEvent.OrderId.ToString(),
                orderEvent.UserName,
                orderEvent.TotalPrice);

            _logger.LogInformation("✅ Orden {OrderId} procesada exitosamente", orderEvent.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error en OrderCreatedConsumer");
            throw;
        }
    }
}