// DatabaseInitializer es el encargado de crear las tablas en la base de datos cuando arranca la app
// si las tablas ya existen no hace nada, solo las crea si no existen
// se ejecuta una sola vez al arrancar la aplicacion desde Program.cs
using Microsoft.Data.Sqlite;

namespace Products.API.Data
{
    public class DatabaseInitializer
    {
        // guarda la cadena de conexion para poder conectarse a la base de datos
        // la cadena de conexion viene del appsettings.json → "DefaultConnection": "Data Source=products.db"
        private readonly string _connectionString;

        // cuando DatabaseInitializer arranca, recibe la configuracion de la app automaticamente
        // y busca la cadena de conexion en appsettings.json
        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=products.db";
        }

        // este metodo crea la tabla en la base de datos si no existe
        // se llama desde Program.cs al arrancar la aplicacion
        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // crea la tabla products si no existe
            // cada producto tiene: id, nombre, descripcion, precio, stock, categoria y fechaCreacion
            var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS products (
                    id TEXT PRIMARY KEY,
                    nombre TEXT NOT NULL,
                    descripcion TEXT,
                    precio REAL NOT NULL,
                    stock INTEGER NOT NULL,
                    categoria TEXT NOT NULL,
                    fechaCreacion TEXT NOT NULL
                );
            """;
            command.ExecuteNonQuery();
        }
    }
}