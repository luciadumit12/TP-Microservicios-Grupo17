// Excepción que se lanza cuando se viola una regla de negocio.
// En Notifications.API se usa cuando los datos de la notificación son inválidos.
// NTF-002 → mensaje vacío o tipo no reconocido (Email | Push | SMS)
// El BusinessRuleExceptionHandler la captura y arma la respuesta HTTP 400.

namespace Notifications.API.Exceptions
{
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        // Código de error del catálogo del TP (ej: "NTF-002")
        public string ErrorCode { get; } = errorCode;
    }
}