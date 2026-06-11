// Este filtro le dice a Swagger qué ejemplos mostrar en la sección Responses
// de cada endpoint de Notifications.API.
// Se registra en Program.cs con c.OperationFilter<NotificationsOperationFilter>()
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Notifications.API.SwaggerFilters
{
    public class NotificationsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Lee el nombre del endpoint actual para saber qué ejemplos poner
            var actionName = context.MethodInfo.Name;

            if (actionName == "Send")
                AplicarEjemplosSend(operation);

            if (actionName == "GetByUser")
                AplicarEjemplosGetByUser(operation);
        }

        // ─────────────────────────────
        // POST /api/notifications/send
        // ─────────────────────────────
        private static void AplicarEjemplosSend(OpenApiOperation operation)
        {
            // Ejemplo de éxito 201
            if (operation.Responses.TryGetValue("201", out var resp201))
            {
                resp201.Content.Clear();
                resp201.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["id"] = new OpenApiString("de4d9ee3-0f63-45a1-9838-b1ab6d7f417e"),
                        ["usuarioId"] = new OpenApiString("aa863f64-5e21-44ee-9d14-50e4c60e26b2"),
                        ["mensaje"] = new OpenApiString("Su orden fue confirmada."),
                        ["tipo"] = new OpenApiString("Email"),
                        ["estado"] = new OpenApiString("Enviada"),
                        ["fechaEnvio"] = new OpenApiString("2026-05-24T23:07:52Z")
                    }
                };
            }

            // Ejemplo de error 400 - NTF-002
            if (operation.Responses.TryGetValue("400", out var resp400))
            {
                resp400.Content.Clear();
                resp400.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                        ["title"] = new OpenApiString("Bad Request"),
                        ["status"] = new OpenApiInteger(400),
                        ["detail"] = new OpenApiString("La solicitud no es valida."),
                        ["instance"] = new OpenApiString("/api/notifications/send"),
                        ["errorCode"] = new OpenApiString("NTF-002"),
                        ["errorMessage"] = new OpenApiString("Los datos de la notificacion son invalidos.")
                    }
                };
            }

            // Ejemplo de error 404 - NTF-001
            if (operation.Responses.TryGetValue("404", out var resp404))
            {
                resp404.Content.Clear();
                resp404.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.4"),
                        ["title"] = new OpenApiString("Not Found"),
                        ["status"] = new OpenApiInteger(404),
                        ["detail"] = new OpenApiString("El recurso solicitado no fue encontrado."),
                        ["instance"] = new OpenApiString("/api/notifications/send"),
                        ["errorCode"] = new OpenApiString("NTF-001"),
                        ["errorMessage"] = new OpenApiString("El usuario destinatario no fue encontrado.")
                    }
                };
            }

            // Ejemplo de error 500 - NTF-004
            if (operation.Responses.TryGetValue("500", out var resp500))
            {
                resp500.Content.Clear();
                resp500.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.6.1"),
                        ["title"] = new OpenApiString("Internal Server Error"),
                        ["status"] = new OpenApiInteger(500),
                        ["detail"] = new OpenApiString("Ocurrio un error inesperado."),
                        ["instance"] = new OpenApiString("/api/notifications/send"),
                        ["errorCode"] = new OpenApiString("NTF-004"),
                        ["errorMessage"] = new OpenApiString("Error interno al procesar la notificacion.")
                    }
                };
            }
        }

        // ─────────────────────────────
        // GET /api/notifications/{userId}
        // ─────────────────────────────
        private static void AplicarEjemplosGetByUser(OpenApiOperation operation)
        {
            // Ejemplo de éxito 200
            if (operation.Responses.TryGetValue("200", out var resp200))
            {
                resp200.Content.Clear();
                resp200.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["id"] = new OpenApiString("de4d9ee3-0f63-45a1-9838-b1ab6d7f417e"),
                            ["usuarioId"] = new OpenApiString("aa863f64-5e21-44ee-9d14-50e4c60e26b2"),
                            ["mensaje"] = new OpenApiString("Su orden fue confirmada."),
                            ["tipo"] = new OpenApiString("Email"),
                            ["estado"] = new OpenApiString("Enviada"),
                            ["fechaEnvio"] = new OpenApiString("2026-05-24T23:07:52Z")
                        }
                    }
                };
            }

            // Ejemplo de error 404 - NTF-003
            if (operation.Responses.TryGetValue("404", out var resp404))
            {
                resp404.Content.Clear();
                resp404.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.4"),
                        ["title"] = new OpenApiString("Not Found"),
                        ["status"] = new OpenApiInteger(404),
                        ["detail"] = new OpenApiString("El recurso solicitado no fue encontrado."),
                        ["instance"] = new OpenApiString("/api/notifications/{userId}"),
                        ["errorCode"] = new OpenApiString("NTF-003"),
                        ["errorMessage"] = new OpenApiString("No se encontraron notificaciones para el usuario.")
                    }
                };
            }

            // Ejemplo de error 500 - NTF-004
            if (operation.Responses.TryGetValue("500", out var resp500))
            {
                resp500.Content.Clear();
                resp500.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.6.1"),
                        ["title"] = new OpenApiString("Internal Server Error"),
                        ["status"] = new OpenApiInteger(500),
                        ["detail"] = new OpenApiString("Ocurrio un error inesperado."),
                        ["instance"] = new OpenApiString("/api/notifications/{userId}"),
                        ["errorCode"] = new OpenApiString("NTF-004"),
                        ["errorMessage"] = new OpenApiString("Error interno al procesar la notificacion.")
                    }
                };
            }
        }
    }
}