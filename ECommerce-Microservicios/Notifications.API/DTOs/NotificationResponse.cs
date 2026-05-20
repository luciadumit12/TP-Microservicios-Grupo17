// DTO que representa lo que devuelve la API cuando responde con datos de una notificación.
// Es lo que sale en el BODY de las respuestas exitosas.
// Incluye todos los campos porque el cliente necesita ver el estado y la fecha de envío.

namespace Notifications.API.DTOs
{
    public class NotificationResponse
    {
        // Id único de la notificación — generado automáticamente por el sistema
        public Guid Id { get; set; }

        // Id del usuario destinatario
        public Guid UsuarioId { get; set; }

        // Texto de la notificación
        public string Mensaje { get; set; } = string.Empty;

        // Canal de envío: Email | Push | SMS
        public string Tipo { get; set; } = string.Empty;

        // Estado actual: Pendiente | Enviada | Fallida
        public string Estado { get; set; } = string.Empty;

        // Fecha y hora en que se registró la notificación
        public DateTime FechaEnvio { get; set; }
    }
}