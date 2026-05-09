using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Models;
using System.Net.Http.Json;

namespace Cart.API.Services
{
    public class CartService
    {
        private static readonly List<Cart.API.Models.Cart> _carts = new();
        private readonly HttpClient _productsClient;

        public CartService(IHttpClientFactory httpClientFactory)
        {
            _productsClient = httpClientFactory.CreateClient("ProductsAPI");
        }

        public Task<CartResponse> GetByUserIdAsync(Guid userId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId)
                ?? throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            return Task.FromResult(MapToResponse(cart));
        }

        public async Task<CartResponse> AddItemAsync(Guid userId, AddCartItemRequest request)
        {
            if (request.Cantidad <= 0)
                throw new ValidationException("CRT-004", "Cantidad inválida.");

            // Validar producto y stock en Products.API
            var response = await _productsClient.GetAsync($"api/products/{request.ProductoId}");
            if (!response.IsSuccessStatusCode)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            var product = await response.Content.ReadFromJsonAsync<ProductDto>()
                ?? throw new NotFoundException("CRT-002", "Producto no encontrado.");

            if (product.Stock < request.Cantidad)
                throw new BusinessRuleException("CRT-003", $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {request.Cantidad}.");

            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            if (cart == null)
            {
                cart = new Cart.API.Models.Cart { UsuarioId = userId };
                _carts.Add(cart);
            }

            var item = cart.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);
            if (item == null)
                cart.Items.Add(new CartItem { ProductoId = request.ProductoId, Cantidad = request.Cantidad });
            else
                item.Cantidad += request.Cantidad;

            cart.FechaActualizacion = DateTime.UtcNow;
            return MapToResponse(cart);
        }

        public async Task<CartResponse> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequest request)
        {
            if (request.Cantidad <= 0)
                throw new ValidationException("CRT-004", "Cantidad inválida.");

            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId)
                ?? throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId)
                ?? throw new NotFoundException("CRT-002", "Producto no encontrado en el carrito.");

            // Validar stock en Products.API
            var response = await _productsClient.GetAsync($"api/products/{productId}");
            if (!response.IsSuccessStatusCode)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            var product = await response.Content.ReadFromJsonAsync<ProductDto>()
                ?? throw new NotFoundException("CRT-002", "Producto no encontrado.");

            if (product.Stock < request.Cantidad)
                throw new BusinessRuleException("CRT-003", $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {request.Cantidad}.");

            item.Cantidad = request.Cantidad;
            cart.FechaActualizacion = DateTime.UtcNow;
            return MapToResponse(cart);
        }

        public Task DeleteItemAsync(Guid userId, Guid productId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId)
                ?? throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId)
                ?? throw new NotFoundException("CRT-002", "Producto no encontrado en el carrito.");

            cart.Items.Remove(item);
            cart.FechaActualizacion = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public Task ClearCartAsync(Guid userId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId)
                ?? throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            _carts.Remove(cart);
            return Task.CompletedTask;
        }

        private static CartResponse MapToResponse(Cart.API.Models.Cart c) => new()
        {
            UsuarioId = c.UsuarioId,
            Items = c.Items.Select(i => new CartItemResponse
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad
            }).ToList(),
            FechaActualizacion = c.FechaActualizacion
        };
    }

    // DTO interno para leer la respuesta de Products.API
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}