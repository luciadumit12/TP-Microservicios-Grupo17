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

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserRequest request)
        {
            var response = _userService.Register(request);
            return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserRequest request)
        {
            var response = _userService.Login(request);
            return Ok(response);
        }

        // Endpoint interno para verificación entre microservicios
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var response = _userService.GetById(id);
            return Ok(response);
        }
    }
}