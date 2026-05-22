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

        /// <summary>
        /// Registrar y simular el envío de una notificación a un usuario.
        /// </summary>
        /// <remarks>
        /// Ejemplo de request:
        ///
        ///     POST /api/notifications/send
        ///     {
        ///         "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///         "mensaje": "Su orden #f1e2d3c4 fue confirmada.",
        ///         "tipo": "Email"
        ///     }
        ///
        /// </remarks>
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

        /// <summary>
        /// Listar todas las notificaciones de un usuario.
        /// </summary>
        //el async/await significa que espera que el Service busque las notificaciones en la base de datos
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