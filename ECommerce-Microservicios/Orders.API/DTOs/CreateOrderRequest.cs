namespace Orders.API.DTOs
{
    public class CreateOrderRequest
    {
        public Guid UsuarioId { get; set; }
        public List<OrderItemRequest> Items { get; set; } = new();
    }
}