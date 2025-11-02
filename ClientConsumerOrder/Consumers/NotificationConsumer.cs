using ClientConsumerOrder.Events;
using ClientConsumerOrder.Services;
using MassTransit;

namespace ClientConsumerOrder.Consumers
{

    public class NotificationConsumer :
        IConsumer<OrderCreatedEvent>,
        IConsumer<OrderUpdatedEvent>,
        IConsumer<OrderDeletedEvent>
    {
        private readonly ILogger<NotificationConsumer> _logger;
        private readonly IEmailNotificationService _emailService;

        public NotificationConsumer(ILogger<NotificationConsumer> logger, IEmailNotificationService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var orderEvent = context.Message;
            _logger.LogInformation("📧 [NOTIFICATION] Orden creada notificada - {OrderId}", orderEvent.OrderId);
            await _emailService.SendOrderConfirmationAsync(orderEvent);
        }

        public async Task Consume(ConsumeContext<OrderUpdatedEvent> context)
        {
            var orderEvent = context.Message;
            _logger.LogInformation("📧 [NOTIFICATION] Orden actualizada notificada - {OrderId}", orderEvent.OrderId);
            await _emailService.SendOrderUpdatedAsync(orderEvent);
        }

        public async Task Consume(ConsumeContext<OrderDeletedEvent> context)
        {
            var orderEvent = context.Message;
            _logger.LogInformation("📧 [NOTIFICATION] Orden eliminada notificada - {OrderId}", orderEvent.OrderId);
            await _emailService.SendOrderCancelledAsync(orderEvent);
        }
    }
}
