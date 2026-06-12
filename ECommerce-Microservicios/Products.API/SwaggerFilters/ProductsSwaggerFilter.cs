using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace Products.API.SwaggerFilters
{
    public class ProductsSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var path = context.ApiDescription.RelativePath?.ToLower() ?? "";
            var method = context.ApiDescription.HttpMethod?.ToUpper() ?? "";

            operation.Responses ??= new OpenApiResponses();

            // ─────────────────────────────────────────────────────────────
            // GET /api/products (Listar productos)
            // ─────────────────────────────────────────────────────────────
            if (path == "api/products" && method == "GET")
            {
                var resp200 = operation.Responses.ContainsKey("200") ? operation.Responses["200"] : new OpenApiResponse { Description = "Success" };
                resp200.Content.Clear();

                var arrayProductos = new OpenApiArray();
                var productoEjemplo = new OpenApiObject
                {
                    { "id", new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6") },
                    { "nombre", new OpenApiString("Notebook Dell XPS 15") },
                    { "descripcion", new OpenApiString("Laptop 15 pulgadas, 32GB RAM") },
                    { "precio", new OpenApiDouble(1500.00) },
                    { "stock", new OpenApiInteger(10) },
                    { "categoria", new OpenApiString("Electrónica") },
                    { "fechaCreacion", new OpenApiString("2026-06-11T10:30:00Z") }
                };
                arrayProductos.Add(productoEjemplo);

                resp200.Content["application/json"] = new OpenApiMediaType { Example = arrayProductos };
                operation.Responses["200"] = resp200;

                ForceResponseError(operation, "500", "https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", 500, "Ocurrió un error inesperado al recuperar el catálogo de productos.", "/api/products", "PRD-005", "Error inesperado al procesar el catálogo.");
            }

            // ─────────────────────────────────────────────────────────────
            // GET /api/products/{id} (Obtener producto por ID)
            // ─────────────────────────────────────────────────────────────
            if (path.Contains("api/products/") && method == "GET")
            {
                var resp200 = operation.Responses.ContainsKey("200") ? operation.Responses["200"] : new OpenApiResponse { Description = "Success" };
                resp200.Content.Clear();
                resp200.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        { "id", new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6") },
                        { "nombre", new OpenApiString("Notebook Dell XPS 15") },
                        { "descripcion", new OpenApiString("Laptop 15 pulgadas, 32GB RAM") },
                        { "precio", new OpenApiDouble(1500.00) },
                        { "stock", new OpenApiInteger(10) },
                        { "categoria", new OpenApiString("Electrónica") },
                        { "fechaCreacion", new OpenApiString("2026-06-11T10:30:00Z") }
                    }
                };
                operation.Responses["200"] = resp200;

                ForceResponseError(operation, "404", "https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", 404, "El recurso solicitado no fue encontrado.", "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-001", "Producto no encontrado.");
                ForceResponseError(operation, "500", "https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", 500, "Ocurrió un error inesperado.", "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-005", "Error interno en el servidor.");
            }

            // ─────────────────────────────────────────────────────────────
            // POST /api/products (Crear producto)
            // ─────────────────────────────────────────────────────────────
            if (path == "api/products" && method == "POST")
            {
                var resp201 = operation.Responses.ContainsKey("201") ? operation.Responses["201"] : new OpenApiResponse { Description = "Created" };
                resp201.Content.Clear();
                resp201.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        { "id", new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6") },
                        { "nombre", new OpenApiString("Notebook Dell XPS 15") },
                        { "descripcion", new OpenApiString("Laptop 15 pulgadas, 32GB RAM") },
                        { "precio", new OpenApiDouble(1500.00) },
                        { "stock", new OpenApiInteger(10) },
                        { "categoria", new OpenApiString("Electrónica") },
                        { "fechaCreacion", new OpenApiString("2026-06-11T10:30:00Z") }
                    }
                };
                operation.Responses["201"] = resp201;

                ForceResponseError(operation, "400", "https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", 400, "Los datos enviados son inválidos.", "/api/products", "PRD-002", "El precio no puede ser negativo.");
                ForceResponseError(operation, "409", "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", 409, "Conflicto con las reglas de negocio.", "/api/products", "PRD-003", "Ya existe un producto con el mismo nombre en esta categoría.");
                ForceResponseError(operation, "500", "https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", 500, "Ocurrió un error inesperado.", "/api/products", "PRD-005", "Error interno en el servidor.");
            }

            // ─────────────────────────────────────────────────────────────
            // PUT /api/products/{id} (Actualizar producto)
            // ─────────────────────────────────────────────────────────────
            if (path.Contains("api/products/") && method == "PUT")
            {
                var resp200 = operation.Responses.ContainsKey("200") ? operation.Responses["200"] : new OpenApiResponse { Description = "Success" };
                resp200.Content.Clear();
                resp200.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        { "id", new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6") },
                        { "nombre", new OpenApiString("Notebook Dell XPS 15") },
                        { "descripcion", new OpenApiString("Laptop 15 pulgadas, 64GB RAM") },
                        { "precio", new OpenApiDouble(1750.00) },
                        { "stock", new OpenApiInteger(8) },
                        { "categoria", new OpenApiString("Electrónica") },
                        { "fechaCreacion", new OpenApiString("2026-06-11T10:30:00Z") }
                    }
                };
                operation.Responses["200"] = resp200;

                ForceResponseError(operation, "400", "https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", 400, "Los datos enviados son inválidos.", "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-002", "Los datos provistos para la actualización son inválidos.");
                ForceResponseError(operation, "404", "https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", 404, "El recurso solicitado no fue encontrado.", "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-001", "No se encontró el producto que se desea modificar.");
                ForceResponseError(operation, "500", "https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", 500, "Ocurrió un error inesperado.", "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-005", "Error interno al actualizar el producto.");
            }

            // ─────────────────────────────────────────────────────────────
            // DELETE /api/products/{id} (Eliminar producto)
            // ─────────────────────────────────────────────────────────────
            if (path.Contains("api/products/") && method == "DELETE")
            {
                ForceResponseError(operation, "404", "https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", 404, "El recurso solicitado no fue encontrado.", "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-001", "El producto que se desea eliminar no existe.");
                ForceResponseError(operation, "409", "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", 409, "Conflicto con las reglas de negocio.", "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-004", "El producto tiene órdenes activas y no puede eliminarse.");
                ForceResponseError(operation, "500", "https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", 500, "Ocurrió un error inesperado.", "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-005", "Error interno al procesar la baja.");
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