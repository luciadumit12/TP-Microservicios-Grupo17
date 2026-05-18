//BUSINESSRULEEXCEPTION.CS
//se lanza cuando se viola una regla del negocio
//por ej cuando se intenta cambiar el estado de una orden de Entregada a Pendiente
//el BusinessRuleExceptionHandler la atrapa y devuelve 409 con el codigo ORD-006
//o cuando no hay stock suficiente, devuelve 422 con el codigo ORD-005
namespace Orders.API.Exceptions
{
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}