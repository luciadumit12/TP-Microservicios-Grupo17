// El Controller es la puerta de entrada de la API.
// Recibe los requests HTTP, llama al Service y devuelve la respuesta.
// NO tiene lógica de negocio — solo delega al Service.

using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // ─────────────────────────────
        // POST /api/users/register
        // Respuestas posibles: 201, 400, 409
        // ─────────────────────────────
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserRequest request)
        {
            var response = _userService.Register(request);
            return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
        }

        // ─────────────────────────────
        // POST /api/users/login
        // Respuestas posibles: 200, 400, 401, 403
        // ─────────────────────────────
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserRequest request)
        {
            var response = _userService.Login(request);
            return Ok(response);
        }

        // ─────────────────────────────
        // GET /api/users/{id}
        // Endpoint interno para verificación entre microservicios
        // Usado por Notifications.API para validar que el usuario existe
        // ─────────────────────────────
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var response = _userService.GetById(id);
            return Ok(response);
        }
    }
}