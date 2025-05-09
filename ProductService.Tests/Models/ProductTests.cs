using System.ComponentModel.DataAnnotations;
using ProductService.Models;
using Xunit;

namespace ProductService.Tests.Models;

public class ProductTests
{
    [Fact]
    public void Product_ValidData_PassesValidation()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test Category",
            Quantity = 10
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(product, 
            new ValidationContext(product), 
            validationResults, 
            true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("", "The Name field is required.")]
    [InlineData("ThisNameIsTooLongThisNameIsTooLongThisNameIsTooLongThisNameIsTooLongThisNameIsTooLongThisNameIsTooLongThisNameIsTooLong", "The field Name must be a string with a maximum length of 100.")]
    public void Product_InvalidName_FailsValidation(string name, string expectedError)
    {
        // Arrange
        var product = new Product
        {
            Name = name,
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test Category",
            Quantity = 10
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(product, 
            new ValidationContext(product), 
            validationResults, 
            true);

        // Assert
        Assert.False(isValid);
        var errorMessage = validationResults.First().ErrorMessage;
        Assert.Equal(expectedError, errorMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99.99)]
    public void Product_InvalidPrice_FailsValidation(decimal price)
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = price,
            Category = "Test Category",
            Quantity = 10
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(product, 
            new ValidationContext(product), 
            validationResults, 
            true);

        // Assert
        Assert.False(isValid);
        var errorMessage = validationResults.First().ErrorMessage;
        Assert.Equal("Price must be greater than 0.", errorMessage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Product_InvalidQuantity_FailsValidation(int quantity)
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test Category",
            Quantity = quantity
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(product, 
            new ValidationContext(product), 
            validationResults, 
            true);

        // Assert
        Assert.False(isValid);
        var errorMessage = validationResults.First().ErrorMessage;
        Assert.Equal("Quantity must be greater than or equal to 0.", errorMessage);
    }
} 