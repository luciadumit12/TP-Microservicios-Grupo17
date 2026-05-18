//Cuando llega una llamada HTTP por ej 'crear order'. el sistema debe saber:
//Quien la recibe  ? Controller:  recibe la llamada HTTP
//Quien la procesa ? Service:  decide que hacer con la llamada 
//Quien maneja los errores ? ExceptionHandlers:  categoriza los errores que se pueden presentar 

//Los builder.services.add configuran quien se encarga de que cosa

//Program.cs es donde le avisa al sistema que esas tres cosas existen antes
//de que llegue cualquier llamada.


//nombres de las carpetas de las clases que se nombran en program
using Orders.API.ExceptionHandlers;
using Orders.API.Services;

//en esta variable se crea la aplicacion Orders.API,
//?la app se guarda en un builder para que pueda configurarla antes de arrancarla
//?con builder.Services.Add... por ej quien recibe la llamada, quien va a tener el swagger, que va a tener disponible el orderservice y quiero va a saber manejar los errores
//WebApplication es la clase .NET que representa una aplicacion web
//CreateBuilder es el metodo para crear el constructor de esa aplicacion 
var builder = WebApplication.CreateBuilder(args);

//CONTROLLERS 
//configuraciones con builder.services.add .. 
//Le avisamos a la API que va a recibir llamadas HTTP, se prepara para recibir por ej POST /api/orders
builder.Services.AddControllers();


//hacemos que la app pueda leer y entender todos los endpoints del proyecto 
//GET  /api/orders
//POST / api / orders
//GET / api / orders /{ id}
//PUT / api / orders /{ id}/ status
builder.Services.AddEndpointsApiExplorer();
//genera el swagger, lee la info de la linea anterior y lo convierte en botones POST, etc
builder.Services.AddSwaggerGen();

//ACA USA LA CLASE DE LA CARPETA SERVICES
//el AddScoped crea un orderservice por cada llamada de HTTP
//cada vez que controller recibe una llamada HTTP, esta linea le pasa automaticamente a OrdersService ya que sabe que hacer con ella llamada
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

//verifica esta en entorno de desarrollo, no para usuarios reales.
//esa linea verifica cual de los dos modos (desarrollo o produccion) se esta corriendo
//si la aplicacion esta corriendo:
//activa el swagger 
//activa la interfaz visual en el navegador y estaria listo para PROBAR
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Cada vez que se lance un error, esta linea atrapa ese error y 
//buscá en la lista de handlers quién puede manejarlo. Activa los handlers y devuelve el JSON.
app.UseExceptionHandler();

//cuando llega una llamada HTTP con su URL, esta linea se encarga de 
//mandar al controller correspondiente 
app.MapControllers();

//despues de configurar todo, arranca la aplicacion para
//que se puedan recibir las llamadas 
app.Run();
