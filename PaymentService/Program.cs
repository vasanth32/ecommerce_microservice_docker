using PaymentService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

app.MapPost("/api/payments", (PaymentRequest payment) =>
{
    // Mock successful payment processing
    return Results.Ok(new { Message = $"Payment for OrderId {payment.OrderId} successful." });
})
.WithName("ProcessPayment")
.WithOpenApi();

app.Run();
