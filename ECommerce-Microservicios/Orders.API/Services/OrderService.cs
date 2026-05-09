using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;

namespace Orders.API.Services
{
    public class OrderService
    {
        private readonly List<Order> _ordenes = new();

        private static readonly Dictionary<string, List<string>> TransicionesValidas = new()
        {
            { "Pendiente",  new() { "Confirmada", "Cancelada" } },
            { "Confirmada", new() { "Enviada", "Cancelada" } },
            { "Enviada",    new() { "Entregada" } },
            { "Entregada",  new() { } },
            { "Cancelada",  new() { } }
        };

        public List<OrderResponse> ObtenerTodas(Guid? usuarioId)
        {
            var ordenes = _ordenes.AsEnumerable();

            if (usuarioId.HasValue)
                ordenes = ordenes.Where(o => o.UsuarioId == usuarioId.Value);

            return ordenes.Select(MapearAResponse).ToList();
        }

        public OrderResponse ObtenerPorId(Guid id)
        {
            var orden = _ordenes.FirstOrDefault(o => o.Id == id)
                ?? throw new NotFoundException("ORD-001", "Orden no encontrada.");

            return MapearAResponse(orden);
        }

        public OrderResponse CrearOrden(CreateOrderRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                throw new ValidationException("ORD-002", "Los datos de la orden son inválidos.");

            var items = request.Items.Select(i => new OrderItem
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = 0
            }).ToList();

            var orden = new Order
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Items = items,
                Total = items.Sum(i => i.Cantidad * i.PrecioUnitario),
                Estado = "Pendiente",
                FechaCreacion = DateTime.UtcNow
            };

            _ordenes.Add(orden);

            return MapearAResponse(orden);
        }

        public OrderResponse ActualizarEstado(Guid id, UpdateOrderStatusRequest request)
        {
            var orden = _ordenes.FirstOrDefault(o => o.Id == id)
                ?? throw new NotFoundException("ORD-001", "Orden no encontrada.");

            if (!TransicionesValidas[orden.Estado].Contains(request.Estado))
                throw new BusinessRuleException("ORD-006",
                    $"Una orden en estado '{orden.Estado}' no puede cambiar a '{request.Estado}'.");

            orden.Estado = request.Estado;

            return MapearAResponse(orden);
        }

        private static OrderResponse MapearAResponse(Order orden) => new()
        {
            Id = orden.Id,
            UsuarioId = orden.UsuarioId,
            Items = orden.Items.Select(i => new OrderItemResponse
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList(),
            Total = orden.Total,
            Estado = orden.Estado,
            FechaCreacion = orden.FechaCreacion
        };
    }
}
