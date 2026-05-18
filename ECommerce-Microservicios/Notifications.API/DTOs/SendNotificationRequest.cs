//SENDNOTIFICATIONREQUEST.CS
//lo que el cliente manda cuando hace un POST para enviar una notificacion
//solo incluye lo que el cliente puede informar: a quien va, el mensaje y el tipo
//no incluye Id, Estado ni FechaEnvio porque esos los genera el sistema
namespace Notifications.API.DTOs
{
    public class SendNotificationRequest
    {
        //id del usuario que va a recibir la notificacion
        public Guid UsuarioId { get; set; }
        //el texto de la notificacion, por ej "Su orden #f1e2d3c4 fue confirmada."
        public string Mensaje { get; set; } = string.Empty;
        //el canal por donde se envia la notificacion
        //solo puede ser Email, Push o SMS
        public string Tipo { get; set; } = string.Empty;
    }
}