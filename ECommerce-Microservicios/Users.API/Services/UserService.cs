// El Service contiene toda la lógica de negocio de Users.API.
// El Controller no piensa — solo recibe el request y llama al Service.
// El Service es quien valida, procesa y devuelve el resultado.
// IMPORTANTE: por ahora usa una lista en memoria en lugar de base de datos.
// Cuando la cátedra provea la librería de persistencia, se reemplaza _users por esa librería.

using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services
{
    public class UserService
    {
        // Lista en memoria que simula la base de datos
        // Se pierde cuando se reinicia la aplicación — es temporal hasta tener la librería de persistencia
        private readonly List<User> _users = [];

        // ─────────────────────────────
        // REGISTRAR USUARIO
        // POST /api/users/register → 201 Created
        // ─────────────────────────────
        public UserResponse Register(RegisterUserRequest request)
        {
            // USR-002: validar que ningún campo venga vacío → 400 Bad Request
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

            // USR-001: verificar que el email no esté ya registrado → 409 Conflict
            if (_users.Any(u => u.Email == request.Email))
                throw new BusinessRuleException("USR-001", $"El email '{request.Email}' ya está registrado.");

            // Crear el nuevo usuario
            var user = new User
            {
                Id = Guid.NewGuid(),             // Id generado automáticamente
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                PasswordHash = request.Password, // Por ahora se guarda sin hashear
                FechaRegistro = DateTime.UtcNow, // Fecha actual automática
                Activo = true,                   // El usuario arranca activo
                IntentosFallidos = 0             // Sin intentos fallidos
            };

            _users.Add(user);

            // Devolvemos UserResponse — NUNCA devolvemos User directamente
            // porque User tiene PasswordHash que no debe exponerse
            return ToResponse(user);
        }

        // ─────────────────────────────
        // LOGIN
        // POST /api/users/login → 200 OK
        // ─────────────────────────────
        public UserResponse Login(LoginUserRequest request)
        {
            // USR-002: validar que email y contraseña no vengan vacíos → 400 Bad Request
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

            // Buscar el usuario por email
            var user = _users.FirstOrDefault(u => u.Email == request.Email);

            // USR-003: si el email no existe en el sistema → 401 Unauthorized
            if (user is null)
                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");

            // USR-004: si el usuario está bloqueado por intentos fallidos → 403 Forbidden
            if (!user.Activo && user.IntentosFallidos >= 3)
                throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

            // USR-005: si el usuario fue bloqueado manualmente por fraude → 403 Forbidden
            if (!user.Activo && user.IntentosFallidos < 3)
                throw new ForbiddenException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");

            // Verificar que la contraseña sea correcta
            if (user.PasswordHash != request.Password)
            {
                // Incrementar el contador de intentos fallidos
                user.IntentosFallidos++;

                // Si llegó a 3 intentos fallidos → bloquear el usuario
                if (user.IntentosFallidos >= 3)
                {
                    user.Activo = false;
                    throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
                }

                // Todavía no llegó a 3 → credenciales incorrectas
                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
            }

            // Login exitoso → resetear el contador de intentos fallidos
            user.IntentosFallidos = 0;
            return ToResponse(user);
        }

        // ─────────────────────────────
        // GET POR ID — endpoint interno para comunicación entre microservicios
        // Lo usa Notifications.API para verificar que el usuario existe antes de enviar una notificación
        // ─────────────────────────────
        public UserResponse GetById(Guid id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                throw new NotFoundException("USR-003", "Usuario no encontrado.");
            return ToResponse(user);
        }

        // ─────────────────────────────
        // MÉTODO PRIVADO: convertir User → UserResponse
        // Se usa en todos los métodos para no repetir código
        // Garantiza que PasswordHash nunca salga en ninguna respuesta
        // ─────────────────────────────
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