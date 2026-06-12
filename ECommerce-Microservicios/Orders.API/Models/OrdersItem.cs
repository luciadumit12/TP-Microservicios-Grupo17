//.ORDERITEM.CS
//representa un producto dentro de una orden
//una orden puede tener muchos OrderItems
namespace Orders.API.Models
{
    public class OrderItem
    {
        //id interno del item en la base de datos
        //lo genera el Repository automaticamente al guardar
        public Guid Id { get; set; }

        //id de la orden a la que pertenece este item
        //es la referencia que une el item con su orden en la base de datos
        //Dapper necesita esta propiedad para mapear el campo orderId de la tabla order_items
        public Guid OrderId { get; set; }

        //id del producto comprado
        public Guid ProductoId { get; set; }

        //cuantos se compraron
        public int Cantidad { get; set; }

        //precio del producto al momento de crear la orden
        //se congela en ese momento, si el producto cambia de precio la orden no se modifica
        public decimal PrecioUnitario { get; set; }
    }
}