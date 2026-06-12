using Cart.API.Data;
using Cart.API.ExceptionHandlers;
using Cart.API.Services;
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
    .Enrich.WithProperty("Servicio", "Cart.API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        new Serilog.Formatting.Json.JsonFormatter(),
        "logs/cart-.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Cart API",
        Version = "v1",
        Description = "API para la gestión del carrito del e-commerce."
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.OperationFilter<Cart.API.SwaggerFilters.CartSwaggerFilter>();
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
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=cart.db";
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error BD", ex);
        }
    }, tags: new[] { "ready" });

// HttpClient para consultar Products.API
// DangerousAcceptAnyServerCertificateValidator acepta el certificado autofirmado en desarrollo
builder.Services.AddHttpClient("ProductsAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7268/");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// HttpClient para consultar Users.API
builder.Services.AddHttpClient("UsersAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7075/");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// ─────────────────────────────
// EXCEPTION HANDLERS EN ORDEN
// los especificos van primero, GlobalExceptionHandler va ultimo como red de seguridad
// ─────────────────────────────
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();      // CRT-001, CRT-002 → 404
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();    // CRT-004 → 400
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();  // CRT-003 → 422
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();        // CRT-005 → 500
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<CartRepository>();

var app = builder.Build();

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

app.MapHealthChecks("/health");

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.Run();