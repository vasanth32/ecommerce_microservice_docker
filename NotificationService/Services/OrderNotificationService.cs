using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificationService.Configuration;

namespace NotificationService.Services;

public class OrderNotificationService : BackgroundService
{
    private readonly ILogger<OrderNotificationService> _logger;
    private readonly RabbitMQConfig _config;
    private IConnection? _connection;
    private IModel? _channel;

    public OrderNotificationService(
        ILogger<OrderNotificationService> logger,
        IOptions<RabbitMQConfig> config)
    {
        _logger = logger;
        _config = config.Value;
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _config.HostName,
                UserName = _config.UserName,
                Password = _config.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: _config.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _logger.LogInformation("RabbitMQ connection initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null) throw new InvalidOperationException("RabbitMQ channel not initialized");

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var orderId = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received message: {OrderId}", orderId);
                _logger.LogInformation("Sending email for order {OrderId}", orderId);
                _logger.LogInformation("Sending SMS for order {OrderId}", orderId);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: _config.QueueName,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
} 