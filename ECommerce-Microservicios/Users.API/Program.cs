// Program.cs — Punto de entrada de la aplicación
// Acá se configura y registra todo lo que necesita la app para funcionar:
// Serilog (logs), Services, ExceptionHandlers, Swagger, Health Checks y Correlation ID

using Serilog;
using Users.API.ExceptionHandlers;
using Users.API.Services;

// ─────────────────────────────
// CONFIGURAR SERILOG
// Serilog es el sistema de logs — registra todo lo que pasa en la app
// Escribe en dos lugares: consola (para ver en tiempo real) y archivo JSON (para guardar)
// ─────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/users-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// ─────────────────────────────
// REGISTRAR SERVICIOS
// AddScoped = se crea una instancia nueva por cada request HTTP
// ─────────────────────────────
builder.Services.AddScoped<UserService>();

// ─────────────────────────────
// REGISTRAR EXCEPTION HANDLERS EN ORDEN
// Los específicos van primero, GlobalExceptionHandler va último como red de seguridad
// ─────────────────────────────
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<UnauthorizedExceptionHandler>();
builder.Services.AddExceptionHandler<ForbiddenExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ─────────────────────────────
// SWAGGER CON XML COMMENTS
// Genera la documentación visual en /swagger
// Lee los comentarios /// de los controllers para mostrar descripciones
// ─────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Users.API", Version = "v1" });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ─────────────────────────────
// CORRELATION ID
// Genera un ID único por cada request y lo propaga en los logs y en la respuesta
// Si el cliente ya manda un X-Correlation-Id lo reutiliza, si no genera uno nuevo
// ─────────────────────────────
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging();
app.MapControllers();

// ─────────────────────────────
// HEALTH CHECKS
// /health → estado general
// /health/ready → ¿está listo para recibir requests?
// /health/live → ¿está vivo el proceso?
// ─────────────────────────────
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();