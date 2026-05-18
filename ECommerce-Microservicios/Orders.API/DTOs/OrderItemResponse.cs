//ORDERITEMRESPONSE.CS
//El item dentro del OrderResponse, aca si incluye PrecioUnitario
//porque cuando el sistema responde le muestra al cliente cuanto costo cada producto porque es lo que devuelve el sistema 
namespace Orders.API.DTOs
{
    public class OrderItemResponse
    {
        //id del producto
        public Guid ProductoId { get; set; }
        //cantidad comprada
        public int Cantidad { get; set; }
        //precio del producto al momento de crear la orden
        //este precio queda fijo, si el producto cambia de precio la orden no se modifica
        public decimal PrecioUnitario { get; set; }
    }
}