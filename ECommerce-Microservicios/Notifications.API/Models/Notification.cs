namespace Notifications.API.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;    // Email | Push | SMS
        public string Estado { get; set; } = "Pendiente";   // Pendiente | Enviada | Fallida
        public DateTime FechaEnvio { get; set; }
    }
}
