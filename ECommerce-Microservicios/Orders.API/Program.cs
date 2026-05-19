//Cuando llega una llamada HTTP por ej 'crear order'. el sistema debe saber:
//Quien la recibe → Controller: recibe la llamada HTTP
//Quien la procesa → Service: decide que hacer con la llamada
//Quien maneja los errores → ExceptionHandlers: categoriza los errores que se pueden presentar

//Los builder.services.add configuran quien se encarga de que cosa

//Program.cs es donde le avisa al sistema que esas tres cosas existen antes
//de que llegue cualquier llamada.

//nombres de las carpetas de las clases que se nombran en program
using Orders.API.ExceptionHandlers;
using Orders.API.Services;
//Serilog es una libreria externa que guarda registros de todo lo que pasa en el sistema
//por ej cuando llega un POST, cuando ocurre un error, etc
using Serilog;

//configura Serilog antes de que arranque la aplicacion
//WriteTo.Console() → muestra los logs en la consola de Visual Studio
//WriteTo.File() → guarda los logs en un archivo dentro de la carpeta logs/
//RollingInterval.Day → crea un archivo de log nuevo por dia
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/orders-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

//en esta variable se crea la aplicacion Orders.API,
//→la app se guarda en un builder para que pueda configurarla antes de arrancarla
//→con builder.Services.Add... por ej quien recibe la llamada, quien va a tener el swagger, que va a tener disponible el orderservice y quien va a saber manejar los errores
//WebApplication es la clase .NET que representa una aplicacion web
//CreateBuilder es el metodo para crear el constructor de esa aplicacion
var builder = WebApplication.CreateBuilder(args);

//le dice a la aplicacion que use Serilog para registrar todo lo que pasa
builder.Host.UseSerilog();

//CONTROLLERS
//configuraciones con builder.services.add ..
//Le avisamos a la API que va a recibir llamadas HTTP, se prepara para recibir por ej POST /api/orders
builder.Services.AddControllers();

//hacemos que la app pueda leer y entender todos los endpoints del proyecto
//GET  /api/orders
//POST /api/orders
//GET  /api/orders/{id}
//PUT  /api/orders/{id}/status
builder.Services.AddEndpointsApiExplorer();
//genera el swagger, lee la info de la linea anterior y lo convierte en botones POST, etc
builder.Services.AddSwaggerGen();

//se activa el health checks que permite saber si la aplicacion esta funcionando correctamente
//se puede consultar en /health, /health/ready y /health/live
//GET /health → estado general de la aplicación. Responde Healthy si la app está corriendo.
//GET / health / ready → si la app está lista para recibir llamadas. En un sistema real verificaría si la base de datos está conectada, si los servicios externos responden, etc. Por ahora como no tenemos base de datos real, siempre responde Healthy.
//GET /health/live → si la app está viva. Es el más básico — solo verifica que el proceso está corriendo. Siempre responde Healthy mientras la app esté levantada.
builder.Services.AddHealthChecks();

//ACA USA LA CLASE DE LA CARPETA SERVICES
//el AddScoped crea un OrderService por cada llamada HTTP
//cada vez que el Controller recibe una llamada HTTP, esta linea le pasa automaticamente el OrderService
//por ej cuando llega un POST /api/orders, el OrderService crea la orden y le asigna estado y la guarda.
builder.Services.AddScoped<OrderService>();

//ACA USA LA CLASE DE LA CARPETA EXCEPTIONHANDLERS
//Existen 4 manejadores de errores si el OrderService detecta un problema
//estas lineas solo anotan, no hacen nada.
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

//Cada vez que se lance un error, esta linea atrapa ese error y
//busca en la lista de handlers quien puede manejarlo. Activa los handlers y devuelve el JSON.
app.UseExceptionHandler();

//verifica si la aplicacion esta corriendo desde Visual Studio en modo desarrollo
//si esta en desarrollo activa el swagger
//activa la interfaz visual en el navegador y estaria listo para PROBAR
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//redirige las llamadas HTTP a HTTPS para que sean seguras
app.UseHttpsRedirection();

//CORRELATION ID
//cada llamada HTTP que llega recibe un id unico llamado Correlation ID
//si la llamada ya trae un Correlation ID lo usa, si no lo genera automaticamente
//ese id se agrega a todos los logs de esa llamada para poder rastrearla
//por ej si ocurre un error, con el Correlation ID podes encontrar todos los logs de esa llamada
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