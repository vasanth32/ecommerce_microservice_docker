using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static readonly List<Product> _defaultProducts = new()
    {
        new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Quantity = 10, Category = "Electronics" },
        new Product { Id = 2, Name = "Smartphone", Description = "Latest smartphone", Price = 599.99m, Quantity = 15, Category = "Electronics" },
        new Product { Id = 3, Name = "Headphones", Description = "Wireless headphones", Price = 199.99m, Quantity = 20, Category = "Accessories" }
    };

    private readonly List<Product> _products;

    public ProductsController()
    {
        _products = new List<Product>(_defaultProducts);
    }

    public ProductsController(List<Product> initialProducts)
    {
        _products = initialProducts;
    }

    // GET: api/products
    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProducts([FromQuery] string? category = null)
    {
        var query = _products.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        return query.ToList();
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return product;
    }

    // POST: api/products
    [HttpPost]
    public ActionResult<Product> CreateProduct(Product product)
    {
        // Validate model
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate business rules
        if (_products.Any(p => p.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", "A product with this name already exists.");
            return BadRequest(ModelState);
        }

        if (product.Price <= 0)
        {
            ModelState.AddModelError("Price", "Price must be greater than 0.");
            return BadRequest(ModelState);
        }

        if (product.Quantity < 0)
        {
            ModelState.AddModelError("Quantity", "Quantity must be greater than or equal to 0.");
            return BadRequest(ModelState);
        }

        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // PUT: api/products/5
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest("Product ID mismatch");
        }

        // Validate model
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingProduct = _products.FirstOrDefault(p => p.Id == id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        // Validate business rules
        if (product.Price <= 0)
        {
            ModelState.AddModelError("Price", "Price must be greater than 0.");
            return BadRequest(ModelState);
        }

        if (product.Quantity < 0)
        {
            ModelState.AddModelError("Quantity", "Quantity must be greater than or equal to 0.");
            return BadRequest(ModelState);
        }

        // Check for duplicate name (excluding the current product)
        if (_products.Any(p => p.Id != id && p.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", "A product with this name already exists.");
            return BadRequest(ModelState);
        }

        var index = _products.IndexOf(existingProduct);
        _products[index] = product;

        return NoContent();
    }

    // DELETE: api/products/5
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        _products.Remove(product);

        return NoContent();
    }
} 