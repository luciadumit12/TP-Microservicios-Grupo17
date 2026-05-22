// UserRepository.cs — Maneja todas las operaciones con la base de datos
// El Service usa este repositorio en lugar de la lista en memoria
// Cada método abre una conexión, ejecuta la query y la cierra automáticamente

using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Busca un usuario por email — usado en Register para verificar duplicados
        // y en Login para encontrar el usuario
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = new SqliteConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync(
                "SELECT * FROM users WHERE email = @Email",
                new { Email = email });

            if (result is null) return null;

            return new User
            {
                Id = Guid.Parse((string)result.id),
                Nombre = (string)result.nombre,
                Apellido = (string)result.apellido,
                Email = (string)result.email,
                PasswordHash = (string)result.passwordHash,
                FechaRegistro = DateTime.Parse((string)result.fechaRegistro),
                Activo = (long)result.activo == 1,
                IntentosFallidos = (int)(long)result.intentosFallidos
            };
        }

        // Busca un usuario por Id — usado por el endpoint interno GET /api/users/{id}
        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync(
                "SELECT * FROM users WHERE id = @Id",
                new { Id = id.ToString() });

            if (result is null) return null;

            return new User
            {
                Id = Guid.Parse((string)result.id),
                Nombre = (string)result.nombre,
                Apellido = (string)result.apellido,
                Email = (string)result.email,
                PasswordHash = (string)result.passwordHash,
                FechaRegistro = DateTime.Parse((string)result.fechaRegistro),
                Activo = (long)result.activo == 1,
                IntentosFallidos = (int)(long)result.intentosFallidos
            };
        }

        // Guarda un usuario nuevo en la base de datos
        public async Task InsertAsync(User user)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("""
                INSERT INTO users (id, nombre, apellido, email, passwordHash, fechaRegistro, activo, intentosFallidos)
                VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash, @FechaRegistro, @Activo, @IntentosFallidos)
                """,
                new
                {
                    Id = user.Id.ToString(),
                    user.Nombre,
                    user.Apellido,
                    user.Email,
                    user.PasswordHash,
                    FechaRegistro = user.FechaRegistro.ToString("o"),
                    Activo = user.Activo ? 1 : 0,
                    user.IntentosFallidos
                });
        }

        // Actualiza el estado y los intentos fallidos de un usuario
        // Se usa en Login cuando falla la contraseña o cuando se bloquea el usuario
        public async Task UpdateAsync(User user)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("""
                UPDATE users
                SET activo = @Activo, intentosFallidos = @IntentosFallidos
                WHERE id = @Id
                """,
                new
                {
                    Id = user.Id.ToString(),
                    Activo = user.Activo ? 1 : 0,
                    user.IntentosFallidos
                });
        }
    }
}