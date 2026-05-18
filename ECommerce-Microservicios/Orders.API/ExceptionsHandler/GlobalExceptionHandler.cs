//GLOBALEXCEPTIONHANDLER.CS
//atrapa cualquier error inesperado que los otros tres handlers no pudieron manejar
//se activa para ORD-007 → error inesperado en el servicio o la persistencia
//siempre devuelve 500 porque si llego hasta aca es un error que el sistema no esperaba
using Microsoft.AspNetCore.Diagnostics;

namespace Orders.API.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            //no verifica el tipo de error porque atrapa cualquier cosa
            //siempre devuelve 500 con el codigo ORD-007
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrio un error inesperado.",
                //la URL donde ocurrio el error
                instance = context.Request.Path.Value,
                errorCode = "ORD-007",
                errorMessage = "Error interno al procesar la orden."
            }, cancellationToken);

            //devuelve true para avisarle al sistema que este handler manejo el error
            return true;
        }
    }
}