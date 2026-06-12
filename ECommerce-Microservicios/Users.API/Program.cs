using Serilog;
using Users.API.Data;
using Users.API.ExceptionHandlers;
using Users.API.Services;
using Users.API.DTOs;
using Users.API.SwaggerFilters;

// ─────────────────────────────
// CONFIGURAR SERILOG
// consola → formato legible para desarrollo
// archivo → formato JSON estructurado para produccion
// Enrich.WithProperty agrega el nombre del servicio a todos los logs
// Enrich.FromLogContext permite que el CorrelationId se incluya en cada log
// ─────────────────────────────
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Users.API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        new Serilog.Formatting.Json.JsonFormatter(),
        "logs/users-.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

//le dice a la aplicacion que use Serilog para registrar todo lo que pasa
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Users.API",
        Version = "v1",
        Description = "API para gestion de usuarios del eCommerce."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    c.MapType<RegisterUserRequest>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "object",
        Example = new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["nombre"] = new Microsoft.OpenApi.Any.OpenApiString("María"),
            ["apellido"] = new Microsoft.OpenApi.Any.OpenApiString("González"),
            ["email"] = new Microsoft.OpenApi.Any.OpenApiString("maria@email.com"),
            ["password"] = new Microsoft.OpenApi.Any.OpenApiString("MiPassword123!")
        }
    });

    c.MapType<LoginUserRequest>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "object",
        Example = new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["email"] = new Microsoft.OpenApi.Any.OpenApiString("maria@email.com"),
            ["password"] = new Microsoft.OpenApi.Any.OpenApiString("MiPassword123!")
        }
    });

    c.OperationFilter<UsersOperationFilter>();
});

builder.Services.AddHealthChecks();

builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<UserRepository>();

//conexion con Notifications.API para enviar notificacion cuando se registra un usuario
builder.Services.AddHttpClient("NotificationsAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7185/");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

builder.Services.AddScoped<UserService>();

builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<UnauthorizedExceptionHandler>();
builder.Services.AddExceptionHandler<ForbiddenExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
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
// genera un ID unico por request y lo guarda en context.Items
// para que los handlers lo puedan leer cuando hay un error
// ─────────────────────────────
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

//va DESPUES del Correlation ID para que los handlers ya tengan el ID disponible
app.UseExceptionHandler();

//loggea inicio/fin de cada request con duracion automaticamente
app.UseSerilogRequestLogging();

app.MapControllers();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();