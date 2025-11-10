using ClientConsumerOrder.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ClientConsumerOrder.Services
{
    public class RabbitMQPublisherService : IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly ILogger<RabbitMQPublisherService> _logger;

        // Cambiar: Recibir la conexión por inyección en lugar de crearla
        public RabbitMQPublisherService(IConnection connection, ILogger<RabbitMQPublisherService> logger)
        {
            _connection = connection;
            _logger = logger;

            // Usar la conexión compartida para crear el canal
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            string[] exchanges = {
                "order_created_exchange",
                "order_updated_exchange",
                "order_deleted_exchange",
                "order_read_exchange",
                "order_commands_exchange"
            };

            foreach (var exchange in exchanges)
            {
                _channel.ExchangeDeclareAsync(exchange: exchange, type: ExchangeType.Fanout, durable: true)
                    .GetAwaiter().GetResult();
            }

            _logger.LogInformation("RabbitMQ Publisher Service initialized successfully");
        }

        // El resto del código igual...
        public async Task PublishAsync<T>(T message, string queueName) where T : class
        {
            try
            {
                var exchangeName = queueName.Replace("_queue", "_exchange");

                if (queueName == "order_commands_exchange")
                {
                    exchangeName = "order_commands_exchange";
                }

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                await _channel.BasicPublishAsync(exchange: exchangeName, routingKey: string.Empty, body: body);
                _logger.LogInformation("[{Exchange}] Message published: {MessageType}", exchangeName, typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to exchange for queue: {QueueName}", queueName);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_channel != null)
                    await _channel.CloseAsync();
                // NO cerrar la conexión aquí, se cierra cuando la aplicación termine
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ resources");
            }
        }
    }
}