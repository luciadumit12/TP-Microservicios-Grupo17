namespace Cart.API.DTOs
{
    /// <summary>
    /// Datos necesarios para actualizar un producto del carrito.
    /// </summary>
    public class UpdateCartItemRequest
    {
        /// <summary>
        /// Nueva cantidad de unidades.
        /// </summary>
        /// <example>4</example>
        public int Cantidad { get; set; }
    }
}