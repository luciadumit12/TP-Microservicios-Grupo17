// Cuando llega una llamada HTTP por ej 'crear producto'. el sistema debe saber:
// Quien la recibe → Controller: recibe la llamada HTTP
// Quien la procesa → Service: decide que hacer con la llamada
// Quien maneja los errores → ExceptionHandlers: categoriza los errores que se pueden presentar

using Products.API.Data;
using Products.API.ExceptionHandlers;
using Products.API.Services;
using Serilog;
using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Data.Sqlite;

// ─────────────────────────────
// CONFIGURAR SERILOG
// consola → formato legible para desarrollo
// archivo → formato JSON estructurado para produccion
// Enrich.WithProperty agrega el nombre del servicio a todos los logs
// Enrich.FromLogContext permite que el CorrelationId se incluya en cada log
// ─────────────────────────────
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Products.API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        new Serilog.Formatting.Json.JsonFormatter(),
        "logs/products-.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configura Swagger para leer los XML comments y mostrar la documentación de cada endpoint
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Products API",
        Version = "v1",
        Description = "API para la gestión de productos del e-commerce."
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.OperationFilter<Products.API.SwaggerFilters.ProductsSwaggerFilter>();
});

// ─────────────────────────────
// HEALTH CHECKS
// Self → /health/live verifica que el proceso esta corriendo
// Database → /health/ready verifica que la base de datos responde
// ─────────────────────────────
builder.Services.AddHealthChecks()
    .AddCheck("Self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddCheck("Database", () =>
    {
        try
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=products.db";
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            return HealthCheckResult.Healthy("Base de datos SQLite conectada correctamente.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("No se pudo establecer conexión con la base de datos SQLite.", ex);
        }
    }, tags: new[] { "ready" });

// Inyección de Dependencias
builder.Services.AddSingleton<DatabaseInitializer>();
// el ProductRepository maneja todas las operaciones con la base de datos SQLite
builder.Services.AddScoped<ProductRepository>();
// el ProductService contiene toda la logica de negocio
builder.Services.AddScoped<ProductService>();

// HttpClient para consultar a Orders.API y verificar si el producto tiene ordenes activas (PRD-004)
builder.Services.AddHttpClient("OrdersAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7168/");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// ─────────────────────────────
// EXCEPTION HANDLERS EN ORDEN
// los especificos van primero, GlobalExceptionHandler va ultimo como red de seguridad
// ─────────────────────────────
// cuando algo no se encuentra: PRD-001 → devuelve 404
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
// cuando los datos enviados son invalidos: PRD-002 → devuelve 400
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
// cuando se viola una regla de negocio: PRD-003, PRD-004 → devuelve 409
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
// cuando ocurre cualquier error inesperado: PRD-005 → devuelve 500
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Inicializa la base de datos al arrancar la aplicacion (crea la tabla si no existe)
app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ─────────────────────────────
// CORRELATION ID — va ANTES de UseExceptionHandler
// genera un ID unico por request y lo guarda en context.Items
// para que los handlers lo puedan leer cuando hay un error
// ─────────────────────────────
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();
    // guardamos en Items para que los handlers lo lean cuando hay un error
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

// va DESPUES del Correlation ID para que los handlers ya tengan el ID disponible
app.UseExceptionHandler();

// loggea inicio/fin de cada request con duracion automaticamente
app.UseSerilogRequestLogging();

app.MapControllers();

// Endpoint generico (ejecuta todos los chequeos cargados)
app.MapHealthChecks("/health");

// Endpoint Live: Solo evalua que la app este corriendo en memoria (tag: live)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

// Endpoint Ready: Evalua que las dependencias duras como la BD respondan (tag: ready)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();