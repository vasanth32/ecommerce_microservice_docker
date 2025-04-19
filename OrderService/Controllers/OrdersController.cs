using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Clients;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private static readonly List<Order> _orders = new();
    private readonly ProductClient _productClient;
    private readonly IRabbitMQService _rabbitMQService;

    public OrdersController(ProductClient productClient, IRabbitMQService rabbitMQService)
    {
        _productClient = productClient;
        _rabbitMQService = rabbitMQService;
    }

    // GET: api/Orders
    [HttpGet]
    public ActionResult<IEnumerable<Order>> GetOrders()
    {
        return Ok(_orders);
    }

    // POST: api/Orders
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(Order order)
    {
        // Temporarily commenting out product validation for testing
        /*try
        {
            var products = await _productClient.GetAllProductsAsync();
            if (!products.Any(p => p.GetProperty("id").GetInt32() == order.ProductId))
            {
                return BadRequest("Invalid ProductId");
            }
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "Product service is unavailable");
        }*/

        // Set initial values
        order.Id = _orders.Count > 0 ? _orders.Max(o => o.Id) + 1 : 1;
        order.Status = OrderStatus.Placed;

        _orders.Add(order);

        // Publish to RabbitMQ
        _rabbitMQService.PublishOrderPlaced(order);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    // GET: api/Orders/1
    [HttpGet("{id:int}")]
    public ActionResult<Order> GetOrder(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    // PUT: api/Orders/1/status
    [HttpPut("{id:int}/status")]
    public IActionResult UpdateOrderStatus(int id, [FromBody] string status)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        // Validate status
        var validStatuses = typeof(OrderStatus).GetFields()
            .Where(f => f.IsLiteral)
            .Select(f => f.GetValue(null)?.ToString());

        if (!validStatuses.Contains(status))
        {
            return BadRequest($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        order.Status = status;
        return NoContent();
    }

    // GET: api/Orders/products
    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetProductsFromProductService()
    {
        try
        {
            var products = await _productClient.GetAllProductsAsync();
            return Ok(products);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, $"Product service is unavailable: {ex.Message}");
        }
    }
}
