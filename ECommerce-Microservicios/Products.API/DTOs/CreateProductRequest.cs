namespace Products.API.DTOs
{
    /// <summary>
    /// Request utilizado para crear un nuevo producto.
    /// </summary>
    public class CreateProductRequest
    {
        /// <summary>
        /// Nombre del producto.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del producto.
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Precio unitario del producto.
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// Cantidad disponible en stock.
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// Categoría a la que pertenece el producto.
        /// </summary>
        public string Categoria { get; set; } = string.Empty;
    }
}