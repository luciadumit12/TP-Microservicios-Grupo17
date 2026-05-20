// El Controller es la puerta de entrada de la API.
// Recibe requests HTTP, llama al Service y devuelve la respuesta.
// No contiene lógica de negocio.

using Microsoft.AspNetCore.Mvc;
using Cart.API.DTOs;
using Cart.API.Services;

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
        /// Ejemplo de request:
        ///
        ///     POST /api/cart/{userId}/items
        ///     {
        ///         "productId": "11111111-1111-1111-1111-111111111111",
        ///         "cantidad": 2
        ///     }
        ///
        /// </remarks>
        [HttpPost("{userId}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddItem(
            Guid userId,
            [FromBody] AddCartItemRequest request)
        {
            var cart = await _service.AddItemAsync(userId, request);
            return Ok(cart);
        }

        /// <summary>
        /// Actualizar la cantidad de un producto del carrito.
        /// </summary>
        [HttpPut("{userId}/items/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateItem(
            Guid userId,
            Guid productId,
            [FromBody] UpdateCartItemRequest request)
        {
            var cart = await _service.UpdateItemAsync(
                userId,
                productId,
                request);

            return Ok(cart);
        }

        /// <summary>
        /// Eliminar un producto del carrito.
        /// </summary>
        [HttpDelete("{userId}/items/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteItem(
            Guid userId,
            Guid productId)
        {
            await _service.DeleteItemAsync(userId, productId);
            return NoContent();
        }

        /// <summary>
        /// Vaciar completamente el carrito de un usuario.
        /// </summary>
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