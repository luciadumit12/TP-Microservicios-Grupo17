using Cart.API.Data;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Models;
using System.Net.Http.Json;

namespace Cart.API.Services
{
    public class CartService
    {
        private readonly CartRepository _repository;
        private readonly HttpClient _productsClient;
        private readonly HttpClient _usersClient;

        public CartService(
            CartRepository repository,
            IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _productsClient = httpClientFactory.CreateClient("ProductsAPI");
            _usersClient = httpClientFactory.CreateClient("UsersAPI");
        }

        //metodo privado para validar que el usuario existe en Users.API, reutilizado en todos los endpoints
        private async Task ValidarUsuarioExisteAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ValidationException("CRT-004", "El ID del usuario no puede estar vacío."); //Si está vacío: Lanza inmediatamente una ValidationException(CRT-004).

            // Llamada real a Users.API para verificar existencia
            var userResponse = await _usersClient.GetAsync($"api/users/{userId}");

            if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Si Users.API devuelve 404, significa que el GUID no corresponde a ningún usuario registrado
                throw new NotFoundException("CRT-006", "El usuario especificado no existe.");
            }

            if (!userResponse.IsSuccessStatusCode)
            {
                throw new Exception("Error interno al validar el usuario en Users.API.");
            }
        }

        // GET carrito
        public async Task<CartResponse> GetByUserIdAsync(Guid userId)
        {
            await ValidarUsuarioExisteAsync(userId); // Valida Empty y existencia en Users.API

            var cart = await _repository.ObtenerPorUsuarioId(userId)
                ?? throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            return MapToResponse(cart);
        }

        // ADD item
        public async Task<CartResponse> AddItemAsync(Guid userId, AddCartItemRequest request)
        {
            await ValidarUsuarioExisteAsync(userId);// Valida Empty y existencia en Users.API

            if (request.Cantidad <= 0)
                throw new ValidationException("CRT-004", "Cantidad inválida.");

            var productResponse = await _productsClient.GetAsync($"api/products/{request.ProductoId}");
            if (!productResponse.IsSuccessStatusCode)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>()
                ?? throw new NotFoundException("CRT-002", "Producto no encontrado.");

            var cart = await _repository.ObtenerPorUsuarioId(userId)
                ?? new Cart.API.Models.Cart { UsuarioId = userId };

            var item = cart.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);

            // OPTIMIZACIÓN: Validamos el stock total real (la cantidad solicitada + lo que ya tenía en el carrito)
            int cantidadTotal = request.Cantidad + (item?.Cantidad ?? 0);
            if (product.Stock < cantidadTotal)
            {
                throw new BusinessRuleException(
                    "CRT-003",
                    $"Stock insuficiente. Disponible: {product.Stock}, solicitado total en carrito: {cantidadTotal}"
                );
            }

            if (item == null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductoId = request.ProductoId,
                    Cantidad = request.Cantidad
                });
            }
            else
            {
                item.Cantidad += request.Cantidad;
            }

            cart.FechaActualizacion = DateTime.UtcNow;

            await _repository.Guardar(cart);

            return MapToResponse(cart);
        }

        // UPDATE item
        public async Task<CartResponse> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequest request)
        {
            await ValidarUsuarioExisteAsync(userId); // Valida Empty y existencia en Users.API

            if (request.Cantidad <= 0)
                throw new ValidationException("CRT-004", "Cantidad inválida.");

            var cart = await _repository.ObtenerPorUsuarioId(userId)
                ?? throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId)
                ?? throw new NotFoundException("CRT-002", "Producto no encontrado en el carrito.");

            var productResponse = await _productsClient.GetAsync($"api/products/{productId}");
            if (!productResponse.IsSuccessStatusCode)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>()
                ?? throw new NotFoundException("CRT-002", "Producto no encontrado.");

            if (product.Stock < request.Cantidad)
                throw new BusinessRuleException(
                    "CRT-003",
                    $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {request.Cantidad}"
                );

            item.Cantidad = request.Cantidad;
            cart.FechaActualizacion = DateTime.UtcNow;

            await _repository.Guardar(cart);

            return MapToResponse(cart);
        }

        // DELETE item
        public async Task DeleteItemAsync(Guid userId, Guid productId)
        {
            await ValidarUsuarioExisteAsync(userId); // Valida Empty y existencia en Users.API

            var cart = await _repository.ObtenerPorUsuarioId(userId)
                ?? throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId)
                ?? throw new NotFoundException("CRT-002", "Producto no encontrado.");

            cart.Items.Remove(item);
            cart.FechaActualizacion = DateTime.UtcNow;

            await _repository.Guardar(cart);
        }

        // CLEAR cart
        public async Task ClearCartAsync(Guid userId)
        {
            await ValidarUsuarioExisteAsync(userId); // Valida Empty y existencia en Users.API

            var cart = await _repository.ObtenerPorUsuarioId(userId)
                ?? throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            await _repository.Eliminar(userId);
        }

        // mapper
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

    // DTO interno para Products.API
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}