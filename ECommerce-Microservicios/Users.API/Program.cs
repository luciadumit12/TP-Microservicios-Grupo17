// Punto de entrada de la aplicación.
// Acá se registran todos los servicios, handlers, Swagger, etc.

using Serilog;
using Users.API.ExceptionHandlers;
using Users.API.Services;

// Configurar Serilog — escribe logs en consola y en archivo JSON
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/users-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Reemplazar el logger default por Serilog
builder.Host.UseSerilog();

// Registrar el Service para que el Controller pueda recibirlo por inyección
builder.Services.AddScoped<UserService>();

// Registrar los ExceptionHandlers en orden
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Registrar Controllers
builder.Services.AddControllers();

// Registrar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Activar el manejo global de excepciones — options => {} es obligatorio
app.UseExceptionHandler(options => { });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middleware de Serilog — loggea inicio y fin de cada request con duración
app.UseSerilogRequestLogging();

app.MapControllers();

// Endpoints de Health Checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();