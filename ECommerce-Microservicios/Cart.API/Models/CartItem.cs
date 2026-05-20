namespace Cart.API.Models
{
    /// <summary>
    /// Modelo que representa un producto dentro del carrito.
    /// </summary>
    public class CartItem
    {
        /// <summary>
        /// ID del producto.
        /// </summary>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Cantidad del producto.
        /// </summary>
        public int Cantidad { get; set; }
    }
}