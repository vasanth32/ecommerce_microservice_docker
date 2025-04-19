using NotificationService.Configuration;
using NotificationService.Services;
using RabbitMQ.Client;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddHostedService<OrderNotificationService>();
builder.Services.AddScoped<IOrderPublisherService, OrderPublisherService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add a simple health check endpoint
app.MapGet("/health", () => Results.Ok("Notification Service is running!"))
    .WithName("Health")
    .WithOpenApi();

// Add test endpoint to simulate OrderService
app.MapPost("/test/orders/{orderId}", (string orderId, IOrderPublisherService publisher) =>
{
    publisher.PublishOrder(orderId);
    return Results.Ok($"Order {orderId} published to queue");
})
.WithName("TestPublishOrder")
.WithOpenApi();

// Add RabbitMQ connection test endpoint
app.MapGet("/test/rabbitmq", (IOptions<RabbitMQConfig> config) =>
{
    try
    {
        var factory = new ConnectionFactory
        {
            HostName = config.Value.HostName,
            UserName = config.Value.UserName,
            Password = config.Value.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        
        return Results.Ok("Successfully connected to RabbitMQ!");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to connect to RabbitMQ: {ex.Message}");
    }
})
.WithName("TestRabbitMQConnection")
.WithOpenApi();

app.Run();
