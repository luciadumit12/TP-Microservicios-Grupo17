using Microsoft.Data.Sqlite;

namespace Cart.API.Data
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=cart.db";
        }

        public void Initialize()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = """
                CREATE TABLE IF NOT EXISTS carts (
                    usuarioId TEXT PRIMARY KEY,
                    fechaActualizacion TEXT NOT NULL
                );
            """;

            command.ExecuteNonQuery();

            command.CommandText = """
                CREATE TABLE IF NOT EXISTS cart_items (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    usuarioId TEXT NOT NULL,
                    productoId TEXT NOT NULL,
                    cantidad INTEGER NOT NULL
                );
            """;

            command.ExecuteNonQuery();
        }
    }
}