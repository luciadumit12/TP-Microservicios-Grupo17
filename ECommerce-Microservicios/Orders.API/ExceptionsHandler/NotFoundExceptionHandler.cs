//NOTFOUNDEXCEPTIONHANDLER.CS
//atrapa la NotFoundException que lanza el OrderService
//se activa en 3 casos segun el catalogo del TP:
//ORD-001 → cuando se busca una orden que no existe
//ORD-003 → cuando el usuario no existe en Users.API (cuando se conecte)
//ORD-004 → cuando el producto no existe en Products.API (cuando se conecte)
//en todos los casos devuelve 404
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
                //el codigo del catalogo del TP que se definio cuando se lanzo la excepcion
                //por ej ORD-001, ORD-003 o ORD-004
                errorCode = ex.ErrorCode,
                //el mensaje que se definio cuando se lanzo la excepcion
                //por ej "Orden no encontrada."
                errorMessage = ex.Message
            }, cancellationToken);

            //devuelve true para avisarle al sistema que este handler manejo el error
            return true;
        }
    }
}