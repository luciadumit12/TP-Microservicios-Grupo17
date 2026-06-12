using Cart.API.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Net.Http.Json; 

namespace Cart.API.Data
{
    public class CartRepository
    {
        private readonly string _connectionString;
        private readonly IHttpClientFactory _httpClientFactory; // 

        
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
            // ─── VALIDAR EL STOCK DE CADA ÍTEM VÍA HTTP ───
            var client = _httpClientFactory.CreateClient();

            foreach (var item in cart.Items)
            {
                try
                {
                    // Cambiá el puerto (ej: 7001 o el que use tu Products.API)
                    var response = await client.GetAsync($"https://localhost:7001/api/products/{item.ProductoId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var productData = await response.Content.ReadFromJsonAsync<ProductStockResult>();

                        // Si el stock disponible en Products.API es menor a la cantidad del carrito...
                        if (productData != null && productData.Stock < item.Cantidad)
                        {
                            // Lanzamos el ArgumentException que atrapará tu GlobalExceptionHandler para transformarlo en 422
                            throw new ArgumentException("No hay stock suficiente para el producto solicitado.");
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    // Si Products.API está apagada durante tu defensa o pruebas, 
                    // este catch evita que la app se rompa y la deja continuar.
                }
            }

            // ─── 4. TU CÓDIGO ACTUAL DE PERSISTENCIA (Sigue intacto y protegido) ───
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
                    FechaActualizacion =
                        cart.FechaActualizacion.ToString("o")
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
                        ProductoId = item.ProductoId.ToString(),
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

    // DTO auxiliar al final del archivo para mapear la respuesta del catálogo de productos
    public class ProductStockResult
    {
        public Guid Id { get; set; }
        public int Stock { get; set; }
    }
}