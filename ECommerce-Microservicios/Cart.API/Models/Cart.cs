namespace Cart.API.Models
{
    /// <summary>
    /// Modelo que representa un carrito de compras.
    /// </summary>
    public class Cart
    {
        /// <summary>
        /// ID del usuario dueño del carrito.
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Lista de productos del carrito.
        /// </summary>
        public List<CartItem> Items { get; set; } = new();

        /// <summary>
        /// Fecha de última actualización del carrito.
        /// </summary>
        public DateTime FechaActualizacion { get; set; }
    }
}