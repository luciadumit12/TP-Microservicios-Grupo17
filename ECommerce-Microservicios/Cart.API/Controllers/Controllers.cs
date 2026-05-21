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
        /// <remarks>
        /// ### Ejemplo de Éxito (200 OK):
        /// 
        ///     {
        ///         "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///         "items": [
        ///             { "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "cantidad": 1 }
        ///         ],
        ///         "fechaActualizacion": "2026-05-21T12:00:00Z"
        ///     }
        /// 
        /// ### Ejemplo de Error (Carrito no encontrado - 404):
        /// 
        ///     {
        ///         "errorCode": "CRT-001",
        ///         "errorMessage": "Carrito no encontrado.",
        ///         "status": 404
        ///     }
        /// </remarks>
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
        /// ### Ejemplo de Éxito (200 OK):
        /// 
        ///     {
        ///         "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///         "items": [
        ///             { "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "cantidad": 2 }
        ///         ],
        ///         "fechaActualizacion": "2026-05-21T12:00:00Z"
        ///     }
        /// 
        /// ### Ejemplo de Error (Stock Insuficiente - 422):
        /// 
        ///     {
        ///         "errorCode": "CRT-003",
        ///         "errorMessage": "Stock insuficiente. Disponible: 5, solicitado total en carrito: 10",
        ///         "status": 422
        ///     }
        /// 
        /// ### Ejemplo de Error (Producto no encontrado - 404):
        /// 
        ///     {
        ///         "errorCode": "CRT-002",
        ///         "errorMessage": "Producto no encontrado.",
        ///         "status": 404
        ///     }
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
        /// Actualizar la cantidad de un producto en el carrito.
        /// </summary>
        /// <remarks>
        /// ### Ejemplo de Éxito (200 OK):
        /// 
        ///     {
        ///         "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///         "items": [
        ///             { "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "cantidad": 5 }
        ///         ],
        ///         "fechaActualizacion": "2026-05-21T12:10:00Z"
        ///     }
        /// 
        /// ### Ejemplo de Error (Cantidad Inválida - 400):
        /// 
        ///     {
        ///         "errorCode": "CRT-004",
        ///         "errorMessage": "Cantidad inválida.",
        ///         "status": 400
        ///     }
        /// </remarks>
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
        /// Eliminar un producto específico del carrito.
        /// </summary>
        /// <remarks>
        /// ### Ejemplo de Éxito (204 No Content):
        /// (Respuesta vacía - El producto fue eliminado correctamente)
        /// 
        /// ### Ejemplo de Error (Producto no encontrado en carrito - 404):
        /// 
        ///     {
        ///         "errorCode": "CRT-002",
        ///         "errorMessage": "Producto no encontrado en el carrito.",
        ///         "status": 404
        ///     }
        /// 
        /// ### Ejemplo de Error (Usuario no encontrado - 404):
        /// 
        ///     {
        ///         "errorCode": "CRT-006",
        ///         "errorMessage": "El usuario especificado no existe.",
        ///         "status": 404
        ///     }
        /// </remarks>
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
        /// <remarks>
        /// ### Ejemplo de Éxito (204 No Content):
        /// (Respuesta vacía - El carrito ha sido vaciado)
        /// 
        /// ### Ejemplo de Error (Carrito no encontrado - 404):
        /// 
        ///     {
        ///         "errorCode": "CRT-001",
        ///         "errorMessage": "Carrito no encontrado.",
        ///         "status": 404
        ///     }
        /// 
        /// ### Ejemplo de Error (Usuario no encontrado - 404):
        /// 
        ///     {
        ///         "errorCode": "CRT-006",
        ///         "errorMessage": "El usuario especificado no existe.",
        ///         "status": 404
        ///     }
        /// </remarks>
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