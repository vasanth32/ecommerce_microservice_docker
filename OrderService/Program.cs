using OrderService.Clients;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register ProductClient with logging of HTTP requests
builder.Services.AddHttpClient<ProductClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000"); // Updated to match ProductService port
})
.AddHttpMessageHandler(() =>
{
    return new LoggingHttpMessageHandler(builder.Services.BuildServiceProvider()
        .GetRequiredService<ILogger<ProductClient>>());
});

// Register RabbitMQ Service
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Urls.Add("http://*:5001");
app.Run();

// Custom HTTP message handler for logging
public class LoggingHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public LoggingHttpMessageHandler(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending request to {Url}", request.RequestUri);
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            _logger.LogInformation("Received response with status code {StatusCode}", response.StatusCode);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending request to {Url}", request.RequestUri);
            throw;
        }
    }
}
