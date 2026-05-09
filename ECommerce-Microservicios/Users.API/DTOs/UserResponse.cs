// Este objeto representa lo que devuelve la API cuando responde con datos de un usuario.
// Es lo que sale en el BODY de las respuestas exitosas.
// IMPORTANTE: no incluye PasswordHash — nunca se expone la contraseña, ni hasheada.
// Tampoco incluye IntentosFallidos porque es un dato interno del sistema.

namespace Users.API.DTOs
{
    public class UserResponse
    {
        // Id del usuario recién creado
        public Guid Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        // Fecha en que se registró — la genera el sistema, la devuelve en la respuesta
        public DateTime FechaRegistro { get; set; }

        // Indica si el usuario está activo o bloqueado
        public bool Activo { get; set; }
    }
}