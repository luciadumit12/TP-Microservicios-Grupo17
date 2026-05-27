// El Service contiene toda la lógica de negocio de Users.API.
// El Controller no piensa — solo recibe el request y llama al Service.
// El Service es quien valida, procesa y devuelve el resultado.
// Ahora usa UserRepository para persistir los datos en SQLite.
// Al registrar un usuario, llama automáticamente a Notifications.API para enviar bienvenida.

using Users.API.Data;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services
{
    public class UserService
    {
        private readonly UserRepository _repository;

        // HttpClientFactory para llamar a Notifications.API automáticamente
        private readonly IHttpClientFactory _httpClientFactory;

        public UserService(UserRepository repository, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        // ─────────────────────────────
        // REGISTRAR USUARIO
        // POST /api/users/register → 201 Created
        // ─────────────────────────────
        public async Task<UserResponse> Register(RegisterUserRequest request)
        {
            // USR-002: validar que ningún campo venga vacío → 400 Bad Request
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

            // USR-001: verificar que el email no esté ya registrado → 409 Conflict
            var existente = await _repository.GetByEmailAsync(request.Email);
            if (existente is not null)
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

            await _repository.InsertAsync(user);

            // ─────────────────────────────
            // NOTIFICACIÓN AUTOMÁTICA DE BIENVENIDA
            // Llama a Notifications.API para enviar una notificación al usuario recién registrado
            // Si Notifications.API no está disponible, no falla el registro — es un best-effort
            // ─────────────────────────────
            try
            {
                var client = _httpClientFactory.CreateClient("NotificationsAPI");
                await client.PostAsJsonAsync("api/notifications/send", new
                {
                    usuarioId = user.Id,
                    mensaje = $"Bienvenido/a {user.Nombre}! Tu cuenta fue creada exitosamente.",
                    tipo = "Email"
                });
            }
            catch
            {
                // Si Notifications.API no está disponible, el registro igual es exitoso
                // No propagamos el error para no bloquear el registro del usuario
            }

            return ToResponse(user);
        }

        // ─────────────────────────────
        // LOGIN
        // POST /api/users/login → 200 OK
        // ─────────────────────────────
        public async Task<UserResponse> Login(LoginUserRequest request)
        {
            // USR-002: validar que email y contraseña no vengan vacíos → 400 Bad Request
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

            // Buscar el usuario por email en la base de datos
            var user = await _repository.GetByEmailAsync(request.Email);

            // USR-003: si el email no existe → 401 Unauthorized
            if (user is null)
                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");

            // USR-004: si el usuario está bloqueado por intentos fallidos → 403 Forbidden
            if (!user.Activo && user.IntentosFallidos >= 3)
                throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

            // USR-005: si el usuario fue bloqueado manualmente por fraude → 403 Forbidden
            if (!user.Activo && user.IntentosFallidos < 3)
                throw new ForbiddenException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");

            // Verificar contraseña
            if (user.PasswordHash != request.Password)
            {
                user.IntentosFallidos++;

                if (user.IntentosFallidos >= 3)
                {
                    user.Activo = false;
                    await _repository.UpdateAsync(user);
                    throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
                }

                await _repository.UpdateAsync(user);
                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
            }

            // Login exitoso → resetear intentos fallidos
            user.IntentosFallidos = 0;
            await _repository.UpdateAsync(user);
            return ToResponse(user);
        }

        // ─────────────────────────────
        // GET POR ID — endpoint interno para comunicación entre microservicios
        // ─────────────────────────────
        public async Task<UserResponse> GetById(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user is null)
                throw new NotFoundException("USR-003", "Usuario no encontrado.");
            return ToResponse(user);
        }

        // Convertir User → UserResponse sin exponer PasswordHash
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