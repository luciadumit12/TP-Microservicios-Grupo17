// Cuando llega una llamada HTTP por ej 'crear producto'. el sistema debe saber:
// Quien la recibe → Controller: recibe la llamada HTTP
// Quien la procesa → Service: decide que hacer con la llamada
// Quien maneja los errores → ExceptionHandlers: categoriza los errores que se pueden presentar

using Products.API.Data;
using Products.API.ExceptionHandlers;
using Products.API.Services;
using Serilog;
using System.Reflection;

// configura Serilog antes de que arranque la aplicacion
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/products-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// configura Swagger para leer los XML comments y mostrar la documentacion de cada endpoint
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Products API",
        Version = "v1",
        Description = "API para gestión de productos del eCommerce. Permite crear, actualizar, eliminar y consultar productos."
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddHealthChecks();

// el DatabaseInitializer crea la tabla products en la base de datos cuando arranca la app
// si la tabla ya existe no hace nada
builder.Services.AddSingleton<DatabaseInitializer>();

// el ProductRepository maneja todas las operaciones con la base de datos SQLite
// AddScoped crea un ProductRepository nuevo por cada llamada HTTP
builder.Services.AddScoped<ProductRepository>();

// el ProductService contiene toda la logica de negocio
// AddScoped crea un ProductService nuevo por cada llamada HTTP
builder.Services.AddScoped<ProductService>();

// HttpClient para consultar a Orders.API y verificar si el producto tiene ordenes activas (PRD-004)
builder.Services.AddHttpClient("OrdersAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7168/");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

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

// inicializa la base de datos al arrancar la aplicacion
// crea la tabla products en el archivo products.db si no existe
app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORRELATION ID
// cada llamada HTTP recibe un id unico para poder rastrearla en los logs
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