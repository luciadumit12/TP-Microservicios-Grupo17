using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services
{
    public class NotificationService
    {
        private readonly List<Notification> _notificaciones = new();

        private readonly List<string> TiposValidos = new() { "Email", "Push", "SMS" };

        public NotificationResponse EnviarNotificacion(SendNotificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Mensaje) || !TiposValidos.Contains(request.Tipo))
                throw new ValidationException("NTF-002", "Los datos de la notificación son inválidos.");

            // Cuando Users.API esté lista, acá se verifica que el usuario exista
            // Si no existe se lanza: throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");

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
