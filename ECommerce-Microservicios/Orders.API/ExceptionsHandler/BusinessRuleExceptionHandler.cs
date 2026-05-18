//BUSINESSRULEEXCEPTIONHANDLER.CS
//atrapa la BusinessRuleException que lanza el OrderService
//se activa en 2 casos segun el catalogo del TP:
//ORD-005 → cuando la cantidad solicitada supera el stock disponible → devuelve 422
//ORD-006 → cuando se intenta cambiar el estado de una orden a uno que no es valido → devuelve 409
//por ej pasar una orden de Entregada a Pendiente
using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            //verifica si el error que llego es una BusinessRuleException
            //si no lo es, devuelve false y el sistema prueba con el siguiente handler
            if (exception is not BusinessRuleException ex) return false;

            //si el codigo es ORD-005 (stock insuficiente) devuelve 422
            //para ORD-006 (transicion de estado invalida) devuelve 409
            var status = ex.ErrorCode == "ORD-005" ? 422 : 409;

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new
            {
                type = status == 422
                    ? "https://tools.ietf.org/html/rfc4918#section-11.2"
                    : "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                title = status == 422 ? "Unprocessable Entity" : "Conflict",
                status,
                detail = status == 422
                    ? "No se puede procesar la solicitud."
                    : "No se puede modificar el estado.",
                //la URL donde ocurrio el error, por ej /api/orders o /api/orders/00000000/status
                instance = context.Request.Path.Value,
                //el codigo del catalogo del TP → ORD-005 o ORD-006
                errorCode = ex.ErrorCode,
                //el mensaje que se definio cuando se lanzo la excepcion
                //por ej "Stock insuficiente para Notebook Dell XPS 15. Disponible: 2, solicitado: 5."
                //o "Una orden en estado Entregada no puede cambiar a Pendiente."
                errorMessage = ex.Message
            }, cancellationToken);

            //devuelve true para avisarle al sistema que este handler manejo el error
            return true;
        }
    }
}