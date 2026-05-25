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
        //recibe una llamada POST con los datos de la notificacion en el body
        //le pide al Service que verifique el usuario en Users.API antes de guardar
        //devuelve 201 con la notificacion creada
        //el async/await significa que espera la respuesta de Users.API antes de continuar
        /// <summary>Registrar y simular el envio de una notificacion a un usuario</summary>
        /// <param name="request">Datos de la notificacion: usuarioId, mensaje y tipo</param>
        /// <remarks>
        /// Ejemplo de Exito (201 Created):
        ///
        ///     {
        ///       "id": "de4d9ee3-0f63-45a1-9838-b1ab6d7f417e",
        ///       "usuarioId": "aa863f64-5e21-44ee-9d14-50e4c60e26b2",
        ///       "mensaje": "Su orden fue confirmada.",
        ///       "tipo": "Email",
        ///       "estado": "Enviada",
        ///       "fechaEnvio": "2026-05-24T23:07:52Z"
        ///     }
        ///
        /// Ejemplo de Error (400 - Datos invalidos - NTF-002):
        ///
        ///     {
        ///       "errorCode": "NTF-002",
        ///       "errorMessage": "Los datos de la notificacion son invalidos.",
        ///       "status": 400
        ///     }
        ///
        /// Ejemplo de Error (404 - Usuario no encontrado - NTF-001):
        ///
        ///     {
        ///       "errorCode": "NTF-001",
        ///       "errorMessage": "El usuario destinatario no fue encontrado.",
        ///       "status": 404
        ///     }
        ///
        /// Ejemplo de Error (500 - Error interno - NTF-004):
        ///
        ///     {
        ///       "errorCode": "NTF-004",
        ///       "errorMessage": "Error interno al procesar la notificacion.",
        ///       "status": 500
        ///     }
        ///
        /// </remarks>
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
        //recibe una llamada GET con el id del usuario en la URL
        //le pide al Service todas las notificaciones de ese usuario en la base de datos
        //si no tiene notificaciones lanza NotFoundException con NTF-003
        //el async/await significa que espera que el Service busque en la base de datos
        /// <summary>Listar todas las notificaciones de un usuario</summary>
        /// <param name="userId">ID del usuario cuyas notificaciones se quieren obtener</param>
        /// <remarks>
        /// Ejemplo de Exito (200 OK):
        ///
        ///     [
        ///       {
        ///         "id": "de4d9ee3-0f63-45a1-9838-b1ab6d7f417e",
        ///         "usuarioId": "aa863f64-5e21-44ee-9d14-50e4c60e26b2",
        ///         "mensaje": "Su orden fue confirmada.",
        ///         "tipo": "Email",
        ///         "estado": "Enviada",
        ///         "fechaEnvio": "2026-05-24T23:07:52Z"
        ///       }
        ///     ]
        ///
        /// Ejemplo de Error (404 - Sin notificaciones - NTF-003):
        ///
        ///     {
        ///       "errorCode": "NTF-003",
        ///       "errorMessage": "No se encontraron notificaciones para el usuario.",
        ///       "status": 404
        ///     }
        ///
        /// Ejemplo de Error (500 - Error interno - NTF-004):
        ///
        ///     {
        ///       "errorCode": "NTF-004",
        ///       "errorMessage": "Error interno al procesar la notificacion.",
        ///       "status": 500
        ///     }
        ///
        /// </remarks>
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