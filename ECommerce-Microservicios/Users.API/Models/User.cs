// Esta clase representa un Usuario dentro del sistema.
// Es la entidad del dominio — cómo se guarda un usuario internamente.
// NUNCA se expone directamente en las respuestas de la API, para eso están los DTOs.

namespace Users.API.Models
{
    public class User
    {
        // Identificador único del usuario, se genera automáticamente al crear
        public Guid Id { get; set; }

        // Nombre del usuario — obligatorio
        public string Nombre { get; set; } = string.Empty;

        // Apellido del usuario — obligatorio
        public string Apellido { get; set; } = string.Empty;

        // Email del usuario — obligatorio, debe ser único en el sistema
        public string Email { get; set; } = string.Empty;

        // Contraseña hasheada — NUNCA se devuelve en ninguna respuesta de la API
        // Se guarda el hash, no la contraseña en texto plano
        public string PasswordHash { get; set; } = string.Empty;

        // Fecha en que se registró el usuario — se asigna automáticamente al crear
        public DateTime FechaRegistro { get; set; }

        // Indica si el usuario está activo o bloqueado
        // true = puede loguearse / false = está bloqueado por intentos fallidos
        public bool Activo { get; set; }

        // Contador de intentos de login fallidos consecutivos
        // Se incrementa cada vez que falla el login
        // Se resetea a 0 cuando el login es exitoso
        // Cuando llega a 3, Activo pasa a false (usuario bloqueado)
        public int IntentosFallidos { get; set; }
    }
}