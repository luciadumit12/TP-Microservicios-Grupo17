// Captura CUALQUIER excepción inesperada que no fue manejada por los otros handlers.
// Devuelve siempre HTTP 500 sin exponer detalles internos del error.

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Users.API.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
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
            // Logueamos el error completo internamente (nunca lo exponemos al cliente)
            _logger.LogError(exception, "Error inesperado: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "Ocurrió un error inesperado.",
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["errorCode"] = "USR-005";
            problemDetails.Extensions["errorMessage"] = "Error interno del servidor.";

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}