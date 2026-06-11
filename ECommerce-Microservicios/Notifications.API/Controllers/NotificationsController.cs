// El Controller es la puerta de entrada de Notifications.API.
// Recibe los requests HTTP, llama al Service y devuelve la respuesta.
// NO tiene lógica de negocio — solo delega al Service.
using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Tags("Notifications")]
    public class NotificationsController : ControllerBase
    {
        // El Service se inyecta — el Controller no lo crea, lo recibe
        private readonly NotificationService _notificationService;

        public NotificationsController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        //ENDPOINT 1: POST /api/notifications/send
        /// <summary>Registrar y simular el envio de una notificacion a un usuario</summary>
        /// <param name="request">Datos de la notificacion: usuarioId, mensaje y tipo</param>
        /// <response code="201">Notificacion creada y enviada exitosamente</response>
        /// <response code="400">Datos invalidos, por ej mensaje vacio o tipo no reconocido (NTF-002)</response>
        /// <response code="404">Usuario no encontrado en Users.API (NTF-001)</response>
        /// <response code="500">Error interno del servidor (NTF-004)</response>
        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
        {
            var notificacion = await _notificationService.EnviarNotificacion(request);
            return StatusCode(201, notificacion);
        }

        //ENDPOINT 2: GET /api/notifications/{userId}
        /// <summary>Listar todas las notificaciones de un usuario</summary>
        /// <param name="userId">ID del usuario cuyas notificaciones se quieren obtener</param>
        /// <response code="200">Lista de notificaciones del usuario</response>
        /// <response code="404">No se encontraron notificaciones para ese usuario (NTF-003)</response>
        /// <response code="500">Error interno del servidor (NTF-004)</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var notificaciones = await _notificationService.ObtenerPorUsuario(userId);
            return Ok(notificaciones);
        }
    }
}