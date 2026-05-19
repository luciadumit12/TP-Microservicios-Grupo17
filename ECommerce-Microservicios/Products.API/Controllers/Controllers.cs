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