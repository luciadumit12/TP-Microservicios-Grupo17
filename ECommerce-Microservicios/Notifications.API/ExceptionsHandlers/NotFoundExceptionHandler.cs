// Captura las NotFoundException y arma la respuesta HTTP 404
// con el formato de error que pide el TP (errorCode, errorMessage, etc.)
// Maneja dos casos de Notifications.API
// NTF-001 → cuando el usuario no existe en Users.API
// NTF-003 → cuando el usuario no tiene notificaciones registradas

using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            // Si la excepción NO es NotFoundException, no la manejamos acá
            // Devolvemos false para que el sistema pruebe con el siguiente handler
            if (exception is not NotFoundException ex) return false;

            // Armamos la respuesta 404 con el formato exacto que pide el TP
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "El recurso solicitado no fue encontrado.",
                instance = context.Request.Path.Value,  // URL donde ocurrió el error
                errorCode = ex.ErrorCode,               // NTF-001 o NTF-003
                errorMessage = ex.Message               // Mensaje del catálogo del TP
            }, cancellationToken);

            // Devolvemos true para avisarle al sistema que este handler manejó el error
            return true;
        }
    }
}