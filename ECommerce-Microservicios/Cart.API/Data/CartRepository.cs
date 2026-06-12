using Cart.API.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Net.Http.Json;

namespace Cart.API.Data
{
    public class CartRepository
    {
        private readonly string _connectionString;
        private readonly IHttpClientFactory _httpClientFactory;

        public CartRepository(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=cart.db";
            _httpClientFactory = httpClientFactory;
        }

        private SqliteConnection CreateConnection()
            => new(_connectionString);

        public async Task<Cart.API.Models.Cart?> ObtenerPorUsuarioId(Guid userId)
        {
            using var conn = CreateConnection();

            var cartRow = await conn.QueryFirstOrDefaultAsync(
                """
                SELECT usuarioId, fechaActualizacion
                FROM carts
                WHERE usuarioId = @UsuarioId
                """,
                new { UsuarioId = userId.ToString() });

            if (cartRow == null)
                return null;

            var cart = new Cart.API.Models.Cart
            {
                UsuarioId = Guid.Parse((string)cartRow.usuarioId),
                FechaActualizacion = DateTime.Parse((string)cartRow.fechaActualizacion)
            };

            var itemsRows = await conn.QueryAsync(
                """
                SELECT productoId, cantidad
                FROM cart_items
                WHERE usuarioId = @UsuarioId
                """,
                new { UsuarioId = userId.ToString() });

            cart.Items = itemsRows.Select(row => new CartItem
            {
                ProductoId = Guid.Parse((string)row.productoId),
                Cantidad = Convert.ToInt32(row.cantidad)
            }).ToList();

            return cart;
        }

        public async Task Guardar(Cart.API.Models.Cart cart)
        {
            // Usa el cliente nombrado para heredar la URL base de tu Program.cs (https://localhost:7268/)
            var client = _httpClientFactory.CreateClient("ProductsAPI");

            foreach (var item in cart.Items)
            {
                try
                {
                    var response = await client.GetAsync($"api/products/{item.ProductoId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var productData = await response.Content.ReadFromJsonAsync<ProductStockResult>();

                        if (productData != null && productData.Stock < item.Cantidad)
                        {
                            throw new ArgumentException("No hay stock suficiente para el producto solicitado.");
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    // Si Products.API está apagada, se ignora el chequeo para evitar romper el flujo
                }
            }

            using var conn = CreateConnection();

            await conn.ExecuteAsync(
                """
                INSERT OR REPLACE INTO carts
                (usuarioId, fechaActualizacion)
                VALUES
                (@UsuarioId, @FechaActualizacion)
                """,
                new
                {
                    UsuarioId = cart.UsuarioId.ToString(),
                    FechaActualizacion = cart.FechaActualizacion.ToString("o")
                });

            await conn.ExecuteAsync(
                """
                DELETE FROM cart_items
                WHERE usuarioId = @UsuarioId
                """,
                new { UsuarioId = cart.UsuarioId.ToString() });

            foreach (var item in cart.Items)
            {
                await conn.ExecuteAsync(
                    """
                    INSERT INTO cart_items
                    (usuarioId, productoId, cantidad)
                    VALUES
                    (@UsuarioId, @ProductId, @Cantidad)
                    """,
                    new
                    {
                        UsuarioId = cart.UsuarioId.ToString(),
                        ProductId = item.ProductoId.ToString(), // Corregido el nombre para matchear con @ProductId
                        item.Cantidad
                    });
            }
        }

        public async Task Eliminar(Guid userId)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync(
                """
                DELETE FROM cart_items
                WHERE usuarioId = @UsuarioId
                """,
                new { UsuarioId = userId.ToString() });

            await conn.ExecuteAsync(
                """
                DELETE FROM carts
                WHERE usuarioId = @UsuarioId
                """,
                new { UsuarioId = userId.ToString() });
        }
    }

    public class ProductStockResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public Guid Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("stock")]
        public int Stock { get; set; }
    }
}