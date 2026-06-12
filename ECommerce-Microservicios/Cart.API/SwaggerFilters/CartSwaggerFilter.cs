using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace Cart.API.SwaggerFilters
{
    public class CartSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var path = context.ApiDescription.RelativePath?.ToLower() ?? "";
            var method = context.ApiDescription.HttpMethod?.ToUpper() ?? "";

            operation.Responses ??= new OpenApiResponses();

            // ─────────────────────────────────────────────────────────────
            // GET /api/cart/{userId} (Obtener el carrito)
            // ─────────────────────────────────────────────────────────────
            if (path.StartsWith("api/cart/") && method == "GET" && !path.Contains("/items"))
            {
                var resp200 = operation.Responses.ContainsKey("200") ? operation.Responses["200"] : new OpenApiResponse { Description = "Success" };
                resp200.Content.Clear();
                resp200.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        { "usuarioId", new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6") },
                        { "items", new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    { "productoId", new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6") },
                                    { "cantidad", new OpenApiInteger(2) }
                                }
                            }
                        },
                        { "fechaActualizacion", new OpenApiString("2026-06-11T21:15:00Z") }
                    }
                };
                operation.Responses["200"] = resp200;

                ForceResponseError(operation, "404", "https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", 404, "El recurso solicitado no fue encontrado.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6", "CRT-001", "Carrito no encontrado.");
                ForceResponseError(operation, "500", "https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", 500, "Ocurrió un error inesperado al recuperar el carrito.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6", "CRT-005", "Error interno en el servidor.");
            }

            // ─────────────────────────────────────────────────────────────
            // POST /api/cart/{userId}/items (Agregar ítem al carrito)
            // ─────────────────────────────────────────────────────────────
            if (path.StartsWith("api/cart/") && path.Contains("/items") && method == "POST")
            {
                var resp200 = operation.Responses.ContainsKey("200") ? operation.Responses["200"] : new OpenApiResponse { Description = "Success" };
                resp200.Content.Clear();
                resp200.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        { "usuarioId", new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6") },
                        { "items", new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    { "productoId", new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6") },
                                    { "cantidad", new OpenApiInteger(3) }
                                }
                            }
                        },
                        { "fechaActualizacion", new OpenApiString("2026-06-11T21:16:00Z") }
                    }
                };
                operation.Responses["200"] = resp200;

                ForceResponseError(operation, "400", "https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", 400, "Los datos enviados son inválidos.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6/items", "CRT-004", "La cantidad debe ser mayor a cero.");
                ForceResponseError(operation, "404", "https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", 404, "El recurso solicitado no fue encontrado.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6/items", "CRT-002", "Producto no encontrado en el catálogo.");
                ForceResponseError(operation, "422", "https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity", 422, "No se puede procesar la entidad debido a reglas de negocio.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6/items", "CRT-003", "Stock insuficiente del producto seleccionado.");
                ForceResponseError(operation, "500", "https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", 500, "Ocurrió un error inesperado.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6/items", "CRT-005", "Error interno en el servidor.");
            }

            // ─────────────────────────────────────────────────────────────
            // PUT /api/cart/{userId}/items/{productId} (Actualizar cantidad)
            // ─────────────────────────────────────────────────────────────
            if (path.StartsWith("api/cart/") && path.Contains("/items/") && method == "PUT")
            {
                var resp200 = operation.Responses.ContainsKey("200") ? operation.Responses["200"] : new OpenApiResponse { Description = "Success" };
                resp200.Content.Clear();
                resp200.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        { "usuarioId", new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6") },
                        { "items", new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    { "productoId", new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6") },
                                    { "cantidad", new OpenApiInteger(5) }
                                }
                            }
                        },
                        { "fechaActualizacion", new OpenApiString("2026-06-11T21:18:00Z") }
                    }
                };
                operation.Responses["200"] = resp200;

                ForceResponseError(operation, "400", "https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", 400, "Los datos enviados son inválidos.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6/items/3fa85f64", "CRT-004", "La cantidad informada es inválida.");
                ForceResponseError(operation, "404", "https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", 404, "El recurso solicitado no fue encontrado.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6/items/3fa85f64", "CRT-001", "Carrito no encontrado.");
                ForceResponseError(operation, "422", "https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity", 422, "No se puede procesar la entidad.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6/items/3fa85f64", "CRT-003", "No hay stock suficiente para satisfacer la nueva cantidad.");
                ForceResponseError(operation, "500", "https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", 500, "Ocurrió un error inesperado.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6/items/3fa85f64", "CRT-005", "Error interno.");
            }

            // ─────────────────────────────────────────────────────────────
            // DELETE /api/cart/{userId} o /items/{productId} (Vaciar o remover)
            // ─────────────────────────────────────────────────────────────
            if (method == "DELETE")
            {
                ForceResponseError(operation, "404", "https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", 404, "El recurso solicitado no fue encontrado.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6", "CRT-001", "El carrito solicitado no existe.");
                ForceResponseError(operation, "500", "https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", 500, "Ocurrió un error inesperado.", "/api/cart/3fa85f64-5717-4562-b3fc-2c963f66afa6", "CRT-005", "Error inesperado al vaciar o quitar el ítem.");
            }
        }

        private static void ForceResponseError(OpenApiOperation op, string statusCode, string type, string title, int status, string detail, string instance, string errorCode, string errorMsg)
        {
            var response = new OpenApiResponse { Description = title };
            response.Content["application/json"] = new OpenApiMediaType
            {
                Example = new OpenApiObject
                {
                    { "type", new OpenApiString(type) },
                    { "title", new OpenApiString(title) },
                    { "status", new OpenApiInteger(status) },
                    { "detail", new OpenApiString(detail) },
                    { "instance", new OpenApiString(instance) },
                    { "errorCode", new OpenApiString(errorCode) },
                    { "errorMessage", new OpenApiString(errorMsg) }
                }
            };
            op.Responses[statusCode] = response;
        }
    }
}