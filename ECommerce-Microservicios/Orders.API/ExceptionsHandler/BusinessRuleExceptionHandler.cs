//BUSINESSRULEEXCEPTIONHANDLER.CS
//atrapa la BusinessRuleException que lanza el OrderService
//por ej cuando se intenta cambiar el estado de una orden de Entregada a Pendiente
//o cuando no hay stock suficiente
//arma la respuesta 409 o 422 segun el codigo del error
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
            //para cualquier otro codigo de BusinessRuleException devuelve 409
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
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message
            }, cancellationToken);

            return true;
        }
    }
}