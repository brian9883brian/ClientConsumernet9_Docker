using ClientConsumerOrder.Events;

namespace ClientConsumerOrder.Services
{

    public interface IEmailNotificationService
    {
        Task SendOrderConfirmationAsync(OrderCreatedEvent orderEvent);
        Task SendOrderUpdatedAsync(OrderUpdatedEvent orderEvent);
        Task SendOrderCancelledAsync(OrderDeletedEvent orderEvent);
    }

    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(ILogger<EmailNotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendOrderConfirmationAsync(OrderCreatedEvent orderEvent)
        {
            _logger.LogInformation("   ✉️  [EMAIL] Enviando confirmación para orden {OrderId}...", orderEvent.OrderId);
            await Task.Delay(100); // Simular envío de email
            _logger.LogInformation("   ✅ [EMAIL] Confirmación enviada a {Email}", orderEvent.EmailAddress);
        }

        public async Task SendOrderUpdatedAsync(OrderUpdatedEvent orderEvent)
        {
            _logger.LogInformation("   ✉️  [EMAIL] Enviando actualización para orden {OrderId}...", orderEvent.OrderId);
            await Task.Delay(100);
            _logger.LogInformation("   ✅ [EMAIL] Actualización enviada");
        }

        public async Task SendOrderCancelledAsync(OrderDeletedEvent orderEvent)
        {
            _logger.LogInformation("   ✉️  [EMAIL] Enviando cancelación para orden {OrderId}...", orderEvent.OrderId);
            await Task.Delay(100);
            _logger.LogInformation("   ✅ [EMAIL] Cancelación enviada");
        }
    }
}
