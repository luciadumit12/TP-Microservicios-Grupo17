// DTO = Data Transfer Object
// Este objeto representa los datos que manda el cliente cuando quiere registrarse.
// Es lo que llega en el BODY del POST /api/users/register
// No incluye Id ni FechaRegistro porque esos los genera el sistema, no el cliente.

namespace Users.API.DTOs
{
    public class RegisterUserRequest
    {
        // Nombre que ingresa el usuario al registrarse
        public string Nombre { get; set; } = string.Empty;

        // Apellido que ingresa el usuario al registrarse
        public string Apellido { get; set; } = string.Empty;

        // Email que usará para identificarse — debe ser único en el sistema
        public string Email { get; set; } = string.Empty;

        // Contraseña en texto plano — el Service la va a hashear antes de guardarla
        // Acá se llama Password, no PasswordHash, porque el cliente manda texto plano
        public string Password { get; set; } = string.Empty;
    }
}