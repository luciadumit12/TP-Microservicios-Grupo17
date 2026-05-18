//BUSINESSRULEEXCEPTION.CS
//se lanza cuando se viola una regla del negocio
//el BusinessRuleExceptionHandler la atrapa y devuelve 400
namespace Notifications.API.Exceptions
{
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}