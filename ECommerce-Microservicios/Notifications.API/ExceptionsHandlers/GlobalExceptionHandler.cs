//GLOBALEXCEPTIONHANDLER.CS
//atrapa cualquier error inesperado que los otros dos handlers no pudieron manejar
//se activa para NTF-004 → error inesperado en el servicio o la persistencia
//siempre devuelve 500 porque si llego hasta aca es un error que el sistema no esperaba
using Microsoft.AspNetCore.Diagnostics;

namespace Notifications.API.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            //no verifica el tipo de error porque atrapa cualquier cosa
            //siempre devuelve 500 con el codigo NTF-004
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrio un error inesperado.",
                //la URL donde ocurrio el error
                instance = context.Request.Path.Value,
                errorCode = "NTF-004",
                errorMessage = "Error interno al procesar la notificacion."
            }, cancellationToken);

            //devuelve true para avisarle al sistema que este handler manejo el error
            return true;
        }
    }
}