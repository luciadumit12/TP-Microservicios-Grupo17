//ORDERRESPONSE.CS
//lo que la API devuelve en cualquier endpoint exitoso (GET, POST, PUT)
//incluye todo: id, items con precios, total, estado y fecha
//es lo que ve el cliente cuando hace una llamada exitosa
namespace Orders.API.DTOs
{
    public class OrderResponse
    {
        //id unico de la orden generado por el sistema
        public Guid Id { get; set; }
        //id del usuario que hizo la orden
        public Guid UsuarioId { get; set; }
        //lista de productos con sus precios
        public List<OrderItemResponse> Items { get; set; } = new();
        //total calculado automaticamente por el sistema
        public decimal Total { get; set; }
        //estado actual de la orden: Pendiente, Confirmada, Enviada, Entregada o Cancelada
        public string Estado { get; set; } = string.Empty;
        //fecha y hora en que se creo la orden
        public DateTime FechaCreacion { get; set; }
    }
}