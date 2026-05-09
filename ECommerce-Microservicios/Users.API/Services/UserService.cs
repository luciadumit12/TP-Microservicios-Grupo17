using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services
{
    public class UserService
    {
        // Lista en memoria que simula la base de datos
        private readonly List<User> _users = [];

        // ─────────────────────────────
        // REGISTRAR USUARIO
        // ─────────────────────────────
        public UserResponse Register(RegisterUserRequest request)
        {
            // USR-001: email duplicado → 409 Conflict
            if (_users.Any(u => u.Email == request.Email))
                throw new BusinessRuleException("USR-001", $"El email '{request.Email}' ya está registrado.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                PasswordHash = request.Password,
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                IntentosFallidos = 0
            };

            _users.Add(user);
            return ToResponse(user);
        }

        // ─────────────────────────────
        // LOGIN
        // ─────────────────────────────
        public UserResponse Login(LoginUserRequest request)
        {
            // Buscar usuario por email
            var user = _users.FirstOrDefault(u => u.Email == request.Email);

            // USR-003: credenciales incorrectas (email no existe) → 401
            if (user is null)
                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");

            // USR-004: bloqueado por intentos fallidos → 403
            if (!user.Activo && user.IntentosFallidos >= 3)
                throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

            // USR-005: bloqueado por fraude → 403
            if (!user.Activo && user.IntentosFallidos < 3)
                throw new ForbiddenException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");

            // Verificar contraseña
            if (user.PasswordHash != request.Password)
            {
                user.IntentosFallidos++;

                // Si llega a 3 intentos → bloquear
                if (user.IntentosFallidos >= 3)
                {
                    user.Activo = false;
                    throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
                }

                // USR-003: contraseña incorrecta → 401
                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
            }

            // Login exitoso → resetear intentos
            user.IntentosFallidos = 0;
            return ToResponse(user);
        }

        // Convertir User → UserResponse (sin exponer PasswordHash)
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