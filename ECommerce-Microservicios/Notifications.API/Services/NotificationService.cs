using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services
{
    public class NotificationService
    {
        private readonly List<Notification> _notificaciones = new();
        private readonly List<string> _tiposValidos = new() { "Email", "Push", "SMS" };
        private readonly IHttpClientFactory _httpClientFactory;

        // Recibimos el IHttpClientFactory por inyección
        public NotificationService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<NotificationResponse> EnviarNotificacion(SendNotificationRequest request)
        {
            // NTF-002: datos inválidos → 400
            if (string.IsNullOrWhiteSpace(request.Mensaje) ||
                string.IsNullOrWhiteSpace(request.Tipo) ||
                !_tiposValidos.Contains(request.Tipo))
                throw new ValidationException("NTF-002", "Los datos de la notificación son inválidos.");

            // NTF-001: verificar que el usuario existe en Users.API → 404
            var client = _httpClientFactory.CreateClient("UsersAPI");
            var response = await client.GetAsync($"api/users/{request.UsuarioId}");
            if (!response.IsSuccessStatusCode)
                throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");

            var notificacion = new Notification
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Mensaje = request.Mensaje,
                Tipo = request.Tipo,
                Estado = "Enviada",
                FechaEnvio = DateTime.UtcNow
            };

            _notificaciones.Add(notificacion);
            return MapearAResponse(notificacion);
        }

        public List<NotificationResponse> ObtenerPorUsuario(Guid usuarioId)
        {
            var notificaciones = _notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .ToList();

            // NTF-003: no hay notificaciones → 404
            if (notificaciones.Count == 0)
                throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");

            return notificaciones.Select(MapearAResponse).ToList();
        }

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
}