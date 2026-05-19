// Excepción que se lanza cuando el usuario está bloqueado e intenta loguearse.
// Puede ser por dos razones:
// USR-004 → bloqueado por acumular 3 intentos fallidos consecutivos
// USR-005 → bloqueado manualmente por detección de fraude
// El ExceptionHandler la captura y arma la respuesta HTTP 403.

namespace Users.API.Exceptions
{
    public class ForbiddenException : Exception
    {
        // Código de error del catálogo del TP (ej: "USR-004" o "USR-005")
        public string ErrorCode { get; }

        public ForbiddenException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}