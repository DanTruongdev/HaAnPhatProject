using GlassECommerce.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GlassECommerce.Services.Interfaces
{
    public interface ICustomerService
    {
        public Task<IActionResult> AddToCart(CartDTO model);
        // public Task<IActionResult> GetAllCarts();
        public Task<IActionResult> RemoveCartItem(int cartId);
        public Task<IActionResult> EditCart(int cartId, CartDTO model);

        public Task<IActionResult> AddOrder(OrderDTO model);
        public Task<IActionResult> GetAllOrders(string status);
        public Task<IActionResult> CancelOrder(int orderId);
    }
}
