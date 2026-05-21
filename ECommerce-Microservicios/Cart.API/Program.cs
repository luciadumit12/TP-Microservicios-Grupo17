using Cart.API.Data;
using Cart.API.ExceptionHandlers;
using Cart.API.Services;
using Serilog;
using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Data.Sqlite;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
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
        Description = "API para la gestión del carrito del e-commerce.."
    });

    // Habilita XML Comments en Swagger
    var xmlFilename =
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    options.IncludeXmlComments(
        Path.Combine(AppContext.BaseDirectory, xmlFilename)
    );
});

// =========================
// Health Checks (Corregido con validación de BD)
// =========================
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
builder.Services.AddHttpClient("ProductsAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7268/");
});

// HttpClient para consultar Users.API 
builder.Services.AddHttpClient("UsersAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7075/");
});

// =========================
// Exception Handlers
// =========================
// CRT-001 / CRT-002 → 404
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();

// CRT-004 → 400
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

// CRT-003 → 422
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();

// CRT-005 → 500
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

// =========================
// Inyección de Dependencias
// =========================
builder.Services.AddSingleton<DatabaseInitializer>(); // Para crear las tablas
builder.Services.AddScoped<CartService>(); // Dejado una sola vez
builder.Services.AddScoped<CartRepository>();

var app = builder.Build();

// AGREGADO: Inicializar la base de datos al arrancar
app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Cart API v1");
    });
}

app.UseHttpsRedirection();

// =========================
// Correlation ID Middleware
// =========================
app.Use(async (context, next) =>
{
    var correlationId =
        context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        ?? Guid.NewGuid().ToString();

    context.Response.Headers["X-Correlation-Id"] = correlationId;

    using (Serilog.Context.LogContext.PushProperty(
        "CorrelationId",
        correlationId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging();

// =========================
// Exception Handler Middleware 
// =========================
app.UseExceptionHandler();

app.MapControllers();

// =========================
// Health Checks 
// =========================
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