// Excepción que se lanza cuando los datos que mandó el cliente son inválidos.
// Por ejemplo: email sin formato válido, campos vacíos, contraseña muy corta.
// El ExceptionHandler la captura y arma la respuesta HTTP 400.

namespace Users.API.Exceptions
{
    public class ValidationException : Exception
    {
        public string ErrorCode { get; }

        public ValidationException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}