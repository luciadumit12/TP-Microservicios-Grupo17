using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services
{
    public class UserService
    {
        private readonly List<User> _users = [];

        public UserResponse Register(RegisterUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

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

        public UserResponse Login(LoginUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

            var user = _users.FirstOrDefault(u => u.Email == request.Email);

            if (user is null)
                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");

            if (!user.Activo && user.IntentosFallidos >= 3)
                throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

            if (!user.Activo && user.IntentosFallidos < 3)
                throw new ForbiddenException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");

            if (user.PasswordHash != request.Password)
            {
                user.IntentosFallidos++;

                if (user.IntentosFallidos >= 3)
                {
                    user.Activo = false;
                    throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
                }

                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
            }

            user.IntentosFallidos = 0;
            return ToResponse(user);
        }

        public UserResponse GetById(Guid id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                throw new NotFoundException("USR-003", "Usuario no encontrado.");
            return ToResponse(user);
        }

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