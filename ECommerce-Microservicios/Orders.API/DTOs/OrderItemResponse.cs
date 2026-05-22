//.ORDERITEMRESPONSE.CS
//El item dentro del OrderResponse, aca si incluye PrecioUnitario
//porque cuando el sistema responde le muestra al cliente cuanto costo cada producto
namespace Orders.API.DTOs
{
    /// <summary>
    /// Un producto dentro de la respuesta de la orden con su precio al momento de la compra
    /// </summary>
    public class OrderItemResponse
    {
        //id del producto
        /// <summary>ID del producto comprado</summary>
        public Guid ProductoId { get; set; }

        //cantidad comprada
        /// <summary>Cantidad de unidades compradas</summary>
        public int Cantidad { get; set; }

        //precio del producto al momento de crear la orden
        //este precio queda fijo, si el producto cambia de precio la orden no se modifica
        /// <summary>Precio unitario del producto al momento de crear la orden. Este precio queda fijo aunque el producto cambie de precio despues</summary>
        public decimal PrecioUnitario { get; set; }
    }
}