//NOTFOUNDEXCEPTIONHANDLER.CS
//atrapa la NotFoundException que lanza el NotificationService
//por ej cuando el usuario no existe en Users.API o cuando no tiene notificaciones
//arma la respuesta 404 con el formato del TP
using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
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
                //la URL donde ocurrio el error, por ej /api/notifications/a1b2c3d4
                instance = context.Request.Path.Value,
                //el codigo del catalogo del TP, por ej NTF-001 o NTF-003
                errorCode = ex.ErrorCode,
                //el mensaje que se definio cuando se lanzo la excepcion
                errorMessage = ex.Message
            }, cancellationToken);

            //devuelve true para avisarle al sistema que este handler manejo el error
            return true;
        }
    }
}