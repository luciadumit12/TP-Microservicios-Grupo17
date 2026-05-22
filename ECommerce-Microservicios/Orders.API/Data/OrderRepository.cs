//.OrderRepository es el encargado de todas las operaciones con la base de datos
//el OrderService le pide que guarde, busque o actualice ordenes
//y el Repository se encarga de hablar con SQLite
using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;

namespace Orders.API.Data
{
    public class OrderRepository
    {
        //guarda la cadena de conexion para poder conectarse a la base de datos
        private readonly string _connectionString;

        //cuando OrderRepository arranca, recibe la configuracion de la app automaticamente
        //y busca la cadena de conexion en appsettings.json
        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=orders.db";
        }

        //crea una conexion nueva a la base de datos
        //se usa en cada metodo para abrir y cerrar la conexion automaticamente
        private SqliteConnection CreateConnection() => new(_connectionString);

        //METODO 1: ObtenerTodas
        //busca todas las ordenes en la base de datos
        //si viene un usuarioId filtra por ese usuario
        //si no viene ningun usuarioId devuelve todas las ordenes
        public async Task<IEnumerable<Order>> ObtenerTodas(Guid? usuarioId)
        {
            using var conn = CreateConnection();

            //busca las ordenes en la tabla orders
            var ordenes = usuarioId.HasValue
                ? await conn.QueryAsync<Order>(
                    "SELECT * FROM orders WHERE usuarioId = @UsuarioId",
                    new { UsuarioId = usuarioId.Value.ToString() })
                : await conn.QueryAsync<Order>("SELECT * FROM orders");

            //por cada orden busca sus items en la tabla order_items
            foreach (var orden in ordenes)
            {
                orden.Items = (await conn.QueryAsync<OrderItem>(
                    "SELECT * FROM order_items WHERE orderId = @OrderId",
                    new { OrderId = orden.Id.ToString() })).ToList();
            }

            return ordenes;
        }

        //METODO 2: ObtenerPorId
        //busca una orden especifica por su id
        //si no existe devuelve null
        public async Task<Order?> ObtenerPorId(Guid id)
        {
            using var conn = CreateConnection();

            //busca la orden en la tabla orders
            var orden = await conn.QueryFirstOrDefaultAsync<Order>(
                "SELECT * FROM orders WHERE id = @Id",
                new { Id = id.ToString() });

            if (orden == null) return null;

            //busca los items de esa orden en la tabla order_items
            orden.Items = (await conn.QueryAsync<OrderItem>(
                "SELECT * FROM order_items WHERE orderId = @OrderId",
                new { OrderId = id.ToString() })).ToList();

            return orden;
        }

        //METODO 3: Guardar
        //guarda una orden nueva en la base de datos
        //primero guarda la orden en la tabla orders
        //despues guarda cada item en la tabla order_items
        public async Task Guardar(Order orden)
        {
            using var conn = CreateConnection();

            //guarda la orden en la tabla orders
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

            //guarda cada item de la orden en la tabla order_items
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

        //METODO 4: ActualizarEstado
        //actualiza el estado de una orden en la base de datos
        public async Task ActualizarEstado(Guid id, string nuevoEstado)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync(
                "UPDATE orders SET estado = @Estado WHERE id = @Id",
                new { Estado = nuevoEstado, Id = id.ToString() });
        }
    }
}