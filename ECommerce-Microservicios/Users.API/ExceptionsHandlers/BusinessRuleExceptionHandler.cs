// Captura las BusinessRuleException y arma la respuesta HTTP 400 o 409
// según el tipo de error de negocio

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException businessRuleException)
                return false;

            // USR-001 es conflicto (email duplicado) → 409
            // El resto son errores de negocio → 400
            var statusCode = businessRuleException.ErrorCode == "USR-001"
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.0",
                Title = "Business Rule Violation",
                Status = statusCode,
                Detail = "Se violó una regla de negocio.",
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["errorCode"] = businessRuleException.ErrorCode;
            problemDetails.Extensions["errorMessage"] = businessRuleException.Message;

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}