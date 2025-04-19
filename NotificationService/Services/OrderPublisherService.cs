using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using NotificationService.Configuration;

namespace NotificationService.Services;

public interface IOrderPublisherService
{
    void PublishOrder(string orderId);
}

public class OrderPublisherService : IOrderPublisherService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly ILogger<OrderPublisherService> _logger;

    public OrderPublisherService(
        IOptions<RabbitMQConfig> config,
        ILogger<OrderPublisherService> logger)
    {
        _logger = logger;
        _queueName = config.Value.QueueName;

        var factory = new ConnectionFactory
        {
            HostName = config.Value.HostName,
            UserName = config.Value.UserName,
            Password = config.Value.Password
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Ensure queue exists
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("RabbitMQ publisher initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ publisher");
            throw;
        }
    }

    public void PublishOrder(string orderId)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(orderId);
            
            _channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Published order: {OrderId}", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish order: {OrderId}", orderId);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
} 