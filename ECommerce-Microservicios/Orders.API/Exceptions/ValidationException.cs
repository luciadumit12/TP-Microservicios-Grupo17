//.VALIDATIONEXCEPTION.CS
//se lanza cuando los datos que mando el cliente estan mal
//por ej cuando se crea una orden sin items
//el handler la atrapa y devuelve 400 con el codigo ORD-002
namespace Orders.API.Exceptions
{
    public class ValidationException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}