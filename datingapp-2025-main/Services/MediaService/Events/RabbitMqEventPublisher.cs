using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MediaService.Events;

public interface IEventPublisher
{
    Task PublishAsync<T>(string eventName, T eventData) where T : class;
}

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(IConfiguration configuration, ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/"
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _logger.LogInformation("✅ RabbitMQ EventPublisher initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public Task PublishAsync<T>(string eventName, T eventData) where T : class
    {
        try
        {
            // Publication directe sur la queue "eventName" (exchange par défaut "").
            // Core API déclare cette même queue (durable) avant de la consommer :
            // API/Events/RabbitMqEventConsumer.cs.
            _channel.QueueDeclare(eventName, durable: true, exclusive: false, autoDelete: false);

            var message = JsonSerializer.Serialize(eventData);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: "",
                routingKey: eventName,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation("📨 Event published: {EventName}", eventName);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to publish event: {EventName}", eventName);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}


