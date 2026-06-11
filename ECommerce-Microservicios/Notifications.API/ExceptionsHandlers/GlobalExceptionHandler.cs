// Atrapa cualquier error inesperado que los otros handlers no pudieron manejar
// se activa para NTF-004 → error inesperado en el servicio o la persistencia
// siempre devuelve 500 porque si llego hasta aca es un error que el sistema no esperaba

using Microsoft.AspNetCore.Diagnostics;

namespace Notifications.API.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            // leemos el Correlation ID desde Items donde lo guardó el middleware
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "";

            // loggeamos como Error porque es un error inesperado
            Serilog.Log.Error(exception, "Error NTF-004 - CorrelationId {CorrelationId}: {Message}",
                correlationId, exception.Message);

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrio un error inesperado.",
                instance = context.Request.Path.Value,
                errorCode = "NTF-004",
                errorMessage = "Error interno al procesar la notificacion.",
                correlationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}