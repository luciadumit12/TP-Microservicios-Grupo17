//Cuando llega una llamada HTTP por ej 'crear orden'. el sistema debe saber:
//Quien la recibe → Controller: recibe la llamada HTTP
//Quien la procesa → Service: decide que hacer con la llamada
//Quien maneja los errores → ExceptionHandlers: categoriza los errores que se pueden presentar

//Los builder.services.add configuran quien se encarga de que cosa

//Program.cs es donde le avisa al sistema que esas tres cosas existen antes
//de que llegue cualquier llamada.

using Orders.API.Data;
using Orders.API.ExceptionHandlers;
using Orders.API.Services;
using Orders.API.SwaggerFilters;
//Serilog es una libreria externa que guarda registros de todo lo que pasa en el sistema
//por ej cuando llega un POST, cuando ocurre un error, etc
using Serilog;

// ─────────────────────────────
// CONFIGURAR SERILOG
// consola → formato legible para desarrollo
// archivo → formato JSON estructurado para produccion
// Enrich.WithProperty agrega el nombre del servicio a todos los logs
// Enrich.FromLogContext permite que el CorrelationId se incluya en cada log
// ─────────────────────────────
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Orders.API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        new Serilog.Formatting.Json.JsonFormatter(),
        "logs/orders-.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

//en esta variable se crea la aplicacion Orders.API
//WebApplication es la clase .NET que representa una aplicacion web
//CreateBuilder es el metodo para crear el constructor de esa aplicacion
var builder = WebApplication.CreateBuilder(args);

//le dice a la aplicacion que use Serilog para registrar todo lo que pasa
builder.Host.UseSerilog();

//CONTROLLERS
//Le avisamos a la API que va a recibir llamadas HTTP
builder.Services.AddControllers();

//permite que los Services puedan acceder al HttpContext actual
//se usa para leer el CorrelationId y propagarlo en las llamadas salientes
builder.Services.AddHttpContextAccessor();

//hacemos que la app pueda leer y entender todos los endpoints del proyecto
//GET  /api/orders
//POST /api/orders
//GET  /api/orders/{id}
//PUT  /api/orders/{id}/status
builder.Services.AddEndpointsApiExplorer();

//genera el swagger, lee la info de la linea anterior y lo convierte en botones POST, etc
//tambien le dice a swagger que lea el archivo XML que genera el .csproj
//ese archivo XML contiene todos los comentarios /// del Controller
//sin esto swagger no muestra las descripciones ni los ejemplos de cada endpoint
builder.Services.AddSwaggerGen(options =>
{
    //titulo, version y descripcion de la API que aparecen en la cabecera del Swagger
    options.SwaggerDoc("v1", new()
    {
        Title = "Orders.API",
        Version = "v1",
        Description = "API para gestion de ordenes del eCommerce."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    //registra el filtro que pone los ejemplos reales en la seccion Responses de cada endpoint
    options.OperationFilter<OrdersOperationFilter>();
});

//se activa el health checks que permite saber si la aplicacion esta funcionando correctamente
//GET /health → estado general de la aplicacion. Responde Healthy si la app esta corriendo.
//GET /health/ready → si la app esta lista para recibir llamadas.
//GET /health/live → si la app esta viva. Solo verifica que el proceso esta corriendo.
builder.Services.AddHealthChecks();

//ACA USA LA CLASE DE LA CARPETA DATA
//el DatabaseInitializer crea las tablas en la base de datos cuando arranca la app
//si las tablas ya existen no hace nada
builder.Services.AddSingleton<DatabaseInitializer>();

//el OrderRepository maneja todas las operaciones con la base de datos SQLite
//el AddScoped crea un OrderRepository nuevo por cada llamada HTTP
builder.Services.AddScoped<OrderRepository>();

//ACA USA LA CLASE DE LA CARPETA SERVICES
//el AddScoped crea un OrderService por cada llamada HTTP
//cada vez que el Controller recibe una llamada HTTP, esta linea le pasa automaticamente el OrderService
builder.Services.AddScoped<OrderService>();

//ACA SE CONFIGURA LA CONEXION CON USERS.API
//cuando el OrderService necesite verificar si un usuario existe antes de crear una orden, usa este HttpClient
//sin esta conexion no se puede validar ORD-003 → usuario no encontrado
//BaseAddress es la direccion donde esta corriendo Users.API
builder.Services.AddHttpClient("UsersAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7075/");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

//ACA SE CONFIGURA LA CONEXION CON PRODUCTS.API
//cuando el OrderService necesite verificar si un producto existe y obtener su precio y stock, usa este HttpClient
//sin esta conexion no se puede validar ORD-004 → producto no encontrado
//ni ORD-005 → stock insuficiente
//BaseAddress es la direccion donde esta corriendo Products.API
builder.Services.AddHttpClient("ProductsAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7268/");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

//ACA USA LA CLASE DE LA CARPETA EXCEPTIONHANDLERS
//Existen 4 manejadores de errores si el OrderService detecta un problema
//estas lineas solo anotan, no hacen nada todavia
//cuando algo no se encuentra: ORD-001 (orden), ORD-003 (usuario), ORD-004 (producto) → devuelve 404
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
//cuando los datos enviados son invalidos: ORD-002 → por ej cuando se crea una orden sin items → devuelve 400
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
//cuando se viola una regla de negocio:
//ORD-005 → stock insuficiente → devuelve 422
//ORD-006 → transicion de estado invalida → devuelve 409
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
//cuando ocurre cualquier error inesperado que los otros tres no pudieron manejar: ORD-007 → devuelve 500
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
//gracias a esto se escribe el error en formato estandar JSON
builder.Services.AddProblemDetails();

//toma toda la configuracion que hizo el builder y construye la aplicacion
//despues de aca no se puede configurar mas nada.
var app = builder.Build();

//inicializa la base de datos al arrancar la aplicacion
//crea las tablas orders y order_items en el archivo orders.db si no existen
//si las tablas ya existen no hace nada
app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

//verifica si la aplicacion esta corriendo desde Visual Studio en modo desarrollo
//si esta en desarrollo activa el swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//redirige las llamadas HTTP a HTTPS para que sean seguras
app.UseHttpsRedirection();

// ─────────────────────────────
// CORRELATION ID — va ANTES de UseExceptionHandler
// cada llamada HTTP que llega recibe un id unico llamado Correlation ID
// si la llamada ya trae un Correlation ID lo usa, si no lo genera automaticamente
// ese id se guarda en context.Items para que los handlers lo puedan leer cuando hay un error
// tambien se propaga en las llamadas HTTP salientes a Users.API y Products.API
// ─────────────────────────────
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();
    //lo guardamos en Items para que los handlers lo lean cuando hay un error
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

//Cada vez que se lance un error, esta linea atrapa ese error y
//busca en la lista de handlers quien puede manejarlo. Activa los handlers y devuelve el JSON.
//va DESPUES del Correlation ID para que los handlers ya tengan el ID disponible
app.UseExceptionHandler();

//activa el logging automatico de Serilog para cada llamada HTTP
//registra cuando llego la llamada, cuanto tardo en procesarse y que resultado devolvio
app.UseSerilogRequestLogging();

//cuando llega una llamada HTTP con su URL, esta linea se encarga de
//mandar al controller correspondiente
app.MapControllers();

//activa los endpoints de health checks para saber si la aplicacion esta funcionando
//GET /health → estado general de la aplicacion
//GET /health/ready → si la aplicacion esta lista para recibir llamadas
//GET /health/live → si la aplicacion esta viva y corriendo
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

//despues de configurar todo, arranca la aplicacion para
//que se puedan recibir las llamadas
app.Run();