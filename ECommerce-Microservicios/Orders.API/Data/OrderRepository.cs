//OrderRepository es el encargado de todas las operaciones con la base de datos
//el OrderService le pide que guarde, busque o actualice ordenes
//y el Repository se encarga de hablar con SQLite
using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;

namespace Orders.API.Data
{
    public class OrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=orders.db";
        }

        private SqliteConnection CreateConnection() => new(_connectionString);

        public async Task<IEnumerable<Order>> ObtenerTodas(Guid? usuarioId)
        {
            using var conn = CreateConnection();

            var ordenes = usuarioId.HasValue
                ? await conn.QueryAsync<Order>(
                    "SELECT * FROM orders WHERE usuarioId = @UsuarioId",
                    new { UsuarioId = usuarioId.Value.ToString() })
                : await conn.QueryAsync<Order>("SELECT * FROM orders");

            foreach (var orden in ordenes)
            {
                orden.Items = (await conn.QueryAsync<OrderItem>(
                    "SELECT * FROM order_items WHERE orderId = @OrderId",
                    new { OrderId = orden.Id.ToString() })).ToList();
            }

            return ordenes;
        }

        public async Task<Order?> ObtenerPorId(Guid id)
        {
            using var conn = CreateConnection();

            var orden = await conn.QueryFirstOrDefaultAsync<Order>(
                "SELECT * FROM orders WHERE id = @Id",
                new { Id = id.ToString() });

            if (orden == null) return null;

            orden.Items = (await conn.QueryAsync<OrderItem>(
                "SELECT * FROM order_items WHERE orderId = @OrderId",
                new { OrderId = id.ToString() })).ToList();

            return orden;
        }

        public async Task Guardar(Order orden)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync("""
                INSERT INTO orders (id, usuarioId, total, estado, fechaCreacion)
                VALUES (@Id, @UsuarioId, @Total, @Estado, @FechaCreacion)
            """, new
            {
                Id = orden.Id.ToString(),
                UsuarioId = orden.UsuarioId.ToString(),
                orden.Total,
                orden.Estado,
                FechaCreacion = orden.FechaCreacion.ToString("o")
            });

            foreach (var item in orden.Items)
            {
                await conn.ExecuteAsync("""
                    INSERT INTO order_items (id, orderId, productoId, cantidad, precioUnitario)
                    VALUES (@Id, @OrderId, @ProductoId, @Cantidad, @PrecioUnitario)
                """, new
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = orden.Id.ToString(),
                    ProductoId = item.ProductoId.ToString(),
                    item.Cantidad,
                    item.PrecioUnitario
                });
            }
        }

        public async Task ActualizarEstado(Guid id, string nuevoEstado)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync(
                "UPDATE orders SET estado = @Estado WHERE id = @Id",
                new { Estado = nuevoEstado, Id = id.ToString() });
        }
    }
}