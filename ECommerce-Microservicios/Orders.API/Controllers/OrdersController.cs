using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] Guid? usuarioId)
        {
            var ordenes = _orderService.ObtenerTodas(usuarioId);
            return Ok(ordenes);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var orden = _orderService.ObtenerPorId(id);
            return Ok(orden);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateOrderRequest request)
        {
            var orden = _orderService.CrearOrden(request);
            return CreatedAtAction(nameof(GetById), new { id = orden.Id }, orden);
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var orden = _orderService.ActualizarEstado(id, request);
            return Ok(orden);
        }
    }
}