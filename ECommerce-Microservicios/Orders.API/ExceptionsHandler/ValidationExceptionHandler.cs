//atrapa la ValidationException que lanza el NotificationService
//se activa para NTF-002 → cuando los datos de la notificacion son invalidos
//por ej cuando el mensaje esta vacio o el tipo no es Email, Push o SMS
//devuelve 400
using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    public class ValidationExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ValidationException ex) return false;

            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Los datos enviados son invalidos.",
                instance = context.Request.Path.Value,
                //el codigo del catalogo del TP → NTF-002
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message
            }, cancellationToken);

            //devuelve true para avisarle al sistema que este handler manejo el error.
            return true;
        }
    }
}