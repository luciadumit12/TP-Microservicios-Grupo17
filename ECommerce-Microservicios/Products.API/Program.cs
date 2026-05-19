using Products.API.ExceptionHandlers;
using Products.API.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/products-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddScoped<ProductService>();

// HttpClient para consultar a Orders.API y verificar si el producto tiene órdenes activas (PRD-004)
builder.Services.AddHttpClient("OrdersAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7168/");
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

builder.Services.AddHealthChecks();
var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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