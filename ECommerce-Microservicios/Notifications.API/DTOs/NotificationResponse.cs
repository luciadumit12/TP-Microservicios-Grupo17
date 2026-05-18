//Los DTOs son los datos que viajan entre el cliente y el Controller
//Notifications.API tiene solo 2 DTOs porque tiene 2 formas de comunicarse con el cliente
//una para lo que el cliente manda y otra para lo que el sistema devuelve

//NOTIFICATIONRESPONSE.CS
//lo que el sistema devuelve al cliente cuando la notificacion se crea exitosamente
//incluye todo: id, usuario, mensaje, tipo, estado y fecha
//es lo que ve el cliente cuando hace un POST exitoso
namespace Notifications.API.DTOs
{
    public class NotificationResponse
    {
        //id unico de la notificacion generado por el sistema
        public Guid Id { get; set; }
        //id del usuario que recibio la notificacion
        public Guid UsuarioId { get; set; }
        //el texto de la notificacion
        public string Mensaje { get; set; } = string.Empty;
        //el canal por donde se envio: Email, Push o SMS
        public string Tipo { get; set; } = string.Empty;
        //el estado de la notificacion: Pendiente, Enviada o Fallida
        public string Estado { get; set; } = string.Empty;
        //fecha y hora en que se envio la notificacion
        public DateTime FechaEnvio { get; set; }
    }
}