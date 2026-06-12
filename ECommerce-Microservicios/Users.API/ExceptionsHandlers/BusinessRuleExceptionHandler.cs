using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "";

            Serilog.Log.Warning("Error {ErrorCode} - CorrelationId {CorrelationId}: {Message}",
                ex.ErrorCode, correlationId, ex.Message);

            // USR-001 → 409 (email duplicado)
            // USR-002 → 400 (datos inválidos)
            var statusCode = ex.ErrorCode == "USR-001" ? 409 : 400;
            var type = statusCode == 409
                ? "https://tools.ietf.org/html/rfc7231#section-6.5.9"
                : "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            var title = statusCode == 409 ? "Conflict" : "Bad Request";
            var detail = statusCode == 409
                ? "Ya existe un recurso con esos datos."
                : "Los datos enviados son invalidos.";

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new
            {
                type,
                title,
                status = statusCode,
                detail,
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId
            }, cancellationToken);

            return true;
        }
    }
}