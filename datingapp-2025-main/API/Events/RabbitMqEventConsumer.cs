using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace API.Events;

/// <summary>
/// Implémentation RabbitMQ du consommateur d'événements
/// </summary>
public class RabbitMqEventConsumer : IEventConsumer, IDisposable
{
    private readonly ILogger<RabbitMqEventConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqEventConsumer(ILogger<RabbitMqEventConsumer> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        InitializeRabbitMq();
    }

    private void InitializeRabbitMq()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
                VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _logger.LogInformation("✅ RabbitMQ Consumer connected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize RabbitMQ consumer");
        }
    }

    public Task ConsumeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken)
    {
        if (_channel == null)
        {
            _logger.LogError("❌ RabbitMQ channel not initialized");
            return Task.CompletedTask;
        }

        try
        {
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var @event = JsonSerializer.Deserialize<T>(message);

                    if (@event != null)
                    {
                        _logger.LogInformation("📩 Received event from {Queue}: {Event}", queueName, message);
                        await handler(@event);
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing event from {Queue}", queueName);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queueName, autoAck: false, consumer);

            _logger.LogInformation("✅ Started consuming from queue: {Queue}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to consume from queue: {Queue}", queueName);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

