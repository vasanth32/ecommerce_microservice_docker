namespace NotificationService.Configuration;

public class RabbitMQConfig
{
    public string HostName { get; set; } = "localhost";
    public string QueueName { get; set; } = "order_placed";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
} 