using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Controllers;
using ProductService.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ProductService.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly List<Product> _testProducts;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _loggerMock = new Mock<ILogger<ProductsController>>();
        _testProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Test Product 1", Description = "Description 1", Price = 99.99m, Category = "Electronics", Quantity = 10 },
            new Product { Id = 2, Name = "Test Product 2", Description = "Description 2", Price = 149.99m, Category = "Books", Quantity = 5 }
        };
        
        _controller = new ProductsController(_testProducts);
    }

    [Fact]
    public void GetProducts_ReturnsAllProducts_WhenNoCategorySpecified()
    {
        // Act
        var result = _controller.GetProducts(string.Empty);

        // Assert
        var products = Assert.IsType<List<Product>>(result.Value);
        Assert.Equal(_testProducts.Count, products.Count);
    }

    [Fact]
    public void GetProducts_ReturnsFilteredProducts_WhenCategorySpecified()
    {
        // Arrange
        var category = "Electronics";

        // Act
        var result = _controller.GetProducts(category);

        // Assert
        var products = Assert.IsType<List<Product>>(result.Value);
        Assert.Single(products);
        Assert.All(products, p => Assert.Equal(category, p.Category));
    }

    [Fact]
    public void CreateProduct_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("Name", "Required");
        var product = new Product();

        // Act
        var result = _controller.CreateProduct(product);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public void CreateProduct_ReturnsCreatedAtAction_WhenProductIsValid()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "New Product",
            Description = "New Description",
            Price = 199.99m,
            Category = "New Category",
            Quantity = 30
        };

        // Act
        var result = _controller.CreateProduct(newProduct);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = Assert.IsType<Product>(createdAtActionResult.Value);
        Assert.Equal(newProduct.Name, returnValue.Name);
        Assert.Equal(3, returnValue.Id); // Next available ID
    }

    [Fact]
    public void UpdateProduct_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;
        var product = new Product { Id = nonExistentId, Name = "Non-existent Product" };

        // Act
        var result = _controller.UpdateProduct(nonExistentId, product);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void UpdateProduct_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = 2, Name = "Updated Product" };

        // Act
        var result = _controller.UpdateProduct(productId, product);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Product ID mismatch", badRequestResult.Value);
    }

    [Fact]
    public void DeleteProduct_ReturnsNoContent_WhenProductExists()
    {
        // Arrange
        var productId = 1;

        // Act
        var result = _controller.DeleteProduct(productId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        
        var getResult = _controller.GetProduct(productId);
        Assert.IsType<NotFoundResult>(getResult.Result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GetProducts_ReturnsAllProducts_WhenCategoryIsEmptyOrWhitespace(string category)
    {
        // Act
        var result = _controller.GetProducts(category);

        // Assert
        var products = Assert.IsType<List<Product>>(result.Value);
        Assert.Equal(_testProducts.Count, products.Count);
    }

    [Fact]
    public void GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var expectedProduct = _testProducts.First(p => p.Id == productId);

        // Act
        var result = _controller.GetProduct(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);
        Assert.Equal(expectedProduct.Id, returnedProduct.Id);
        Assert.Equal(expectedProduct.Name, returnedProduct.Name);
    }

    [Fact]
    public void CreateProduct_WithExceedingMaxLengthName_ReturnsBadRequest()
    {
        // Arrange
        var product = new Product
        {
            Name = new string('A', 101), // Exceeds max length of 100
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test Category",
            Quantity = 10
        };

        // Act
        var result = _controller.CreateProduct(product);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public void UpdateProduct_WithDifferentId_ReturnsBadRequest()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = 2, Name = "Updated Product" };

        // Act
        var result = _controller.UpdateProduct(productId, product);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Product ID mismatch", badRequestResult.Value);
    }

    [Fact]
    public void CreateProduct_ReturnsBadRequest_WhenDuplicateNameExists()
    {
        // Arrange
        var existingProduct = _testProducts[0];
        var newProduct = new Product
        {
            Name = existingProduct.Name,
            Description = "New Description",
            Price = 79.99m,
            Category = "Electronics",
            Quantity = 5
        };

        // Act
        var result = _controller.CreateProduct(newProduct);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("A product with this name already exists.", badRequestResult.Value);
    }
} 