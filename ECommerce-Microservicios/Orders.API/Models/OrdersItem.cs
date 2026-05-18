//ORDERITEM.CS
//representa un producto dentro de una orden
//una orden puede tener muchos OrderItems
namespace Orders.API.Models
{
    public class OrderItem
    {
        //id del producto comprado
        public Guid ProductoId { get; set; }
        //cuantos se compraron
        public int Cantidad { get; set; }
        //precio del producto al momento de crear la orden
        //se congela en ese momento, si el producto cambia de precio la orden no se modifica
        public decimal PrecioUnitario { get; set; }
    }
}