using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrderService.Clients
{
    public class ProductClient
    {
        private readonly HttpClient _httpClient;
        private const string ProductServiceUrl = "http://productservice:5000/api/Products";

        public ProductClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GetProductsAsync()
        {
            var response = await _httpClient.GetAsync(ProductServiceUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
} 