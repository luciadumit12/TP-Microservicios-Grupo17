//.DatabaseInitializer es el encargado de crear las tablas en la base de datos cuando arranca la app
//si las tablas ya existen no hace nada, solo las crea si no existen
//se ejecuta una sola vez al arrancar la aplicacion desde Program.cs
using Microsoft.Data.Sqlite;

namespace Orders.API.Data
{
    public class DatabaseInitializer
    {
        //guarda la cadena de conexion para poder conectarse a la base de datos
        //la cadena de conexion viene del appsettings.json → "DefaultConnection": "Data Source=orders.db"
        private readonly string _connectionString;

        //cuando DatabaseInitializer arranca, recibe la configuracion de la app automaticamente
        //y busca la cadena de conexion en appsettings.json
        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=orders.db";
        }

        //este metodo crea las tablas en la base de datos si no existen
        //se llama desde Program.cs al arrancar la aplicacion
        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            //crea la tabla orders si no existe
            //cada orden tiene: id, usuarioId, total, estado y fechaCreacion
            connection.CreateCommand().CommandText = """
                CREATE TABLE IF NOT EXISTS orders (
                    id TEXT PRIMARY KEY,
                    usuarioId TEXT NOT NULL,
                    total REAL NOT NULL DEFAULT 0,
                    estado TEXT NOT NULL DEFAULT 'Pendiente',
                    fechaCreacion TEXT NOT NULL
                );
            """;
            connection.CreateCommand().ExecuteNonQuery();

            //crea la tabla order_items si no existe
            //cada item tiene: id, orderId (referencia a la orden), productoId, cantidad y precioUnitario
            //una orden puede tener muchos items
            var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS orders (
                    id TEXT PRIMARY KEY,
                    usuarioId TEXT NOT NULL,
                    total REAL NOT NULL DEFAULT 0,
                    estado TEXT NOT NULL DEFAULT 'Pendiente',
                    fechaCreacion TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS order_items (
                    id TEXT PRIMARY KEY,
                    orderId TEXT NOT NULL,
                    productoId TEXT NOT NULL,
                    cantidad INTEGER NOT NULL,
                    precioUnitario REAL NOT NULL,
                    FOREIGN KEY (orderId) REFERENCES orders(id)
                );
            """;
            command.ExecuteNonQuery();
        }
    }
}