using Cart.API.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Data
{
    public class CartRepository
    {
        private readonly string _connectionString;

        public CartRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=cart.db";
        }

        private SqliteConnection CreateConnection()
            => new(_connectionString);

        public async Task<Cart.API.Models.Cart?> ObtenerPorUsuarioId(Guid userId)
        {
            using var conn = CreateConnection();

            // 1. Buscamos el carrito como tipo dinámico (Dapper devuelve texto crudo)
            var cartRow = await conn.QueryFirstOrDefaultAsync(
                """
                SELECT usuarioId, fechaActualizacion
                FROM carts
                WHERE usuarioId = @UsuarioId
                """,
                new { UsuarioId = userId.ToString() });

            if (cartRow == null)
                return null;

            // 2. Lo convertimos (mapeamos) manualmente a nuestro modelo C#
            var cart = new Cart.API.Models.Cart
            {
                UsuarioId = Guid.Parse((string)cartRow.usuarioId),
                FechaActualizacion = DateTime.Parse((string)cartRow.fechaActualizacion)
            };

            // 3. Hacemos lo mismo con los ítems del carrito
            var itemsRows = await conn.QueryAsync(
                """
                SELECT productoId, cantidad
                FROM cart_items
                WHERE usuarioId = @UsuarioId
                """,
                new { UsuarioId = userId.ToString() });

            // Mapeamos cada fila de la tabla a un objeto CartItem
            cart.Items = itemsRows.Select(row => new CartItem
            {
                ProductoId = Guid.Parse((string)row.productoId),
                Cantidad = Convert.ToInt32(row.cantidad) 
            }).ToList();

            return cart;
        }

        public async Task Guardar(Cart.API.Models.Cart cart)
        {
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
                    (@UsuarioId, @ProductoId, @Cantidad)
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
}