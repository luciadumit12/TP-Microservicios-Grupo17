//DatabaseInitializer es el encargado de crear las tablas en la base de datos cuando arranca la app
//si las tablas ya existen no hace nada, solo las crea si no existen
//se ejecuta una sola vez al arrancar la aplicacion desde Program.cs
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data
{
    public class DatabaseInitializer
    {
        //guarda la cadena de conexion para poder conectarse a la base de datos
        //la cadena de conexion viene del appsettings.json → "DefaultConnection": "Data Source=notifications.db"
        private readonly string _connectionString;

        //cuando DatabaseInitializer arranca, recibe la configuracion de la app automaticamente
        //y busca la cadena de conexion en appsettings.json
        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=notifications.db";
        }

        //este metodo crea las tablas en la base de datos si no existen
        //se llama desde Program.cs al arrancar la aplicacion
        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            //crea la tabla notifications si no existe
            //cada notificacion tiene: id, usuarioId, mensaje, tipo, estado y fechaEnvio
            var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS notifications (
                    id TEXT PRIMARY KEY,
                    usuarioId TEXT NOT NULL,
                    mensaje TEXT NOT NULL,
                    tipo TEXT NOT NULL,
                    estado TEXT NOT NULL,
                    fechaEnvio TEXT NOT NULL
                );
            """;
            command.ExecuteNonQuery();
        }
    }
}