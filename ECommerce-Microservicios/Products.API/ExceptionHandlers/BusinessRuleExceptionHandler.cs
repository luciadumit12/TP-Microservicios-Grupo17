using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers
{
    /// <summary>
    /// Maneja excepciones de reglas de negocio.
    /// Devuelve HTTP 409 Conflict.
    /// </summary>
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex)
                return false;

            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "";

            Serilog.Log.Warning("Error {ErrorCode} - CorrelationId {CorrelationId}: {Message}",
                ex.ErrorCode, correlationId, ex.Message);

            context.Response.StatusCode = 409;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                title = "Conflict",
                status = 409,
                detail = "No se puede procesar la solicitud.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}