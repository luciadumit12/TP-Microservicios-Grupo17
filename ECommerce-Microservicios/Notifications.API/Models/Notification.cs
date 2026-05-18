//Los Models son la representacion de como existen los datos dentro del sistema
//No tienen logica, solo campos
//Es lo que se guarda en la base de datos
//Es distinto al DTO porque el DTO es lo que ve el cliente
//y el Model es lo que guarda el sistema internamente

//NOTIFICATION.CS
//representa una notificacion dentro del sistema
//cuando el NotificationService crea una notificacion, crea un objeto basado en esta clase
namespace Notifications.API.Models
{
    public class Notification
    {
        //id unico de la notificacion, lo genera el sistema automaticamente
        public Guid Id { get; set; }
        //id del usuario que recibio la notificacion
        public Guid UsuarioId { get; set; }
        //el texto de la notificacion
        public string Mensaje { get; set; } = string.Empty;
        //el canal por donde se envio la notificacion
        //solo puede ser Email, Push o SMS
        public string Tipo { get; set; } = string.Empty;
        //el estado actual de la notificacion
        //arranca siempre en Pendiente cuando se crea
        //puede cambiar a Enviada o Fallida
        public string Estado { get; set; } = "Pendiente";
        //fecha y hora en que se envio la notificacion, la asigna el sistema automaticamente
        public DateTime FechaEnvio { get; set; }
    }
}