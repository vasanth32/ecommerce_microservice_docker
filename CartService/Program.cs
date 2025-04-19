using CartService.Models;
using CartService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<CartManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Cart endpoints
app.MapGet("/api/cart/{userId}", (string userId, CartManager cartManager) =>
{
    var cart = cartManager.GetCart(userId);
    return Results.Ok(cart);
})
.WithName("GetCart")
.WithOpenApi();

app.MapPost("/api/cart/{userId}", (string userId, CartItem item, CartManager cartManager) =>
{
    cartManager.AddItem(userId, item);
    return Results.Ok();
})
.WithName("AddToCart")
.WithOpenApi();

app.MapDelete("/api/cart/{userId}/{productId}", (string userId, string productId, CartManager cartManager) =>
{
    var result = cartManager.RemoveItem(userId, productId);
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("RemoveFromCart")
.WithOpenApi();

app.Run();
