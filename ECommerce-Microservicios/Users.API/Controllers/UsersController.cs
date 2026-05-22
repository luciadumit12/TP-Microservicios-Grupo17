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
    [Tags("Users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registrar un nuevo usuario en el sistema.
        /// </summary>
        /// <remarks>
        /// Ejemplo de request:
        ///
        ///     POST /api/users/register
        ///     {
        ///         "nombre": "María",
        ///         "apellido": "González",
        ///         "email": "maria@email.com",
        ///         "password": "MiPassword123!"
        ///     }
        ///
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var response = await _userService.Register(request);
            return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
        }

        /// <summary>
        /// Autenticar un usuario existente con email y contraseña.
        /// </summary>
        /// <remarks>
        /// Ejemplo de request:
        ///
        ///     POST /api/users/login
        ///     {
        ///         "email": "maria@email.com",
        ///         "password": "MiPassword123!"
        ///     }
        ///
        /// </remarks>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
        {
            var response = await _userService.Login(request);
            return Ok(response);
        }

        /// <summary>
        /// Obtener un usuario por ID. Endpoint interno para comunicación entre microservicios.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _userService.GetById(id);
            return Ok(response);
        }
    }
}