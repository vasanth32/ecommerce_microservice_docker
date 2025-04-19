using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace OrderService.Clients;

public class ProductClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductClient> _logger;

    public ProductClient(HttpClient httpClient, ILogger<ProductClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<dynamic>> GetAllProductsAsync()
    {
        try
        {
            _logger.LogInformation("Attempting to get products from: {BaseAddress}/api/Products", _httpClient.BaseAddress);
            var response = await _httpClient.GetAsync("/api/Products");
            _logger.LogInformation("Response status code: {StatusCode}", response.StatusCode);
            
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Received content: {Content}", content);
            
            return JsonSerializer.Deserialize<IEnumerable<dynamic>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products from ProductService");
            throw;
        }
    }
} 