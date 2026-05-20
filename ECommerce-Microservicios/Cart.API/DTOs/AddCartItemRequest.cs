namespace Cart.API.DTOs
{
    /// <summary>
    /// Datos necesarios para agregar un producto al carrito.
    /// </summary>
    public class AddCartItemRequest
    {
        /// <summary>
        /// ID del producto a agregar.
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Cantidad de unidades a agregar.
        /// </summary>
        /// <example>2</example>
        public int Cantidad { get; set; }
    }
}