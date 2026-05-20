// DTO que representa los datos que manda el cliente cuando quiere enviar una notificación.
// Es lo que llega en el BODY del POST /api/notifications/send
// No incluye Id, Estado ni FechaEnvio porque esos los genera el sistema.

namespace Notifications.API.DTOs
{
    public class SendNotificationRequest
    {
        // Id del usuario destinatario — el Service verifica que exista en Users.API
        public Guid UsuarioId { get; set; }

        // Texto de la notificación — requerido, máximo 500 caracteres
        // Ejemplo: "Su orden #f1e2d3c4 fue confirmada."
        public string Mensaje { get; set; } = string.Empty;

        // Canal de envío — requerido, solo acepta: Email | Push | SMS
        // Si viene otro valor el Service lo rechaza con NTF-002
        public string Tipo { get; set; } = string.Empty;
    }
}