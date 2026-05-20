// El Service contiene toda la lógica de negocio de Notifications.API.
// El Controller no piensa — solo recibe el request y llama al Service.
// A diferencia de Users.API, este Service se conecta con Users.API
// para verificar si el usuario existe antes de crear la notificación.
// IMPORTANTE: por ahora usa una lista en memoria en lugar de base de datos.
// Cuando la cátedra provea la librería de persistencia, se reemplaza _notificaciones por esa librería.

using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services
{
    public class NotificationService
    {
        // Lista en memoria que simula la base de datos
        // Se pierde cuando se reinicia la aplicación — es temporal hasta tener la librería de persistencia
        private readonly List<Notification> _notificaciones = new();

        // Tipos de notificación válidos según el TP
        // Si el cliente manda un tipo que no está en esta lista, se rechaza con NTF-002
        private readonly List<string> _tiposValidos = new() { "Email", "Push", "SMS" };

        // HttpClientFactory para conectarse con Users.API
        // Se registra en Program.cs con AddHttpClient("UsersAPI")
        private readonly IHttpClientFactory _httpClientFactory;

        // .NET inyecta el HttpClientFactory automáticamente
        // gracias a que lo registramos en Program.cs con AddHttpClient("UsersAPI")
        public NotificationService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ─────────────────────────────
        // ENVIAR NOTIFICACIÓN
        // POST /api/notifications/send → 201 Created
        // ─────────────────────────────
        public async Task<NotificationResponse> EnviarNotificacion(SendNotificationRequest request)
        {
            // NTF-002: validar que el mensaje no esté vacío y que el tipo sea válido → 400 Bad Request
            if (string.IsNullOrWhiteSpace(request.Mensaje) ||
                string.IsNullOrWhiteSpace(request.Tipo) ||
                !_tiposValidos.Contains(request.Tipo))
                throw new BusinessRuleException("NTF-002", "Los datos de la notificación son inválidos.");

            // NTF-001: verificar que el usuario existe en Users.API → 404 Not Found
            // Creamos el HttpClient configurado para hablar con Users.API
            var client = _httpClientFactory.CreateClient("UsersAPI");
            var response = await client.GetAsync($"api/users/{request.UsuarioId}");

            // Si Users.API responde que el usuario no existe → NTF-001
            if (!response.IsSuccessStatusCode)
                throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");

            // Leemos la respuesta de Users.API para verificar que el usuario está activo
            // Usamos UserDto — DTO interno que solo usa este Service para leer la respuesta de Users.API
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            if (user is null || !user.Activo)
                throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");

            // Crear la notificación
            var notificacion = new Notification
            {
                Id = Guid.NewGuid(),           // Id generado automáticamente
                UsuarioId = request.UsuarioId,
                Mensaje = request.Mensaje,
                Tipo = request.Tipo,
                Estado = "Enviada",            // Arranca en Enviada porque el usuario existe
                FechaEnvio = DateTime.UtcNow   // Fecha actual automática
            };

            _notificaciones.Add(notificacion);
            return MapearAResponse(notificacion);
        }

        // ─────────────────────────────
        // OBTENER NOTIFICACIONES POR USUARIO
        // GET /api/notifications/{userId} → 200 OK
        // ─────────────────────────────
        public List<NotificationResponse> ObtenerPorUsuario(Guid usuarioId)
        {
            var notificaciones = _notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .ToList();

            // NTF-003: si el usuario no tiene notificaciones → 404 Not Found
            if (notificaciones.Count == 0)
                throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");

            return notificaciones.Select(MapearAResponse).ToList();
        }

        // ─────────────────────────────
        // MÉTODO PRIVADO: convertir Notification → NotificationResponse
        // Se usa en todos los métodos para no repetir código
        // ─────────────────────────────
        private static NotificationResponse MapearAResponse(Notification n) => new()
        {
            Id = n.Id,
            UsuarioId = n.UsuarioId,
            Mensaje = n.Mensaje,
            Tipo = n.Tipo,
            Estado = n.Estado,
            FechaEnvio = n.FechaEnvio
        };
    }

    // ─────────────────────────────
    // DTO INTERNO — solo para uso de NotificationService
    // Lee la respuesta de Users.API cuando consulta si el usuario existe
    // No va en la carpeta DTOs/ porque el cliente nunca lo ve
    // ─────────────────────────────
    public class UserDto
    {
        public Guid Id { get; set; }
        public bool Activo { get; set; }
    }
}