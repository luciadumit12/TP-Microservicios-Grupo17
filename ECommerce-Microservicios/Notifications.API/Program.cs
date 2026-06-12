using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.ExceptionHandlers;
using Notifications.API.Services;
using Notifications.API.SwaggerFilters;
using Serilog;

// ─────────────────────────────
// CONFIGURAR SERILOG
// consola → formato legible para desarrollo
// archivo → formato JSON estructurado para produccion
// Enrich.WithProperty agrega el nombre del servicio a todos los logs
// ─────────────────────────────
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Notifications.API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        new Serilog.Formatting.Json.JsonFormatter(),
        "logs/notifications-.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Reemplazar el logger default de .NET por Serilog
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ─────────────────────────────
// SWAGGER CON XML COMMENTS Y EJEMPLO PRECARGADO EN REQUEST BODY
// ─────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Notifications.API",
        Version = "v1",
        Description = "API para gestion de notificaciones del eCommerce."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    // Ejemplo precargado en el Request Body del POST /api/notifications/send
    c.MapType<SendNotificationRequest>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "object",
        Example = new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["usuarioId"] = new Microsoft.OpenApi.Any.OpenApiString("aa863f64-5e21-44ee-9d14-50e4c60e26b2"),
            ["mensaje"] = new Microsoft.OpenApi.Any.OpenApiString("Su orden fue confirmada."),
            ["tipo"] = new Microsoft.OpenApi.Any.OpenApiString("Email")
        }
    });

    // registra el filtro que pone los ejemplos reales en la seccion Responses
    c.OperationFilter<NotificationsOperationFilter>();
});

builder.Services.AddHealthChecks();

// el DatabaseInitializer crea la tabla notifications en la base de datos cuando arranca la app
builder.Services.AddSingleton<DatabaseInitializer>();

// el NotificationRepository maneja todas las operaciones con la base de datos SQLite
builder.Services.AddScoped<NotificationRepository>();

// AddScoped = se crea un NotificationService nuevo por cada request HTTP
builder.Services.AddScoped<NotificationService>();

// ─────────────────────────────
// CONEXION CON USERS.API
// cuando el NotificationService necesite verificar si un usuario existe, usa este HttpClient
// ─────────────────────────────
builder.Services.AddHttpClient("UsersAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7075/");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// ─────────────────────────────
// REGISTRAR EXCEPTION HANDLERS EN ORDEN
// los especificos van primero, GlobalExceptionHandler va ultimo como red de seguridad
// ─────────────────────────────
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();       // NTF-001, NTF-003 → 404
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();     // NTF-002 → 400
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();   // NTF-002 (regla de negocio) → 400
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();         // NTF-004 → 500
builder.Services.AddProblemDetails();

var app = builder.Build();

// inicializa la base de datos al arrancar la aplicacion
app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

// Swagger solo en desarrollo
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
// tambien propaga el ID en las llamadas HTTP salientes a Users.API
// ─────────────────────────────
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();
    // lo guardamos en Items para que los handlers lo lean cuando hay un error
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

// ─────────────────────────────
// HEALTH CHECKS
// /health → estado general
// /health/ready → esta listo para recibir requests?
// /health/live → esta vivo el proceso?
// ─────────────────────────────
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();