// Captura las NotFoundException y arma la respuesta HTTP 404
// con el formato de error que pide el TP (errorCode, errorMessage, etc.)

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Si la excepción NO es NotFoundException, no la manejamos acá
            if (exception is not NotFoundException notFoundException)
                return false;

            // Armamos la respuesta con el formato del TP
            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = "El recurso solicitado no fue encontrado.",
                Instance = httpContext.Request.Path
            };

            // Agregamos errorCode y errorMessage del catálogo
            problemDetails.Extensions["errorCode"] = notFoundException.ErrorCode;
            problemDetails.Extensions["errorMessage"] = notFoundException.Message;

            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; // La excepción fue manejada
        }
    }
}