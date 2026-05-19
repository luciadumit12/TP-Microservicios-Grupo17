// Captura las ForbiddenException y arma la respuesta HTTP 403
// Se usa cuando el usuario está bloqueado e intenta loguearse
// USR-004 → bloqueado por 3 intentos fallidos consecutivos
// USR-005 → bloqueado manualmente por detección de fraude

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    public class ForbiddenExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Si la excepción NO es ForbiddenException, no la manejamos acá
            if (exception is not ForbiddenException forbiddenException)
                return false;

            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Detail = "El acceso está prohibido.",
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["errorCode"] = forbiddenException.ErrorCode;
            problemDetails.Extensions["errorMessage"] = forbiddenException.Message;

            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}