//NOTFOUNDEXCEPTIONHANDLER.CS
//atrapa la NotFoundException que lanza el OrderService
//por ej cuando se busca una orden que no existe
//arma la respuesta 404 con el formato del TP
using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            //verifica si el error que llego es una NotFoundException
            //si no lo es, devuelve false y el sistema prueba con el siguiente handler
            if (exception is not NotFoundException ex) return false;

            //si es una NotFoundException, arma la respuesta 404 con el formato del TP
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "El recurso solicitado no fue encontrado.",
                //la URL donde ocurrio el error, por ej /api/orders/00000000
                instance = context.Request.Path.Value,
                //el codigo del catalogo del TP, por ej ORD-001
                errorCode = ex.ErrorCode,
                //el mensaje que se definio cuando se lanzo la excepcion
                errorMessage = ex.Message
            }, cancellationToken);

            //devuelve true para avisarle al sistema que este handler manejo el error
            return true;
        }
    }
}