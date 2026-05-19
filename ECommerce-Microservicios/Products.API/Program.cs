using System.Reflection;
using Products.API.ExceptionHandlers;
using Products.API.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/products-.log",
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
        Title = "Products API",
        Version = "v1",
        Description = "Microservicio encargado de la gestión de productos."
    });

    // Habilita XML Comments en Swagger
    var xmlFilename =
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    options.IncludeXmlComments(
        Path.Combine(AppContext.BaseDirectory, xmlFilename)
    );
});

builder.Services.AddHealthChecks();

builder.Services.AddScoped<ProductService>();

// HttpClient para consultar Orders.API
builder.Services.AddHttpClient("OrdersAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7168/");
});


// =========================
// Exception Handlers
// =========================

// PRD-001 → 404
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();

// PRD-002 → 400
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

// PRD-003 / PRD-004 → 409
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();

// PRD-005 → 500
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Products API v1");
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