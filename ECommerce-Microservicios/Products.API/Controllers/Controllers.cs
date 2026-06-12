// El Controller es la puerta de entrada de la API.
// Recibe requests HTTP, llama al Service y devuelve la respuesta.
// No contiene lógica de negocio.
using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Services;

namespace Products.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Tags("Products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductsController(ProductService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtener listado de productos.
        /// </summary>
        /// <remarks>
        /// Permite filtrar productos por categoría y/o nombre.
        ///
        /// Ejemplo:
        ///
        ///     GET /api/products?categoria=Electrónica
        ///
        /// </remarks>
        /// <response code="200">Listado de productos obtenido con éxito.</response>
        /// <response code="500">Error interno del servidor al procesar el listado.</response>
        /// <example>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        ///   "title": "Internal Server Error",
        ///   "status": 500,
        ///   "detail": "Ocurrió un error inesperado al recuperar el catálogo de productos.",
        ///   "instance": "/api/products",
        ///   "errorCode": "PRD-005",
        ///   "errorMessage": "Error inesperado al procesar el catálogo."
        /// }
        /// </example>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? categoria,
            [FromQuery] string? nombre)
        {
            var products = await _service.GetAllAsync(categoria, nombre);
            return Ok(products);
        }

        /// <summary>
        /// Obtener un producto por ID.
        /// </summary>
        /// <remarks>
        /// Ejemplo:
        ///
        ///     GET /api/products/{id}
        ///
        /// </remarks>
        /// <response code="200">Producto encontrado con éxito.</response>
        /// <response code="404">El producto solicitado no existe en el sistema.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <example>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///   "title": "Not Found",
        ///   "status": 404,
        ///   "detail": "El recurso solicitado no fue encontrado.",
        ///   "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "errorCode": "PRD-001",
        ///   "errorMessage": "Producto no encontrado."
        /// }
        /// </example>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _service.GetByIdAsync(id);
            return Ok(product);
        }

        /// <summary>
        /// Crear un nuevo producto.
        /// </summary>
        /// <remarks>
        /// Ejemplo de request:
        ///
        ///     POST /api/products
        ///     {
        ///         "nombre": "Notebook Dell XPS 15",
        ///         "descripcion": "Laptop 15 pulgadas, 32GB RAM",
        ///         "precio": 1500,
        ///         "stock": 10,
        ///         "categoria": "Electrónica"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Producto creado exitosamente.</response>
        /// <response code="400">Los datos enviados son inválidos o faltan campos obligatorios.</response>
        /// <response code="409">Ya existe un producto con el mismo nombre en la categoría.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <example>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        ///   "title": "Bad Request",
        ///   "status": 400,
        ///   "detail": "Los datos enviados son inválidos.",
        ///   "instance": "/api/products",
        ///   "errorCode": "PRD-002",
        ///   "errorMessage": "El precio no puede ser negativo."
        /// }
        /// </example>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            var product = await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                product);
        }

        /// <summary>
        /// Actualizar un producto existente.
        /// </summary>
        /// <remarks>
        /// Ejemplo de request:
        ///
        ///     PUT /api/products/{id}
        ///     {
        ///         "nombre": "Notebook Dell XPS 15",
        ///         "descripcion": "Laptop 15 pulgadas, 64GB RAM",
        ///         "precio": 1750,
        ///         "stock": 8,
        ///         "categoria": "Electrónica"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Producto actualizado con éxito.</response>
        /// <response code="400">Los datos de actualización provistos son inválidos.</response>
        /// <response code="404">El producto que se intenta actualizar no existe.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <example>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///   "title": "Not Found",
        ///   "status": 404,
        ///   "detail": "No se encontró el producto que se desea modificar.",
        ///   "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "errorCode": "PRD-001",
        ///   "errorMessage": "Producto no encontrado."
        /// }
        /// </example>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateProductRequest request)
        {
            var product = await _service.UpdateAsync(id, request);
            return Ok(product);
        }

        /// <summary>
        /// Eliminar un producto.
        /// </summary>
        /// <remarks>
        /// El producto no puede eliminarse si tiene órdenes activas.
        /// </remarks>
        /// <response code="204">Producto eliminado de forma lógica / física con éxito.</response>
        /// <response code="404">El producto a eliminar no existe en el catálogo.</response>
        /// <response code="409">El producto posee órdenes activas asociadas y no puede ser removido.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <example>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
        ///   "title": "Conflict",
        ///   "status": 409,
        ///   "detail": "Conflicto con las reglas de negocio del dominio.",
        ///   "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "errorCode": "PRD-004",
        ///   "errorMessage": "El producto tiene órdenes activas y no puede eliminarse."
        /// }
        /// </example>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}