using Cart.API.Data;
using Cart.API.ExceptionHandlers;
using Cart.API.Services;
using Serilog;
using System.Reflection;

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

builder.Services.AddHealthChecks();

builder.Services.AddScoped<CartService>();

// HttpClient para consultar Products.API
builder.Services.AddHttpClient("ProductsAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7268/");
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

builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<CartRepository>();
var app = builder.Build();

app.UseExceptionHandler();

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

app.MapControllers();


// =========================
// Health Checks
// =========================
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();