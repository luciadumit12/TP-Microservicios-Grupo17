// Excepción que se lanza cuando se viola una regla de negocio.
// Por ejemplo: cuando el usuario está bloqueado e intenta loguearse (USR-004),
// o cuando se intenta registrar un email que ya existe (USR-001).
// El ExceptionHandler la captura y arma la respuesta HTTP 400 o 409.

namespace Users.API.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public string ErrorCode { get; }

        public BusinessRuleException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}