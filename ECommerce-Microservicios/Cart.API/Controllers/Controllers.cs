using Microsoft.AspNetCore.Mvc;
using Cart.API.DTOs;
using Cart.API.Services;

namespace Cart.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly CartService _service;

        public CartController(CartService service)
        {
            _service = service;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(Guid userId)
        {
            var cart = await _service.GetByUserIdAsync(userId);
            return Ok(cart);
        }

        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AddItem(Guid userId, [FromBody] AddCartItemRequest request)
        {
            var cart = await _service.AddItemAsync(userId, request);
            return Ok(cart);
        }

        [HttpPut("{userId}/items/{productId}")]
        public async Task<IActionResult> UpdateItem(Guid userId, Guid productId, [FromBody] UpdateCartItemRequest request)
        {
            var cart = await _service.UpdateItemAsync(userId, productId, request);
            return Ok(cart);
        }

        [HttpDelete("{userId}/items/{productId}")]
        public async Task<IActionResult> DeleteItem(Guid userId, Guid productId)
        {
            await _service.DeleteItemAsync(userId, productId);
            return NoContent();
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            await _service.ClearCartAsync(userId);
            return NoContent();
        }
    }
}