using Microsoft.AspNetCore.Diagnostics;

namespace Products.API.ExceptionHandlers
{
    /// <summary>
    /// Maneja errores inesperados de la aplicación.
    /// Devuelve HTTP 500 Internal Server Error.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "";

            Serilog.Log.Error(exception, "Error PRD-005 - CorrelationId {CorrelationId}: {Message}",
                correlationId, exception.Message);

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrio un error inesperado.",
                instance = context.Request.Path.Value,
                errorCode = "PRD-005",
                errorMessage = "Error interno al procesar el producto.",
                correlationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}