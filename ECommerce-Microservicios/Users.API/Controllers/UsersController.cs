// El Controller es la puerta de entrada de la API.
// Recibe los requests HTTP, llama al Service y devuelve la respuesta.
// NO tiene lógica de negocio — solo delega al Service.

using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers
{
    [ApiController]                          // Indica que esta clase es un controller de API
    [Route("api/users")]                     // Todos los endpoints de esta clase empiezan con /api/users
    public class UsersController : ControllerBase
    {
        // El Service se inyecta — el Controller no lo crea, lo recibe
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // ─────────────────────────────
        // POST /api/users/register
        // Registrar un nuevo usuario
        // Respuestas posibles: 201 Created, 400 Bad Request, 409 Conflict
        // ─────────────────────────────
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserRequest request)
        {
            // Llamamos al Service — él se encarga de validar y registrar
            var response = _userService.Register(request);

            // 201 Created + la ubicación del nuevo recurso + el body con los datos
            return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
        }

        // ─────────────────────────────
        // POST /api/users/login
        // Autenticar un usuario existente
        // Respuestas posibles: 200 OK, 400 Bad Request, 401 Unauthorized, 403 Forbidden
        // ─────────────────────────────
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserRequest request)
        {
            // Llamamos al Service — él verifica credenciales y maneja el bloqueo
            var response = _userService.Login(request);

            // 200 OK con los datos del usuario autenticado
            return Ok(response);
        }
    }
}