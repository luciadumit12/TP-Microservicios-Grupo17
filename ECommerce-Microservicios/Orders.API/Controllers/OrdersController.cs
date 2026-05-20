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
    //[Route("api/orders")] define la URL base de todos los endpoints de esta clase

    /// <summary>
    /// Maneja todas las operaciones relacionadas con ordenes de compra
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        //guarda el OrderService en una variable para que todos los metodos del Controller puedan pasarle las llamadas HTTP
        private readonly OrderService _orderService;

        //recibe el OrderService que .NET entrega automaticamente
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
        /// <summary>Lista todas las ordenes. Se puede filtrar por usuario usando el parametro usuarioId</summary>
        /// <param name="usuarioId">ID del usuario para filtrar sus ordenes. Si no se envia, devuelve todas las ordenes</param>
        /// <response code="200">Lista de ordenes encontradas</response>
        /// <response code="500">Error interno del servidor (ORD-007)</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), 200)]
        [ProducesResponseType(500)]
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
        /// <summary>Obtiene una orden especifica por su ID</summary>
        /// <param name="id">ID unico de la orden</param>
        /// <response code="200">Orden encontrada exitosamente</response>
        /// <response code="404">Orden no encontrada (ORD-001)</response>
        /// <response code="500">Error interno del servidor (ORD-007)</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult GetById(Guid id)
        {
            var orden = _orderService.ObtenerPorId(id);
            return Ok(orden);
        }

        //ENDPOINT 3: POST /api/orders
        //recibe una llamada POST con los datos de la orden en el body
        //le pide al OrderService que valide el usuario, los productos y el stock antes de crear la orden
        //el async/await significa que espera las respuestas de Users.API y Products.API antes de continuar
        //devuelve 201 con la orden creada
        /// <summary>Crea una nueva orden de compra. Valida que el usuario exista, que los productos existan y que haya stock suficiente</summary>
        /// <param name="request">Datos de la orden: usuarioId y lista de items con productoId y cantidad</param>
        /// <response code="201">Orden creada exitosamente</response>
        /// <response code="400">Datos invalidos, por ej lista de items vacia (ORD-002)</response>
        /// <response code="404">Usuario no encontrado (ORD-003) o producto no encontrado (ORD-004)</response>
        /// <response code="422">Stock insuficiente para uno o mas productos (ORD-005)</response>
        /// <response code="500">Error interno del servidor (ORD-007)</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            var orden = await _orderService.CrearOrden(request);
            return CreatedAtAction(nameof(GetById), new { id = orden.Id }, orden);
        }

        //ENDPOINT 4: PUT /api/orders/{id}/status
        //recibe una llamada PUT con el id de la orden en la URL y el nuevo estado en el body
        //le pide al OrderService que cambie el estado de esa orden
        //si la transicion de estado es valida devuelve 200 con la orden actualizada
        //si no es valida el OrderService lanza BusinessRuleException y el handler devuelve 409
        /// <summary>Actualiza el estado de una orden. Los estados validos son: Pendiente, Confirmada, Enviada, Entregada, Cancelada</summary>
        /// <param name="id">ID unico de la orden</param>
        /// <param name="request">Nuevo estado de la orden</param>
        /// <response code="200">Estado actualizado exitosamente</response>
        /// <response code="404">Orden no encontrada (ORD-001)</response>
        /// <response code="409">Transicion de estado invalida (ORD-006)</response>
        /// <response code="500">Error interno del servidor (ORD-007)</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var orden = _orderService.ActualizarEstado(id, request);
            return Ok(orden);
        }
    }
}