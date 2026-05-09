namespace Orders.API.DTOs
{
    public class OrderItemRequest
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}
