using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.ExceptionHandlers;
using Notifications.API.Services;
using Notifications.API.SwaggerFilters;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/notifications-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

    c.OperationFilter<NotificationsOperationFilter>();
});

builder.Services.AddHealthChecks();

builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddHttpClient("UsersAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7075/");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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
// guarda el ID en context.Items para que los handlers lo puedan leer
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

// va DESPUÉS del Correlation ID para que los handlers ya tengan el ID disponible
app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.MapControllers();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();