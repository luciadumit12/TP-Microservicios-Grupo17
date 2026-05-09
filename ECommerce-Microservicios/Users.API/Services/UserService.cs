// El Service contiene toda la lógica de negocio de Users.
// El Controller no piensa — solo recibe el request y llama al Service.
// El Service es quien valida, procesa y devuelve el resultado.

using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services
{
    public class UserService
    {
        // Lista en memoria que simula la base de datos
        // La cátedra va a proveer la librería de persistencia real
        private readonly List<User> _users = [];

        // ─────────────────────────────
        // REGISTRAR USUARIO
        // ─────────────────────────────
        public UserResponse Register(RegisterUserRequest request)
        {
            // Verificar que el email no esté ya registrado
            // Si existe, lanzamos BusinessRuleException con código USR-001
            if (_users.Any(u => u.Email == request.Email))
                throw new BusinessRuleException("USR-001", "El email ya está registrado.");

            // Creamos el nuevo usuario
            var user = new User
            {
                Id = Guid.NewGuid(),                    // Generamos el Id automáticamente
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                PasswordHash = request.Password,        // Por ahora guardamos sin hashear
                FechaRegistro = DateTime.UtcNow,        // Fecha actual automática
                Activo = true,                          // El usuario arranca activo
                IntentosFallidos = 0                    // Sin intentos fallidos
            };

            _users.Add(user);

            // Devolvemos UserResponse — NUNCA devolvemos el User directamente
            // porque User tiene PasswordHash que no se debe exponer
            return ToResponse(user);
        }

        // ─────────────────────────────
        // LOGIN
        // ─────────────────────────────
        public UserResponse Login(LoginUserRequest request)
        {
            // Buscar el usuario por email
            var user = _users.FirstOrDefault(u => u.Email == request.Email);

            // Si no existe, lanzamos NotFoundException con código USR-003
            if (user is null)
                throw new NotFoundException("USR-003", "El usuario no fue encontrado.");

            // Si el usuario está bloqueado, lanzamos BusinessRuleException con código USR-004
            if (!user.Activo)
                throw new BusinessRuleException("USR-004", "El usuario está bloqueado.");

            // Verificar contraseña
            if (user.PasswordHash != request.Password)
            {
                // Incrementar intentos fallidos
                user.IntentosFallidos++;

                // Si llegó a 3 intentos fallidos, bloqueamos el usuario
                if (user.IntentosFallidos >= 3)
                {
                    user.Activo = false;
                    throw new BusinessRuleException("USR-004", "Usuario bloqueado por demasiados intentos fallidos.");
                }

                throw new BusinessRuleException("USR-002", "Credenciales inválidas.");
            }

            // Login exitoso — reseteamos los intentos fallidos
            user.IntentosFallidos = 0;

            return ToResponse(user);
        }

        // ─────────────────────────────
        // MÉTODO PRIVADO: convertir User → UserResponse
        // ─────────────────────────────
        // Lo usamos en Register y Login para no repetir código
        private static UserResponse ToResponse(User user) => new()
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email,
            FechaRegistro = user.FechaRegistro,
            Activo = user.Activo
        };
    }
}