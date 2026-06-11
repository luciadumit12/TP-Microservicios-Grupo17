// Captura las BusinessRuleException y arma la respuesta HTTP 400
// con el formato de error que pide el TP (errorCode, errorMessage, etc.)
// En Notifications.API maneja:
// NTF-002 → cuando los datos de la notificación son inválidos (mensaje vacío o tipo incorrecto)

using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            // Si la excepción NO es BusinessRuleException, no la manejamos acá
            if (exception is not BusinessRuleException ex) return false;

            // leemos el Correlation ID desde Items donde lo guardó el middleware
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "";

            // loggeamos como Warning porque es un error de negocio esperado
            Serilog.Log.Warning("Error {ErrorCode} - CorrelationId {CorrelationId}: {Message}",
                ex.ErrorCode, correlationId, ex.Message);

            // Armamos la respuesta 400 con el formato exacto que pide el TP
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "La solicitud no es valida.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}