using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Users.API.SwaggerFilters
{
    public class UsersOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var actionName = context.MethodInfo.Name;

            if (actionName == "Register")
                AplicarEjemplosRegister(operation);

            if (actionName == "Login")
                AplicarEjemplosLogin(operation);
        }

        private static void AplicarEjemplosRegister(OpenApiOperation operation)
        {
            if (operation.Responses.TryGetValue("201", out var resp201))
            {
                resp201.Description = "Usuario registrado exitosamente";
                resp201.Content.Clear();
                resp201.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["id"] = new OpenApiString("a1b2c3d4-0000-0000-0000-111122223333"),
                        ["nombre"] = new OpenApiString("María"),
                        ["apellido"] = new OpenApiString("González"),
                        ["email"] = new OpenApiString("maria@email.com"),
                        ["fechaRegistro"] = new OpenApiString("2024-03-10T09:00:00Z"),
                        ["activo"] = new OpenApiBoolean(true)
                    }
                };
            }

            if (operation.Responses.TryGetValue("400", out var resp400))
            {
                resp400.Description = "Datos invalidos, por ej campos vacios (USR-002)";
                resp400.Content.Clear();
                resp400.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                        ["title"] = new OpenApiString("Bad Request"),
                        ["status"] = new OpenApiInteger(400),
                        ["detail"] = new OpenApiString("Los datos enviados son invalidos."),
                        ["instance"] = new OpenApiString("/api/users/register"),
                        ["errorCode"] = new OpenApiString("USR-002"),
                        ["errorMessage"] = new OpenApiString("Los datos del usuario son invalidos.")
                    }
                };
            }

            if (operation.Responses.TryGetValue("409", out var resp409))
            {
                resp409.Description = "El email ya esta registrado (USR-001)";
                resp409.Content.Clear();
                resp409.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.9"),
                        ["title"] = new OpenApiString("Conflict"),
                        ["status"] = new OpenApiInteger(409),
                        ["detail"] = new OpenApiString("Ya existe un recurso con esos datos."),
                        ["instance"] = new OpenApiString("/api/users/register"),
                        ["errorCode"] = new OpenApiString("USR-001"),
                        ["errorMessage"] = new OpenApiString("El email 'maria@email.com' ya esta registrado.")
                    }
                };
            }

            if (operation.Responses.TryGetValue("500", out var resp500))
            {
                resp500.Description = "Error interno del servidor (USR-006)";
                resp500.Content.Clear();
                resp500.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.6.1"),
                        ["title"] = new OpenApiString("Internal Server Error"),
                        ["status"] = new OpenApiInteger(500),
                        ["detail"] = new OpenApiString("Ocurrio un error inesperado."),
                        ["instance"] = new OpenApiString("/api/users/register"),
                        ["errorCode"] = new OpenApiString("USR-006"),
                        ["errorMessage"] = new OpenApiString("Error interno al procesar el usuario.")
                    }
                };
            }
        }

        private static void AplicarEjemplosLogin(OpenApiOperation operation)
        {
            if (operation.Responses.TryGetValue("200", out var resp200))
            {
                resp200.Description = "Login exitoso";
                resp200.Content.Clear();
                resp200.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["id"] = new OpenApiString("a1b2c3d4-0000-0000-0000-111122223333"),
                        ["nombre"] = new OpenApiString("María"),
                        ["apellido"] = new OpenApiString("González"),
                        ["email"] = new OpenApiString("maria@email.com"),
                        ["fechaRegistro"] = new OpenApiString("2024-03-10T09:00:00Z"),
                        ["activo"] = new OpenApiBoolean(true)
                    }
                };
            }

            if (operation.Responses.TryGetValue("400", out var resp400))
            {
                resp400.Description = "Datos invalidos, por ej campos vacios (USR-002)";
                resp400.Content.Clear();
                resp400.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                        ["title"] = new OpenApiString("Bad Request"),
                        ["status"] = new OpenApiInteger(400),
                        ["detail"] = new OpenApiString("Los datos enviados son invalidos."),
                        ["instance"] = new OpenApiString("/api/users/login"),
                        ["errorCode"] = new OpenApiString("USR-002"),
                        ["errorMessage"] = new OpenApiString("Los datos del usuario son invalidos.")
                    }
                };
            }

            if (operation.Responses.TryGetValue("401", out var resp401))
            {
                resp401.Description = "Credenciales incorrectas (USR-003)";
                resp401.Content.Clear();
                resp401.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7235#section-3.1"),
                        ["title"] = new OpenApiString("Unauthorized"),
                        ["status"] = new OpenApiInteger(401),
                        ["detail"] = new OpenApiString("Las credenciales no son validas."),
                        ["instance"] = new OpenApiString("/api/users/login"),
                        ["errorCode"] = new OpenApiString("USR-003"),
                        ["errorMessage"] = new OpenApiString("Credenciales incorrectas.")
                    }
                };
            }

            if (operation.Responses.TryGetValue("403", out var resp403))
            {
                resp403.Description = "Usuario bloqueado (USR-004 o USR-005)";
                resp403.Content.Clear();
                resp403.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.3"),
                        ["title"] = new OpenApiString("Forbidden"),
                        ["status"] = new OpenApiInteger(403),
                        ["detail"] = new OpenApiString("El acceso esta prohibido."),
                        ["instance"] = new OpenApiString("/api/users/login"),
                        ["errorCode"] = new OpenApiString("USR-004"),
                        ["errorMessage"] = new OpenApiString("Su cuenta fue bloqueada por superar el maximo de intentos fallidos. Contacte a soporte.")
                    }
                };
            }

            if (operation.Responses.TryGetValue("500", out var resp500))
            {
                resp500.Description = "Error interno del servidor (USR-006)";
                resp500.Content.Clear();
                resp500.Content["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.6.1"),
                        ["title"] = new OpenApiString("Internal Server Error"),
                        ["status"] = new OpenApiInteger(500),
                        ["detail"] = new OpenApiString("Ocurrio un error inesperado."),
                        ["instance"] = new OpenApiString("/api/users/login"),
                        ["errorCode"] = new OpenApiString("USR-006"),
                        ["errorMessage"] = new OpenApiString("Error interno al procesar el usuario.")
                    }
                };
            }
        }
    }
}