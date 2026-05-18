//ORDERITEMREQUEST.CS
//El item dentro del CreateOrderRequest
//solo el id del producto y cuantos quiere
//no incluye PrecioUnitario porque es lo que manda el usuario 
namespace Orders.API.DTOs
{
    public class OrderItemRequest
    {
        //id del producto que quiere comprar
        public Guid ProductoId { get; set; }
        //cuantos quiere comprar
        public int Cantidad { get; set; }
    }
}

