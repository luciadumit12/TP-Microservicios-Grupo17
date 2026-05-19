using Cart.API.ExceptionHandlers;
using Cart.API.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/cart-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddScoped<CartService>();

// HttpClient para consultar a Products.API y verificar si el producto existe y tiene stock (CRT-002, CRT-003)
builder.Services.AddHttpClient("ProductsAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7268/");
});

// cuando algo no se encuentra: CRT-001, CRT-002 → devuelve 404
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
// cuando los datos enviados son invalidos: CRT-004 → devuelve 400
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
// cuando se viola una regla de negocio: CRT-003 → devuelve 422
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
// cuando ocurre cualquier error inesperado: CRT-005 → devuelve 500
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