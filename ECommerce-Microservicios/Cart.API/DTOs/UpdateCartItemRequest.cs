namespace Cart.API.DTOs
{
    /// <summary>
    /// Datos para actualizar la cantidad de un producto en el carrito
    /// </summary>
    public class UpdateCartItemRequest
    {
        /// <summary>Nueva cantidad de unidades</summary>
        /// <example>4</example>
        public int Cantidad { get; set; }
    }
}