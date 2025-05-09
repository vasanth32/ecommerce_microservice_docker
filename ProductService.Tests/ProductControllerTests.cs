using Microsoft.AspNetCore.Mvc;
using ProductService.Controllers;
using ProductService.Models;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace ProductService.Tests;

public class ProductControllerTests
{
    private readonly ProductsController _controller;
    private static readonly List<Product> _initialProducts = new()
    {
        new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Quantity = 10, Category = "Electronics" },
        new Product { Id = 2, Name = "Smartphone", Description = "Latest smartphone", Price = 599.99m, Quantity = 15, Category = "Electronics" },
        new Product { Id = 3, Name = "Headphones", Description = "Wireless headphones", Price = 199.99m, Quantity = 20, Category = "Accessories" }
    };

    public ProductControllerTests()
    {
        _controller = new ProductsController(new List<Product>(_initialProducts));
    }

    private void ResetProducts()
    {
        // Use reflection to access the private static field
        var field = typeof(ProductsController).GetField("_products", BindingFlags.NonPublic | BindingFlags.Static);
        var products = new List<Product>();
        foreach (var product in _initialProducts)
        {
            products.Add(new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Category = product.Category,
                Quantity = product.Quantity
            });
        }
        field?.SetValue(null, products);
    }

    [Fact]
    public void GetProducts_ReturnsAllProducts()
    {
        // Act
        var actionResult = _controller.GetProducts();

        // Assert
        var result = Assert.IsType<ActionResult<IEnumerable<Product>>>(actionResult);
        var products = Assert.IsType<List<Product>>(result.Value);
        Assert.Equal(3, products.Count);
        
        // Verify the content of the products
        Assert.Contains(products, p => p.Name == "Laptop" && p.Category == "Electronics");
        Assert.Contains(products, p => p.Name == "Smartphone" && p.Category == "Electronics");
        Assert.Contains(products, p => p.Name == "Headphones" && p.Category == "Accessories");
    }

    [Fact]
    public void GetProducts_WithCategory_ReturnsFilteredProducts()
    {
        // Act
        var actionResult = _controller.GetProducts("Electronics");

        // Assert
        var result = Assert.IsType<ActionResult<IEnumerable<Product>>>(actionResult);
        var products = Assert.IsType<List<Product>>(result.Value);
        Assert.Equal(2, products.Count);
        Assert.All(products, p => Assert.Equal("Electronics", p.Category));
        
        // Verify specific products
        Assert.Contains(products, p => p.Name == "Laptop");
        Assert.Contains(products, p => p.Name == "Smartphone");
    }

    [Fact]
    public void GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        var testProduct = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Category = "Electronics",
            Quantity = 10
        };

        // Act
        var result = _controller.GetProduct(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);
        Assert.Equal(testProduct.Id, returnedProduct.Id);
        Assert.Equal(testProduct.Name, returnedProduct.Name);
        Assert.Equal(testProduct.Category, returnedProduct.Category);
        Assert.Equal(testProduct.Price, returnedProduct.Price);
    }

    [Fact]
    public void GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = _controller.GetProduct(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void CreateProduct_WithValidProduct_ReturnsCreatedProduct()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test",
            Quantity = 10
        };

        // Act
        var actionResult = _controller.CreateProduct(newProduct);

        // Assert
        var result = Assert.IsType<ActionResult<Product>>(actionResult);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var product = Assert.IsType<Product>(createdAtActionResult.Value);
        Assert.Equal(4, product.Id); // Next available ID
        Assert.Equal("Test Product", product.Name);
    }

    [Fact]
    public void UpdateProduct_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var updatedProduct = new Product
        {
            Id = 1,
            Name = "Updated Laptop",
            Description = "Updated Description",
            Price = 1099.99m,
            Category = "Electronics",
            Quantity = 15
        };

        // Act
        var result = _controller.UpdateProduct(1, updatedProduct);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify the update
        var getResult = _controller.GetProduct(1);
        var product = Assert.IsType<Product>(getResult.Value);
        Assert.Equal("Updated Laptop", product.Name);
        Assert.Equal("Updated Description", product.Description);
        Assert.Equal(1099.99m, product.Price);
    }

    [Fact]
    public void UpdateProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updatedProduct = new Product
        {
            Id = 999,
            Name = "Non-existent Product"
        };

        // Act
        var result = _controller.UpdateProduct(999, updatedProduct);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void DeleteProduct_WithValidId_ReturnsNoContent()
    {
        // Act
        var result = _controller.DeleteProduct(1);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify the deletion
        var getResult = _controller.GetProduct(1);
        Assert.IsType<NotFoundResult>(getResult.Result);
    }

    [Fact]
    public void DeleteProduct_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = _controller.DeleteProduct(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void CreateProduct_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var invalidProduct = new Product
        {
            Name = "", // Empty name
            Price = -1, // Negative price
            Quantity = -1, // Negative quantity
            Category = "" // Empty category
        };

        // Act
        var result = _controller.CreateProduct(invalidProduct);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Product>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public void CreateProduct_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var duplicateProduct = new Product
        {
            Name = "Laptop", // Duplicate name
            Description = "Another laptop",
            Price = 899.99m,
            Category = "Electronics",
            Quantity = 5
        };

        // Act
        var result = _controller.CreateProduct(duplicateProduct);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Product>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public void UpdateProduct_WithZeroPrice_ReturnsBadRequest()
    {
        // Arrange
        var updatedProduct = new Product
        {
            Id = 1,
            Name = "Laptop",
            Description = "High-performance laptop",
            Price = 0, // Zero price
            Category = "Electronics",
            Quantity = 10
        };

        // Act
        var result = _controller.UpdateProduct(1, updatedProduct);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void GetProducts_WithEmptyCategory_ReturnsAllProducts()
    {
        // Act
        var actionResult = _controller.GetProducts("");

        // Assert
        var result = Assert.IsType<ActionResult<IEnumerable<Product>>>(actionResult);
        var products = Assert.IsType<List<Product>>(result.Value);
        Assert.Equal(3, products.Count);
    }

    [Fact]
    public void GetProducts_WithNonExistentCategory_ReturnsEmptyList()
    {
        // Act
        var actionResult = _controller.GetProducts("NonExistentCategory");

        // Assert
        var result = Assert.IsType<ActionResult<IEnumerable<Product>>>(actionResult);
        var products = Assert.IsType<List<Product>>(result.Value);
        Assert.Empty(products);
    }

    [Fact]
    public void UpdateProduct_WithInvalidQuantity_ReturnsBadRequest()
    {
        // Arrange
        var updatedProduct = new Product
        {
            Id = 1,
            Name = "Laptop",
            Description = "High-performance laptop",
            Price = 999.99m,
            Category = "Electronics",
            Quantity = -1 // Invalid quantity
        };

        // Act
        var result = _controller.UpdateProduct(1, updatedProduct);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void CreateProduct_WithMaxLengthName_ReturnsCreatedProduct()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = new string('a', 100), // Max length name
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test",
            Quantity = 10
        };

        // Act
        var actionResult = _controller.CreateProduct(newProduct);

        // Assert
        var result = Assert.IsType<ActionResult<Product>>(actionResult);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var product = Assert.IsType<Product>(createdAtActionResult.Value);
        Assert.Equal(100, product.Name.Length);
    }

    [Fact]
    public void CreateProduct_WithExceedingMaxLengthName_ReturnsBadRequest()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = new string('a', 101), // Exceeding max length
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test",
            Quantity = 10
        };

        // Act
        var result = _controller.CreateProduct(newProduct);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Product>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public void UpdateProduct_WithDifferentId_ReturnsBadRequest()
    {
        // Arrange
        var updatedProduct = new Product
        {
            Id = 2, // Different ID
            Name = "Updated Laptop",
            Description = "Updated Description",
            Price = 1099.99m,
            Category = "Electronics",
            Quantity = 15
        };

        // Act
        var result = _controller.UpdateProduct(1, updatedProduct);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void DeleteProduct_WithZeroQuantity_ReturnsNoContent()
    {
        // Arrange
        var product = _controller.GetProduct(1).Value;
        product.Quantity = 0;

        // Act
        var result = _controller.DeleteProduct(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
} 