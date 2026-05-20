namespace Cart.API.DTOs
{
    /// <summary>
    /// Representa un producto dentro del carrito.
    /// </summary>
    public class CartItemResponse
    {
        /// <summary>
        /// ID del producto.
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Cantidad del producto en el carrito.
        /// </summary>
        /// <example>2</example>
        public int Cantidad { get; set; }
    }
}