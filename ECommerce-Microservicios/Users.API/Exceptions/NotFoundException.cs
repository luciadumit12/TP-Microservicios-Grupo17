// Excepción que se lanza cuando no se encuentra un recurso en el sistema.
// Por ejemplo: cuando se intenta loguear con un email que no existe.
// El ExceptionHandler la captura y arma la respuesta HTTP 404 con el errorCode.

namespace Users.API.Exceptions
{
    public class NotFoundException : Exception
    {
        // Código de error del catálogo del TP (ej: "USR-003")
        public string ErrorCode { get; }

        // Al crear la excepción se pasa el código y el mensaje de error
        public NotFoundException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}