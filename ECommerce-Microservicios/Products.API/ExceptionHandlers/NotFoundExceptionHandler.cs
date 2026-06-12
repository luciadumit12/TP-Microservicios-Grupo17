using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers
{
    /// <summary>
    /// Maneja excepciones de recursos no encontrados.
    /// Devuelve HTTP 404 Not Found.
    /// </summary>
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not NotFoundException ex)
                return false;

            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "";

            Serilog.Log.Warning("Error {ErrorCode} - CorrelationId {CorrelationId}: {Message}",
                ex.ErrorCode, correlationId, ex.Message);

            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "El recurso solicitado no fue encontrado.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}