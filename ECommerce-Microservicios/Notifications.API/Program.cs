// Program.cs — Punto de entrada de Notifications.API
// Acá se configura y registra todo lo que necesita la app para funcionar:
// Serilog (logs), Services, ExceptionHandlers, Swagger, Health Checks y Correlation ID

using Notifications.API.ExceptionHandlers;
using Notifications.API.Services;
using Serilog;

// ─────────────────────────────
// CONFIGURAR SERILOG
// Escribe en consola (para ver en tiempo real) y en archivo (para guardar)
// ─────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/notifications-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ─────────────────────────────
// SWAGGER CON XML COMMENTS
// Lee los comentarios /// de los controllers para mostrar descripciones en /swagger
// ─────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Notifications.API", Version = "v1" });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

builder.Services.AddHealthChecks();

// AddScoped = se crea un NotificationService nuevo por cada request HTTP
builder.Services.AddScoped<NotificationService>();

// ─────────────────────────────
// CONEXIÓN CON USERS.API
// Cuando el NotificationService necesite verificar si un usuario existe, usa este HttpClient
// DangerousAcceptAnyServerCertificateValidator acepta el certificado de desarrollo local
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
// Los específicos van primero, GlobalExceptionHandler va último como red de seguridad
// ─────────────────────────────
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();       // NTF-001, NTF-003 → 404
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();   // NTF-002 → 400
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();         // NTF-004 → 500
builder.Services.AddProblemDetails();

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
// Genera un ID único por request y lo propaga en logs y respuestas
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

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();