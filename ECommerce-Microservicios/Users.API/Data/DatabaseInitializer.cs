// DatabaseInitializer.cs — Inicializa la base de datos al arrancar la aplicación
// Crea la tabla users si no existe
// Se llama una sola vez desde Program.cs al arrancar

using Dapper;
using Microsoft.Data.Sqlite;

namespace Users.API.Data
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Crea la tabla users si no existe
            // Guarda todos los campos del modelo User
            connection.Execute("""
                CREATE TABLE IF NOT EXISTS users (
                    id TEXT PRIMARY KEY,
                    nombre TEXT NOT NULL,
                    apellido TEXT NOT NULL,
                    email TEXT NOT NULL UNIQUE,
                    passwordHash TEXT NOT NULL,
                    fechaRegistro TEXT NOT NULL,
                    activo INTEGER NOT NULL DEFAULT 1,
                    intentosFallidos INTEGER NOT NULL DEFAULT 0
                );
                """);
        }
    }
}