// Esta clase representa una Notificación dentro del sistema.
// Es la entidad del dominio — cómo se guarda una notificación internamente.
// NUNCA se expone directamente en las respuestas de la API, para eso están los DTOs

namespace Notifications.API.Models
{
    public class Notification
    {
        // Identificador único de la notificación — se genera automáticamente al crear
        public Guid Id { get; set; }

        // Id del usuario destinatario de la notificación
        public Guid UsuarioId { get; set; }

        // Texto de la notificación — requerido, máximo 500 caracteres según el TP
        public string Mensaje { get; set; } = string.Empty;

        // Canal de envío — solo puede ser: Email | Push | SMS
        // Se valida en el Service antes de crear la notificación
        public string Tipo { get; set; } = string.Empty;

        // Estado actual de la notificación
        // Valores posibles: Pendiente | Enviada | Fallida
        // Arranca en "Pendiente" por defecto al crearse
        public string Estado { get; set; } = "Pendiente";

        // Fecha y hora en que se registró la notificación — se asigna automáticamente al crear
        public DateTime FechaEnvio { get; set; }
    }
}