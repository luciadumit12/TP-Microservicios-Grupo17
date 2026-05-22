//.VALIDATIONEXCEPTIONHANDLER.CS
//atrapa la ValidationException que lanza el OrderService
//se activa en este caso segun el catalogo del TP:
//ORD-002 → cuando se intenta crear una orden sin items o con datos faltantes
//por ej cuando el cliente manda un POST /api/orders sin items en el body
//devuelve 400
using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    public class ValidationExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            //verifica si el error que llego es una ValidationException
            //si no lo es, devuelve false y el sistema prueba con el siguiente handler
            if (exception is not ValidationException ex) return false;

            //si es una ValidationException, arma la respuesta 400 con el formato del TP
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Los datos enviados son invalidos.",
                //la URL donde ocurrio el error, por ej /api/orders
                instance = context.Request.Path.Value,
                //el codigo del catalogo del TP → ORD-002
                errorCode = ex.ErrorCode,
                //el mensaje que se definio cuando se lanzo la excepcion
                //por ej "Los datos de la orden son invalidos."
                errorMessage = ex.Message
            }, cancellationToken);

            //devuelve true para avisarle al sistema que este handler manejo el error
            return true;
        }
    }
}