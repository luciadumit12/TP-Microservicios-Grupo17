using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationsController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("send")]
        public IActionResult Send([FromBody] SendNotificationRequest request)
        {
            var notificacion = _notificationService.EnviarNotificacion(request);
            return StatusCode(201, notificacion);
        }

        [HttpGet("{userId}")]
        public IActionResult GetByUser(Guid userId)
        {
            var notificaciones = _notificationService.ObtenerPorUsuario(userId);
            return Ok(notificaciones);
        }
    }
}
