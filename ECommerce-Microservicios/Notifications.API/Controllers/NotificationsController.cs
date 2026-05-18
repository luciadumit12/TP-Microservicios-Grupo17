//El Controller es la puerta de entrada de Notifications.API
//Cuando llega una llamada HTTP, el Controller la recibe y se la pasa al NotificationService
//El Controller no decide nada, solo recibe y delega

//nombres de las carpetas de las clases que se nombran en este archivo
using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers
{
    //esta clase es el Controller de Notifications.API
    //ACA SE DEFINEN LOS 2 ENDPOINTS DE LA API
    //[ApiController] le dice a .NET que esta clase recibe llamadas HTTP
    //[Route("api/notifications")] define la URL base de todos los endpoints de esta clase
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        //variable que guarda el NotificationService para poder usarlo en todos los metodos
        private readonly NotificationService _notificationService;

        //cuando el Controller arranca, .NET le entrega el NotificationService automaticamente
        //gracias a que lo registramos en Program.cs con AddScoped<NotificationService>()
        public NotificationsController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        //ENDPOINT 1: POST /api/notifications/send
        //recibe una llamada POST con los datos de la notificacion en el body
        //le pide al NotificationService que verifique el usuario y cree la notificacion
        //devuelve 201 con la notificacion creada
        //el async/await significa que espera la respuesta de Users.API antes de continuar
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
        {
            var notificacion = await _notificationService.EnviarNotificacion(request);
            return StatusCode(201, notificacion);
        }

        //ENDPOINT 2: GET /api/notifications/{userId}
        //recibe una llamada GET con el id del usuario en la URL
        //por ej GET /api/notifications/a1b2c3d4-...
        //le pide al NotificationService todas las notificaciones de ese usuario
        //si el usuario no tiene notificaciones el Service lanza NotFoundException con NTF-003
        //si tiene notificaciones devuelve 200 con la lista
        [HttpGet("{userId}")]
        public IActionResult GetByUser(Guid userId)
        {
            var notificaciones = _notificationService.ObtenerPorUsuario(userId);
            return Ok(notificaciones);
        }
    }
}