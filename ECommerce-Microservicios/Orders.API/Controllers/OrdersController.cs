//El Controller es la puerta de entrada de Orders.API
//Cuando llega una llamada HTTP, el Controller la recibe y se la pasa al OrderService
//El Controller no decide nada, solo recibe y delega

//nombres de las carpetas de las clases que se nombran en este archivo
using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers
{
    //esta clase es el Controller de Orders.API
    //ACA SE DEFINEN LOS 4 ENDPOINTS DE LA API
    //[ApiController] le dice a .NET que esta clase recibe llamadas HTTP
    [ApiController]
    //[Route("api/orders")] define la URL base de todos los endpoints de esta clase
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        //guarda el orderservice en una variable para que los controllers puedan pasarle las llamadas HTTP al orderservice para que la procese, 
        private readonly OrderService _orderService;

        //recibe el orderservice que .net entrega automaticamente 
        //gracias a que lo registramos en Program.cs con AddScoped<OrderService>()
        //y lo guarda en la variable para que los metodos puedan usarlo
        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        //4 ENDPOINTS: busca las ordenes por usuario, busca por orden especifica, crea ordenes, cambia el estado de la orden 

        //ENDPOINT 1: GET /api/orders
        //recibe una llamada GET y le pide al OrderService todas las ordenes
        //si viene un usuarioId en la URL por ej GET /api/orders?usuarioId=123, filtra por ese usuario
        //si no viene ningun usuarioId, devuelve todas las ordenes
        //devuelve 200 con la lista de ordenes
        [HttpGet]
        public IActionResult GetAll([FromQuery] Guid? usuarioId)
        {
            var ordenes = _orderService.ObtenerTodas(usuarioId);
            return Ok(ordenes);
        }

        //ENDPOINT 2: GET /api/orders/{id}
        //recibe una llamada GET con un id especifico en la URL por ej GET /api/orders/3fa85f64
        //le pide al OrderService esa orden especifica
        //si la orden existe devuelve 200 con la orden
        //si no existe el OrderService lanza NotFoundException y el handler devuelve 404
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var orden = _orderService.ObtenerPorId(id);
            return Ok(orden);
        }

        //ENDPOINT 3: POST /api/orders
        //recibe una llamada POST con los datos de la orden en el body
        //le pide al OrderService que cree la orden
        //devuelve 201 con la orden creada
        [HttpPost]
        public IActionResult Create([FromBody] CreateOrderRequest request)
        {
            var orden = _orderService.CrearOrden(request);
            return CreatedAtAction(nameof(GetById), new { id = orden.Id }, orden);
        }

        //ENDPOINT 4: PUT /api/orders/{id}/status
        //recibe una llamada PUT con el id de la orden en la URL y el nuevo estado en el body
        //le pide al OrderService que cambie el estado de esa orden
        //si la transicion de estado es valida devuelve 200 con la orden actualizada
        //si no es valida el OrderService lanza BusinessRuleException y el handler devuelve 409
        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var orden = _orderService.ActualizarEstado(id, request);
            return Ok(orden);
        }
    }
}