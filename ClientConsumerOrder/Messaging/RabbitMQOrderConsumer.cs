using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using ClientConsumerOrder.Services;
using ClientConsumerOrder.Models;

namespace Ordering.Infraestructure.EventMessage
{
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly OrderStorageService _storageService;
        private IChannel _channel;

        private readonly Dictionary<string, string> _exchanges = new();
        private readonly Dictionary<string, string> _queues = new();

        public RabbitMQOrderConsumer(IConfiguration configuration, IConnection connection, OrderStorageService storageService)
        {
            _configuration = configuration;
            _connection = connection;
            _storageService = storageService;

            _exchanges["create"] = _configuration["TopicAndQueueNames:OrderCreatedTopic"] ?? "order_created_exchange";
            _exchanges["update"] = _configuration["TopicAndQueueNames:OrderUpdatedTopic"] ?? "order_updated_exchange";
            _exchanges["delete"] = _configuration["TopicAndQueueNames:OrderDeletedTopic"] ?? "order_deleted_exchange";
            _exchanges["read"] = _configuration["TopicAndQueueNames:OrderReadTopic"] ?? "order_read_exchange";

            _queues["create"] = _configuration["TopicAndQueueNames:OrderCreatedQueue"] ?? "order_created_queue_clientconsumer";
            _queues["update"] = _configuration["TopicAndQueueNames:OrderUpdatedQueue"] ?? "order_updated_queue_clientconsumer";
            _queues["delete"] = _configuration["TopicAndQueueNames:OrderDeletedQueue"] ?? "order_deleted_queue_clientconsumer";
            _queues["read"] = _configuration["TopicAndQueueNames:OrderReadQueue"] ?? "order_read_queue_clientconsumer";
        }

        private async Task ConfigureRabbitMQ()
        {
            _channel = await _connection.CreateChannelAsync();

            foreach (var key in _exchanges.Keys)
            {
                var exchange = _exchanges[key];
                var queue = _queues[key];

                await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: true);
                await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                await _channel.QueueBindAsync(queue, exchange, string.Empty);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConfigureRabbitMQ();

            foreach (var kvp in _queues)
            {
                var queueName = kvp.Value;
                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (sender, args) =>
                {
                    var json = Encoding.UTF8.GetString(args.Body.ToArray());

                    // Manejar comandos de delete primero
                    if (queueName.Contains("deleted"))
                    {
                        try
                        {
                            var deleteCommand = JsonConvert.DeserializeObject<dynamic>(json);
                            if (deleteCommand?.OrderId != null)
                            {
                                var orderId = Guid.Parse(deleteCommand.OrderId.ToString());
                                _storageService.Remove(orderId);
                                Console.WriteLine($"[STORAGE] Removed order by command: {orderId}");
                                return;
                            }
                        }
                        catch
                        {
                            // Continuar con procesamiento normal si falla
                        }
                    }

                    // Procesamiento normal para otros mensajes
                    var message = JsonConvert.DeserializeObject<OrderMessage>(json);
                    Console.WriteLine($"[QUEUE: {queueName}] → Order Event received: {message?.EmailAddress}");

                    if (message != null)
                    {
                        if (queueName.Contains("deleted"))
                        {
                            _storageService.Remove(message.Id);
                            Console.WriteLine($"[STORAGE] Removed order: {message.Id}");
                        }
                        else
                        {
                            _storageService.AddOrUpdate(message);
                            Console.WriteLine($"[STORAGE] Updated order: {message.Id}");
                        }
                    }
                };

                await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null)
                await _channel.CloseAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}