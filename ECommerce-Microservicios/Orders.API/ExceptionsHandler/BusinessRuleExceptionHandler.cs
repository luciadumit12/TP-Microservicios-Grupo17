using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

            var status = ex.ErrorCode == "ORD-005" ? 422 : 409;

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new
            {
                type = status == 422
                    ? "https://tools.ietf.org/html/rfc4918#section-11.2"
                    : "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                title = status == 422 ? "Unprocessable Entity" : "Conflict",
                status,
                detail = status == 422
                    ? "No se puede procesar la solicitud."
                    : "No se puede modificar el estado.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message
            }, cancellationToken);

            return true;
        }
    }
}