namespace Cart.API.DTOs
{
    /// <summary>
    /// Carrito de compras de un usuario
    /// </summary>
    public class CartResponse
    {
        /// <summary>ID del usuario dueño del carrito</summary>
        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        public Guid UsuarioId { get; set; }

        /// <summary>Lista de productos en el carrito</summary>
        public List<CartItemResponse> Items { get; set; } = new();

        /// <summary>Fecha de última modificación del carrito</summary>
        /// <example>2024-03-10T10:50:00Z</example>
        public DateTime FechaActualizacion { get; set; }
    }
}