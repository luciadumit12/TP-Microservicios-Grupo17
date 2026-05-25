//El Controller es la puerta de entrada de Orders.API
//Cuando llega una llamada HTTP, el Controller la recibe y se la pasa al OrderService
//El Controller no decide nada, solo recibe y delega
using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers
{
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
        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        //4 ENDPOINTS: busca las ordenes por usuario, busca por orden especifica, crea ordenes, cambia el estado de la orden

        //ENDPOINT 1: GET /api/orders
        //recibe una llamada GET y le pide al OrderService todas las ordenes
        //si viene un usuarioId en la URL filtra por ese usuario
        //devuelve 200 con la lista de ordenes
        /// <summary>Lista todas las ordenes. Se puede filtrar por usuario usando el parametro usuarioId</summary>
        /// <param name="usuarioId">ID del usuario para filtrar sus ordenes. Si no se envia, devuelve todas las ordenes</param>
        /// <remarks>
        /// Ejemplo de Exito (200 OK):
        ///
        ///     [
        ///       {
        ///         "id": "4fa6a8f0-872e-4217-b91b-58d4b963bafc",
        ///         "usuarioId": "aa863f64-5e21-44ee-9d14-50e4c60e26b2",
        ///         "items": [
        ///           {
        ///             "productoId": "21a35e84-e1ad-4b17-b2ea-3b0598322a96",
        ///             "cantidad": 1,
        ///             "precioUnitario": 15000
        ///           }
        ///         ],
        ///         "total": 15000,
        ///         "estado": "Pendiente",
        ///         "fechaCreacion": "2026-05-24T20:26:30Z"
        ///       }
        ///     ]
        ///
        /// Ejemplo de Error (500 - Error interno - ORD-007):
        ///
        ///     {
        ///       "errorCode": "ORD-007",
        ///       "errorMessage": "Error interno al procesar la orden.",
        ///       "status": 500
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Lista de ordenes encontradas</response>
        /// <response code="500">Error interno del servidor (ORD-007)</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? usuarioId)
        {
            var ordenes = await _orderService.ObtenerTodas(usuarioId);
            return Ok(ordenes);
        }

        //ENDPOINT 2: GET /api/orders/{id}
        //recibe una llamada GET con un id especifico en la URL
        //devuelve 200 con la orden o 404 si no existe
        /// <summary>Obtiene una orden especifica por su ID</summary>
        /// <param name="id">ID unico de la orden</param>
        /// <remarks>
        /// Ejemplo de Exito (200 OK):
        ///
        ///     {
        ///       "id": "4fa6a8f0-872e-4217-b91b-58d4b963bafc",
        ///       "usuarioId": "aa863f64-5e21-44ee-9d14-50e4c60e26b2",
        ///       "items": [
        ///         {
        ///           "productoId": "21a35e84-e1ad-4b17-b2ea-3b0598322a96",
        ///           "cantidad": 1,
        ///           "precioUnitario": 15000
        ///         }
        ///       ],
        ///       "total": 15000,
        ///       "estado": "Pendiente",
        ///       "fechaCreacion": "2026-05-24T20:26:30Z"
        ///     }
        ///
        /// Ejemplo de Error (404 - Orden no encontrada - ORD-001):
        ///
        ///     {
        ///       "errorCode": "ORD-001",
        ///       "errorMessage": "Orden no encontrada.",
        ///       "status": 404
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Orden encontrada exitosamente</response>
        /// <response code="404">Orden no encontrada (ORD-001)</response>
        /// <response code="500">Error interno del servidor (ORD-007)</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var orden = await _orderService.ObtenerPorId(id);
            return Ok(orden);
        }

        //ENDPOINT 3: POST /api/orders
        //recibe una llamada POST con los datos de la orden en el body
        //valida usuario, productos y stock antes de crear la orden
        //devuelve 201 con la orden creada
        /// <summary>Crea una nueva orden de compra. Valida que el usuario exista, que los productos existan y que haya stock suficiente</summary>
        /// <param name="request">Datos de la orden: usuarioId y lista de items con productoId y cantidad</param>
        /// <remarks>
        /// Ejemplo de Exito (201 Created):
        ///
        ///     {
        ///       "id": "4fa6a8f0-872e-4217-b91b-58d4b963bafc",
        ///       "usuarioId": "aa863f64-5e21-44ee-9d14-50e4c60e26b2",
        ///       "items": [
        ///         {
        ///           "productoId": "21a35e84-e1ad-4b17-b2ea-3b0598322a96",
        ///           "cantidad": 1,
        ///           "precioUnitario": 15000
        ///         }
        ///       ],
        ///       "total": 15000,
        ///       "estado": "Pendiente",
        ///       "fechaCreacion": "2026-05-24T20:26:30Z"
        ///     }
        ///
        /// Ejemplo de Error (400 - Datos invalidos - ORD-002):
        ///
        ///     {
        ///       "errorCode": "ORD-002",
        ///       "errorMessage": "Los datos de la orden son invalidos.",
        ///       "status": 400
        ///     }
        ///
        /// Ejemplo de Error (404 - Usuario no encontrado - ORD-003):
        ///
        ///     {
        ///       "errorCode": "ORD-003",
        ///       "errorMessage": "Usuario no encontrado al crear la orden.",
        ///       "status": 404
        ///     }
        ///
        /// Ejemplo de Error (404 - Producto no encontrado - ORD-004):
        ///
        ///     {
        ///       "errorCode": "ORD-004",
        ///       "errorMessage": "Producto no encontrado al crear la orden.",
        ///       "status": 404
        ///     }
        ///
        /// Ejemplo de Error (422 - Stock insuficiente - ORD-005):
        ///
        ///     {
        ///       "errorCode": "ORD-005",
        ///       "errorMessage": "Stock insuficiente para el producto solicitado.",
        ///       "status": 422
        ///     }
        ///
        /// </remarks>
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
        //recibe una llamada PUT con el id de la orden y el nuevo estado en el body
        //devuelve 200 con la orden actualizada o 409 si la transicion no es valida
        /// <summary>Actualiza el estado de una orden. Los estados validos son: Pendiente, Confirmada, Enviada, Entregada, Cancelada</summary>
        /// <param name="id">ID unico de la orden</param>
        /// <param name="request">Nuevo estado de la orden</param>
        /// <remarks>
        /// Ejemplo de Exito (200 OK):
        ///
        ///     {
        ///       "id": "4fa6a8f0-872e-4217-b91b-58d4b963bafc",
        ///       "usuarioId": "aa863f64-5e21-44ee-9d14-50e4c60e26b2",
        ///       "items": [
        ///         {
        ///           "productoId": "21a35e84-e1ad-4b17-b2ea-3b0598322a96",
        ///           "cantidad": 1,
        ///           "precioUnitario": 15000
        ///         }
        ///       ],
        ///       "total": 15000,
        ///       "estado": "Confirmada",
        ///       "fechaCreacion": "2026-05-24T20:26:30Z"
        ///     }
        ///
        /// Ejemplo de Error (404 - Orden no encontrada - ORD-001):
        ///
        ///     {
        ///       "errorCode": "ORD-001",
        ///       "errorMessage": "Orden no encontrada.",
        ///       "status": 404
        ///     }
        ///
        /// Ejemplo de Error (409 - Transicion invalida - ORD-006):
        ///
        ///     {
        ///       "errorCode": "ORD-006",
        ///       "errorMessage": "Una orden en estado 'Entregada' no puede cambiar a 'Pendiente'.",
        ///       "status": 409
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Estado actualizado exitosamente</response>
        /// <response code="404">Orden no encontrada (ORD-001)</response>
        /// <response code="409">Transicion de estado invalida (ORD-006)</response>
        /// <response code="500">Error interno del servidor (ORD-007)</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var orden = await _orderService.ActualizarEstado(id, request);
            return Ok(orden);
        }
    }
}