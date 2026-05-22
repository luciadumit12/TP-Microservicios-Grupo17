//se lanza cuando los datos que mando el cliente estan mal
//en Notifications.API se usa cuando el mensaje esta vacio o el tipo no es Email, Push o SMS
//el ValidationExceptionHandler la atrapa y devuelve 400 con el codigo NTF-002
namespace Notifications.API.Exceptions
{
    public class ValidationException(string errorCode, string message) : Exception(message)
    {
        //guarda el codigo del error para que el Handler lo pueda usar en la respuesta JSON
        public string ErrorCode { get; } = errorCode;
    }
}