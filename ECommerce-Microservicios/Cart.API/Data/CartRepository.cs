using Cart.API.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Runtime.ConstrainedExecution;

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

            var cart = await conn.QueryFirstOrDefaultAsync<Cart.API.Models.Cart>(
                """
                SELECT *
                FROM carts
                WHERE usuarioId = @UsuarioId
                """,
                new { UsuarioId = userId.ToString() });

            if (cart == null)
                return null;

            var items = await conn.QueryAsync<CartItem>(
                """
                SELECT productoId AS ProductoId,
                       cantidad AS Cantidad
                FROM cart_items
                WHERE usuarioId = @UsuarioId
                """,
                new { UsuarioId = userId.ToString() });

            cart.Items = items.ToList();

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