// Captura CUALQUIER excepción que no fue manejada por los otros handlers.
// Es la "red de seguridad" — si algo inesperado falla, este handler lo atrapa.
// Siempre devuelve HTTP 500 con errorCode USR-006.
// IMPORTANTE: loggea el error completo internamente pero NO lo expone al cliente
// (para no filtrar información sensible del sistema).

using Microsoft.AspNetCore.Diagnostics;

namespace Users.API.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        // ILogger para registrar el error completo en los logs de Serilog
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Logueamos el error completo internamente — nunca lo exponemos al cliente
            _logger.LogError(exception, "Error inesperado: {Message}", exception.Message);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrió un error inesperado.",
                instance = httpContext.Request.Path.Value,
                errorCode = "USR-006",
                errorMessage = "Error interno al procesar el usuario."
            }, cancellationToken);

            return true;
        }
    }
}