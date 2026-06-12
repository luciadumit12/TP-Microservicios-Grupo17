using Microsoft.AspNetCore.Diagnostics;
using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers
{
    /// <summary>
    /// Maneja excepciones de reglas de negocio.
    /// Devuelve HTTP 422 Unprocessable Entity.
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

            context.Response.StatusCode = 422;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "Unprocessable Entity",
                status = 422,
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