namespace Orders.API.DTOs
{
    public class OrderItemResponse
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}