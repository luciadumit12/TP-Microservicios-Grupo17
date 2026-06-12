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

        /// <summary>Registrar un nuevo usuario en el sistema</summary>
        /// <param name="request">Datos del usuario: nombre, apellido, email y password</param>
        /// <response code="201">Usuario registrado exitosamente</response>
        /// <response code="400">Datos invalidos, por ej campos vacios (USR-002)</response>
        /// <response code="409">El email ya esta registrado (USR-001)</response>
        /// <response code="500">Error interno del servidor (USR-006)</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var response = await _userService.Register(request);
            return StatusCode(201, response);
        }

        /// <summary>Autenticar un usuario existente con email y contrasena</summary>
        /// <param name="request">Credenciales del usuario: email y password</param>
        /// <response code="200">Login exitoso</response>
        /// <response code="400">Datos invalidos, por ej campos vacios (USR-002)</response>
        /// <response code="401">Credenciales incorrectas (USR-003)</response>
        /// <response code="403">Usuario bloqueado (USR-004 o USR-005)</response>
        /// <response code="500">Error interno del servidor (USR-006)</response>
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

        /// <summary>Obtener un usuario por ID. Endpoint interno para comunicacion entre microservicios</summary>
        /// <param name="id">ID unico del usuario</param>
        /// <response code="200">Usuario encontrado exitosamente</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error interno del servidor (USR-006)</response>
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