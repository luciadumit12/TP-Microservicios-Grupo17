using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;
using System.Net.Http.Json;  

namespace Products.API.Services
{
    public class ProductService
    {
        private static readonly List<Product> _products = new();
        private readonly HttpClient _ordersClient; 

        public ProductService(IHttpClientFactory httpClientFactory)  // ← constructor
        {
            _ordersClient = httpClientFactory.CreateClient("OrdersAPI");
        }


        public async Task DeleteAsync(Guid id)  
        {
            var product = _products.FirstOrDefault(p => p.Id == id)
                ?? throw new NotFoundException("PRD-001", "Producto no encontrado.");

            var response = await _ordersClient.GetAsync("api/orders");
            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
                var tieneOrdenes = orders?.Any(o =>
                    o.Items.Any(i => i.ProductoId == id) &&
                    (o.Estado == "Pendiente" || o.Estado == "Confirmada"));

                if (tieneOrdenes == true)
                    throw new BusinessRuleException("PRD-004", "El producto tiene órdenes activas y no puede eliminarse.");
            }

            _products.Remove(product);
        }

        //
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string Estado { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public Guid ProductoId { get; set; }
    }
}