using Microsoft.AspNetCore.Mvc;
using Cart.API.DTOs;
using Cart.API.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Cart.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Tags("Cart")]
    public class CartController : ControllerBase
    {
        private readonly CartService _service;

        public CartController(CartService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtener el carrito de un usuario.
        /// </summary>
        /// <param name="userId">ID único del usuario dueño del carrito.</param>
        /// <response code="200">Carrito obtenido con éxito.</response>
        /// <response code="404">El carrito solicitado no fue encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCart(Guid userId)
        {
            var cart = await _service.GetByUserIdAsync(userId);
            return Ok(cart);
        }

        /// <summary>
        /// Agregar un producto al carrito.
        /// </summary>
        /// <remarks>
        /// Ejemplo de petición (Request Body):
        /// 
        ///     POST /api/cart/a1b2c3d4-0000-0000-0000-111122223333/items
        ///     {
        ///        "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///        "quantity": 2
        ///     }
        /// 
        /// </remarks>
        /// <param name="userId">ID único del usuario.</param>
        /// <param name="request">Datos del ítem y cantidad a agregar.</param>
        /// <response code="200">Producto agregado con éxito.</response>
        /// <response code="404">Producto o usuario no encontrado.</response>
        /// <response code="422">Stock insuficiente para agregar al carrito.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("{userId}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddItem(Guid userId, [FromBody] AddCartItemRequest request)
        {
            var cart = await _service.AddItemAsync(userId, request);
            return Ok(cart);
        }

        /// <summary>
        /// Actualizar la cantidad de un producto en el carrito.
        /// </summary>
        /// <remarks>
        /// Ejemplo de petición (Request Body):
        /// 
        ///     PUT /api/cart/a1b2c3d4-0000-0000-0000-111122223333/items/3fa85f64-5717-4562-b3fc-2c963f66afa6
        ///     {
        ///        "quantity": 5
        ///     }
        /// 
        /// </remarks>
        /// <param name="userId">ID único del usuario.</param>
        /// <param name="productId">ID único del producto a modificar.</param>
        /// <param name="request">Nueva cantidad solicitada.</param>
        /// <response code="200">Cantidad modificada con éxito.</response>
        /// <response code="400">Cantidad inválida provista.</response>
        /// <response code="404">Carrito o producto no encontrado.</response>
        /// <response code="422">No hay stock disponible para la cantidad solicitada.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPut("{userId}/items/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateItem(Guid userId, Guid productId, [FromBody] UpdateCartItemRequest request)
        {
            var cart = await _service.UpdateItemAsync(userId, productId, request);
            return Ok(cart);
        }

        /// <summary>
        /// Eliminar un producto específico del carrito.
        /// </summary>
        /// <param name="userId">ID único del usuario.</param>
        /// <param name="productId">ID único del producto a remover.</param>
        /// <response code="204">Producto removido con éxito (sin contenido).</response>
        /// <response code="404">Carrito, usuario o producto no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpDelete("{userId}/items/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteItem(Guid userId, Guid productId)
        {
            await _service.DeleteItemAsync(userId, productId);
            return NoContent();
        }

        /// <summary>
        /// Vaciar completamente el carrito de un usuario.
        /// </summary>
        /// <param name="userId">ID único del usuario a vaciar.</param>
        /// <response code="204">Carrito vaciado con éxito (sin contenido).</response>
        /// <response code="404">Carrito o usuario no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            await _service.ClearCartAsync(userId);
            return NoContent();
        }
    }
}