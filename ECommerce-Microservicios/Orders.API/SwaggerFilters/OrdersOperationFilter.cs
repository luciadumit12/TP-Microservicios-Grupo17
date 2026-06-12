// Este filtro le dice a Swagger qué ejemplos mostrar en la sección Responses
// de cada endpoint de Orders.API.
// Se registra en Program.cs con options.OperationFilter<OrdersOperationFilter>()
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Orders.API.SwaggerFilters
{
    public class OrdersOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // lee el nombre del endpoint actual para saber qué ejemplos poner
            var actionName = context.MethodInfo.Name;

            if (actionName == "GetAll")
                AplicarEjemplosGetAll(operation);

            if (actionName == "GetById")
                AplicarEjemplosGetById(operation);

            if (actionName == "Create")
                AplicarEjemplosCreate(operation);

            if (actionName == "UpdateStatus")
                AplicarEjemplosUpdateStatus(operation);
        }

        // ─────────────────────────────
        // GET /api/orders
        // ─────────────────────────────
        private static void AplicarEjemplosGetAll(OpenApiOperation operation)
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
                            ["id"] = new OpenApiString("4fa6a8f0-872e-4217-b91b-58d4b963bafc"),
                            ["usuarioId"] = new OpenApiString("aa863f64-5e21-44ee-9d14-50e4c60e26b2"),
                            ["items"] = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["productoId"] = new OpenApiString("21a35e84-e1ad-4b17-b2ea-3b0598322a96"),
                                    ["cantidad"] = new OpenApiInteger(1),
                                    ["precioUnitario"] = new OpenApiDouble(15000)
                                }
                            },
                            ["total"] = new OpenApiDouble(15000),
                            ["estado"] = new OpenApiString("Pendiente"),
                            ["fechaCreacion"] = new OpenApiString("2026-05-24T20:26:30Z")
                        }
                    }
                };
            }

            // Ejemplo de error 500 - ORD-007
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
                        ["instance"] = new OpenApiString("/api/orders"),
                        ["errorCode"] = new OpenApiString("ORD-007"),
                        ["errorMessage"] = new OpenApiString("Error interno al procesar la orden.")
                    }
                };
            }
        }

        // ─────────────────────────────
        // GET /api/orders/{id}
        // ─────────────────────────────
        private static void AplicarEjemplosGetById(OpenApiOperation operation)
        {
            // Ejemplo de éxito 200
            if (operation.Responses.TryGetValue("200", out var resp200))
            {
                resp200.Content.Clear();
                resp200.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["id"] = new OpenApiString("4fa6a8f0-872e-4217-b91b-58d4b963bafc"),
                        ["usuarioId"] = new OpenApiString("aa863f64-5e21-44ee-9d14-50e4c60e26b2"),
                        ["items"] = new OpenApiArray
                        {
                            new OpenApiObject
                            {
                                ["productoId"] = new OpenApiString("21a35e84-e1ad-4b17-b2ea-3b0598322a96"),
                                ["cantidad"] = new OpenApiInteger(1),
                                ["precioUnitario"] = new OpenApiDouble(15000)
                            }
                        },
                        ["total"] = new OpenApiDouble(15000),
                        ["estado"] = new OpenApiString("Pendiente"),
                        ["fechaCreacion"] = new OpenApiString("2026-05-24T20:26:30Z")
                    }
                };
            }

            // Ejemplo de error 404 - ORD-001
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
                        ["instance"] = new OpenApiString("/api/orders/{id}"),
                        ["errorCode"] = new OpenApiString("ORD-001"),
                        ["errorMessage"] = new OpenApiString("Orden no encontrada.")
                    }
                };
            }

            // Ejemplo de error 500 - ORD-007
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
                        ["instance"] = new OpenApiString("/api/orders/{id}"),
                        ["errorCode"] = new OpenApiString("ORD-007"),
                        ["errorMessage"] = new OpenApiString("Error interno al procesar la orden.")
                    }
                };
            }
        }

        // ─────────────────────────────
        // POST /api/orders
        // ─────────────────────────────
        private static void AplicarEjemplosCreate(OpenApiOperation operation)
        {
            // Ejemplo de éxito 201
            if (operation.Responses.TryGetValue("201", out var resp201))
            {
                resp201.Content.Clear();
                resp201.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["id"] = new OpenApiString("4fa6a8f0-872e-4217-b91b-58d4b963bafc"),
                        ["usuarioId"] = new OpenApiString("aa863f64-5e21-44ee-9d14-50e4c60e26b2"),
                        ["items"] = new OpenApiArray
                        {
                            new OpenApiObject
                            {
                                ["productoId"] = new OpenApiString("21a35e84-e1ad-4b17-b2ea-3b0598322a96"),
                                ["cantidad"] = new OpenApiInteger(1),
                                ["precioUnitario"] = new OpenApiDouble(15000)
                            }
                        },
                        ["total"] = new OpenApiDouble(15000),
                        ["estado"] = new OpenApiString("Pendiente"),
                        ["fechaCreacion"] = new OpenApiString("2026-05-24T20:26:30Z")
                    }
                };
            }

            // Ejemplo de error 400 - ORD-002
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
                        ["detail"] = new OpenApiString("Los datos enviados son invalidos."),
                        ["instance"] = new OpenApiString("/api/orders"),
                        ["errorCode"] = new OpenApiString("ORD-002"),
                        ["errorMessage"] = new OpenApiString("Los datos de la orden son invalidos.")
                    }
                };
            }

            // Ejemplo de error 404 - ORD-003 y ORD-004
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
                        ["instance"] = new OpenApiString("/api/orders"),
                        ["errorCode"] = new OpenApiString("ORD-003"),
                        ["errorMessage"] = new OpenApiString("Usuario no encontrado al crear la orden.")
                    }
                };
            }

            // Ejemplo de error 422 - ORD-005
            if (operation.Responses.TryGetValue("422", out var resp422))
            {
                resp422.Content.Clear();
                resp422.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                        ["title"] = new OpenApiString("Unprocessable Entity"),
                        ["status"] = new OpenApiInteger(422),
                        ["detail"] = new OpenApiString("No se puede procesar la solicitud."),
                        ["instance"] = new OpenApiString("/api/orders"),
                        ["errorCode"] = new OpenApiString("ORD-005"),
                        ["errorMessage"] = new OpenApiString("Stock insuficiente para 'Notebook Dell XPS 15'. Disponible: 2, solicitado: 5.")
                    }
                };
            }

            // Ejemplo de error 500 - ORD-007
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
                        ["instance"] = new OpenApiString("/api/orders"),
                        ["errorCode"] = new OpenApiString("ORD-007"),
                        ["errorMessage"] = new OpenApiString("Error interno al procesar la orden.")
                    }
                };
            }
        }

        // ─────────────────────────────
        // PUT /api/orders/{id}/status
        // ─────────────────────────────
        private static void AplicarEjemplosUpdateStatus(OpenApiOperation operation)
        {
            // Ejemplo de éxito 200
            if (operation.Responses.TryGetValue("200", out var resp200))
            {
                resp200.Content.Clear();
                resp200.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["id"] = new OpenApiString("4fa6a8f0-872e-4217-b91b-58d4b963bafc"),
                        ["usuarioId"] = new OpenApiString("aa863f64-5e21-44ee-9d14-50e4c60e26b2"),
                        ["items"] = new OpenApiArray
                        {
                            new OpenApiObject
                            {
                                ["productoId"] = new OpenApiString("21a35e84-e1ad-4b17-b2ea-3b0598322a96"),
                                ["cantidad"] = new OpenApiInteger(1),
                                ["precioUnitario"] = new OpenApiDouble(15000)
                            }
                        },
                        ["total"] = new OpenApiDouble(15000),
                        ["estado"] = new OpenApiString("Confirmada"),
                        ["fechaCreacion"] = new OpenApiString("2026-05-24T20:26:30Z")
                    }
                };
            }

            // Ejemplo de error 404 - ORD-001
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
                        ["instance"] = new OpenApiString("/api/orders/{id}/status"),
                        ["errorCode"] = new OpenApiString("ORD-001"),
                        ["errorMessage"] = new OpenApiString("Orden no encontrada.")
                    }
                };
            }

            // Ejemplo de error 409 - ORD-006
            if (operation.Responses.TryGetValue("409", out var resp409))
            {
                resp409.Content.Clear();
                resp409.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.9"),
                        ["title"] = new OpenApiString("Conflict"),
                        ["status"] = new OpenApiInteger(409),
                        ["detail"] = new OpenApiString("No se puede modificar el estado."),
                        ["instance"] = new OpenApiString("/api/orders/{id}/status"),
                        ["errorCode"] = new OpenApiString("ORD-006"),
                        ["errorMessage"] = new OpenApiString("Una orden en estado 'Entregada' no puede cambiar a 'Pendiente'.")
                    }
                };
            }

            // Ejemplo de error 500 - ORD-007
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
                        ["instance"] = new OpenApiString("/api/orders/{id}/status"),
                        ["errorCode"] = new OpenApiString("ORD-007"),
                        ["errorMessage"] = new OpenApiString("Error interno al procesar la orden.")
                    }
                };
            }
        }
    }
}