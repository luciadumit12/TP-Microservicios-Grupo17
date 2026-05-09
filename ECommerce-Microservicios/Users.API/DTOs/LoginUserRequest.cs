// Este objeto representa los datos que manda el cliente cuando quiere loguearse.
// Es lo que llega en el BODY del POST /api/users/login
// Solo necesita email y contraseña para autenticarse.

namespace Users.API.DTOs
{
    public class LoginUserRequest
    {
        // Email con el que se registró
        public string Email { get; set; } = string.Empty;

        // Contraseña en texto plano — el Service la va a comparar contra el hash guardado
        public string Password { get; set; } = string.Empty;
    }
}