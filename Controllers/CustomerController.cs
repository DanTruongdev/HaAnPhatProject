using GlassECommerce.Data;
using GlassECommerce.DTOs;
using GlassECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlassECommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "CUSTOMER")]
    public class CustomerController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuthenticationService _authService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IWishlistService _wishlistService;
        private readonly ICartService _cartService;

        public CustomerController(IProductService productService, IUserService userService,
            ApplicationDbContext dbContext, IAuthenticationService authService,
            ICustomerService customerService, ICloudinaryService cloudinaryService, IWishlistService wishlistService, ICartService cartService)
        {
            _productService = productService;
            _userService = userService;
            _dbContext = dbContext;
            _authService = authService;
            _customerService = customerService;
            _cloudinaryService = cloudinaryService;
            _wishlistService = wishlistService;
            _cartService = cartService;
        }
        [AllowAnonymous]
        [HttpGet("upload")]
        public async Task<IActionResult> Upload()
        {
            var res = _cloudinaryService.GetPresignedUrl();
            if (res == null) return StatusCode(StatusCodes.Status502BadGateway, new Response("Error", "Cannot generate presign "));
            return Ok(res);
        }
        [HttpGet("my")]
        public async Task<IActionResult> GetCustomerInfor()
        {
            try
            {
                var currentUser = await _authService.GetCurrentLoggedInUser();
                if (currentUser == null) return Unauthorized();
                var res = await _userService.GetUserById(currentUser.Id);
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }
        }

        [HttpPut("my/edit")]
        public async Task<IActionResult> EditCustomerInfor([FromBody] UserDTO model)
        {

            try
            {
                var currentUser = await _authService.GetCurrentLoggedInUser();
                if (currentUser == null) return Unauthorized();
                var res = await _userService.EditUser(currentUser.Id, model);
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }
        }

        //cart
        [HttpPost("carts/add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] CartDTO model)
        {
            try
            {
                var res = await _cartService.AddToCart(model);
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }
        }

        [HttpGet("carts")]
        public async Task<IActionResult> GetAllCart()
        {
            try
            {
                var res = await _cartService.GetAllCarts();
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }
        }

        [HttpDelete("carts/remove/{cartId}")]
        public async Task<IActionResult> RemoveCartItem([FromRoute] int cartId)
        {
            try
            {
                var res = await _cartService.RemoveCartItem(cartId);
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }
        }

        [HttpPut("carts/edit/{cartId}")]
        public async Task<IActionResult> EditCart([FromRoute] int cartId, [FromBody] CartDTO model)
        {

            try
            {
                var res = await _cartService.EditCart(cartId, model);
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }
        }

        //orders
        [HttpPost("orders/add")]
        public async Task<IActionResult> AddOrder([FromBody] OrderDTO model)
        {
            try
            {
                var res = await _customerService.AddOrder(model);
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }

        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders(string? status)
        {
            try
            {
                var res = await _customerService.GetAllOrders(status);
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }

        }

        [HttpPut("orders/cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder([FromRoute] int orderId)
        {
            try
            {
                var res = await _customerService.CancelOrder(orderId);
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }

        }

        //wishlist
        [HttpPost("wishlist/add")]
        public async Task<IActionResult> AddWishlistItem([FromBody] WishlistDTO model)
        {
            try
            {
                var res = await _wishlistService.AddWishlistItem(model);
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }

        }

        [HttpGet("wishlist")]
        public async Task<IActionResult> GetAllUserWishlist()
        {
            try
            {
                var res = await _wishlistService.GetAllWishlistItems();
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }
        }

        [HttpDelete("wishlist/remove/{wishlistItemId}")]
        public async Task<IActionResult> RemoveWishlistItem([FromRoute] int wishlistItemId)
        {
            try
            {
                var res = await _wishlistService.RemoveWishlistItems(wishlistItemId);
                return res;
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "An error occur when handle request"));
            }
        }




    }
}
