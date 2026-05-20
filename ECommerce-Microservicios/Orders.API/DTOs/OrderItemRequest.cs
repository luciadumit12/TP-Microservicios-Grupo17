//ORDERITEMREQUEST.CS
//El item dentro del CreateOrderRequest
//solo el id del producto y cuantos quiere
//no incluye PrecioUnitario porque el precio lo busca el sistema en Products.API
namespace Orders.API.DTOs
{
    /// <summary>
    /// Un producto dentro de la orden con su cantidad
    /// </summary>
    public class OrderItemRequest
    {
        //id del producto que quiere comprar
        /// <summary>ID del producto que quiere comprar</summary>
        public Guid ProductoId { get; set; }

        //cuantos quiere comprar
        /// <summary>Cantidad de unidades que quiere comprar</summary>
        public int Cantidad { get; set; }
    }
}