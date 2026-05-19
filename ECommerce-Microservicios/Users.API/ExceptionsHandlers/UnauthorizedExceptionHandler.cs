// Captura las UnauthorizedException y arma la respuesta HTTP 401
// Se usa cuando el usuario manda credenciales incorrectas (email o contraseña mal)
// errorCode USR-003 según el catálogo del TP

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    public class UnauthorizedExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Si la excepción NO es UnauthorizedException, no la manejamos acá
            if (exception is not UnauthorizedException unauthorizedException)
                return false;

            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "Las credenciales no son válidas.",
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["errorCode"] = unauthorizedException.ErrorCode;
            problemDetails.Extensions["errorMessage"] = unauthorizedException.Message;

            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
