using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using OrderService.Models;

namespace OrderService.Services;

public interface IRabbitMQService
{
    void PublishOrderPlaced(Order order);
}

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMQService> _logger;
    private const string QueueName = "order_placed";

    public RabbitMQService(ILogger<RabbitMQService> logger)
    {
        _logger = logger;
        _factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };
    }

    public void PublishOrderPlaced(Order order)
    {
        try
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            channel.QueueDeclare(queue: QueueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            var message = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                               routingKey: QueueName,
                               basicProperties: null,
                               body: body);
            
            _logger.LogInformation("Published order {OrderId} to queue {QueueName}", order.Id, QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish order {OrderId} to RabbitMQ", order.Id);
            throw;
        }
    }

    public void Dispose()
    {
        // No need to dispose anything as we're using 'using' statements
    }
} 