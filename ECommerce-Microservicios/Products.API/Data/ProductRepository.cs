// ProductRepository es el encargado de todas las operaciones con la base de datos
// el ProductService le pide que guarde, busque, actualice o elimine productos
// y el Repository se encarga de hablar con SQLite
using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.Models;

namespace Products.API.Data
{
    public class ProductRepository
    {
        // guarda la cadena de conexion para poder conectarse a la base de datos
        private readonly string _connectionString;

        // cuando ProductRepository arranca, recibe la configuracion de la app automaticamente
        // y busca la cadena de conexion en appsettings.json
        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=products.db";
        }

        // crea una conexion nueva a la base de datos
        // se usa en cada metodo para abrir y cerrar la conexion automaticamente
        private SqliteConnection CreateConnection() => new(_connectionString);

        // METODO 1: ObtenerTodos
        // busca todos los productos en la base de datos
        // si viene un filtro de categoria o nombre, filtra por esos valores
        public async Task<IEnumerable<Product>> ObtenerTodos(string? categoria, string? nombre)
        {
            using var conn = CreateConnection();

            var sql = "SELECT * FROM products WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(categoria))
                sql += " AND categoria = @Categoria";

            if (!string.IsNullOrWhiteSpace(nombre))
                sql += " AND nombre LIKE @Nombre";

            var result = await conn.QueryAsync(sql, new
            {
                Categoria = categoria,
                Nombre = $"%{nombre}%"
            });

            return result.Select(p => new Product
            {
                Id = Guid.Parse(p.id),
                Nombre = p.nombre,
                Descripcion = p.descripcion,
                Precio = (decimal)p.precio,
                Stock = (int)p.stock,
                Categoria = p.categoria,
                FechaCreacion = DateTime.Parse(p.fechaCreacion)
            });
        }

        // METODO 2: ObtenerPorId
        // busca un producto especifico por su id
        // si no existe devuelve null
        public async Task<Product?> ObtenerPorId(Guid id)
        {
            using var conn = CreateConnection();

            var result = await conn.QueryFirstOrDefaultAsync(
                "SELECT * FROM products WHERE id = @Id",
                new { Id = id.ToString() });

            if (result == null)
                return null;

            return new Product
            {
                Id = Guid.Parse(result.id),
                Nombre = result.nombre,
                Descripcion = result.descripcion,
                Precio = (decimal)result.precio,
                Stock = (int)result.stock,
                Categoria = result.categoria,
                FechaCreacion = DateTime.Parse(result.fechaCreacion)
            };
        }

        // METODO 3: ExistePorNombreYCategoria
        // verifica si ya existe un producto con ese nombre en esa categoria
        // se usa en el Service para validar duplicados antes de crear un producto
        public async Task<bool> ExistePorNombreYCategoria(string nombre, string categoria, Guid? excludeId = null)
        {
            using var conn = CreateConnection();

            var sql = "SELECT COUNT(*) FROM products WHERE nombre = @Nombre AND categoria = @Categoria";

            if (excludeId.HasValue)
                sql += " AND id != @ExcludeId";

            var count = await conn.ExecuteScalarAsync<int>(sql, new
            {
                Nombre = nombre,
                Categoria = categoria,
                ExcludeId = excludeId?.ToString()
            });

            return count > 0;
        }

        // METODO 4: Guardar
        // guarda un producto nuevo en la base de datos
        public async Task Guardar(Product product)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync("""
                INSERT INTO products (id, nombre, descripcion, precio, stock, categoria, fechaCreacion)
                VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion)
            """, new
            {
                Id = product.Id.ToString(),
                product.Nombre,
                product.Descripcion,
                product.Precio,
                product.Stock,
                product.Categoria,
                FechaCreacion = product.FechaCreacion.ToString("o")
            });
        }

        // METODO 5: Actualizar
        // actualiza los campos de un producto existente en la base de datos
        public async Task Actualizar(Product product)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync("""
                UPDATE products
                SET nombre = @Nombre,
                    descripcion = @Descripcion,
                    precio = @Precio,
                    stock = @Stock,
                    categoria = @Categoria
                WHERE id = @Id
            """, new
            {
                Id = product.Id.ToString(),
                product.Nombre,
                product.Descripcion,
                product.Precio,
                product.Stock,
                product.Categoria
            });
        }

        // METODO 6: Eliminar
        // elimina un producto de la base de datos por su id
        public async Task Eliminar(Guid id)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync(
                "DELETE FROM products WHERE id = @Id",
                new { Id = id.ToString() });
        }
    }
}