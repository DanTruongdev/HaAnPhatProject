using Castle.Core.Internal;
using GlassECommerce.Data;
using GlassECommerce.DTOs;
using GlassECommerce.Models;
using GlassECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GlassECommerce.Services
{
    public class OrderService : ControllerBase, IOrderService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuthenticationService _authService;
        private readonly INotificationService _notificationService;

        public OrderService(ApplicationDbContext dbContext, IAuthenticationService authService, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _authService = authService;
            _notificationService = notificationService;
        }
        //order
        public async Task<IActionResult> GetAllOrders(string status)
        {
            User currentUser = await _authService.GetCurrentLoggedInUser();
            if (currentUser == null) return Unauthorized();
            var userOrders = status.IsNullOrEmpty() ? currentUser.Orders.OrderByDescending(o => o.OrderDate) :
                             currentUser.Orders.Where(o => o.OrderStatus.Equals(status)).OrderByDescending(o => o.OrderDate);
            if (userOrders.IsNullOrEmpty()) return Ok(new List<Order>());
            string userName = currentUser.ToString();
            return Ok(userOrders.Select(o => new
            {
                OrderId = o.UserId,
                User = userName,
                DeliveryAddress = o.DeliveryAddress,
                OrderDate = o.OrderDate,
                DeliveredDate = o?.DeliveredDate,
                OrderStatus = o.OrderStatus,
                Total = o.TotalCost,
                OrderIems = o.OrderItems.Select(od => new
                {
                    OrderItemId = od.OrderItemId,
                    OrderId = od.OrderId,
                    Model = od.ModelId,
                    Quanty = od.Quantity,
                    Cost = od.Cost
                })
            }));
        }
        public async Task<IActionResult> AddOrder(OrderDTO model)
        {

            User userExist = await _authService.GetCurrentLoggedInUser();
            if (userExist == null) return Unauthorized();
            var userCart = userExist.CartItems;
            if (userCart.IsNullOrEmpty()) return BadRequest(new Response("Error", "There is no item in user cart"));
            var cartItemList = new List<CartItem>();
            foreach (var cartItemId in model.CartItemList)
            {
                var cartItemExist = userCart.FirstOrDefault(c => c.CartItemId == cartItemId);
                if (cartItemExist == null) return NotFound(new Response("Error", $"The cart item with id = {cartItemId} does not exist in user cart"));
                cartItemList.Add(cartItemExist);
            }
            try
            {

                Order newOrder = new Order()
                {
                    UserId = userExist.Id,
                    DeliveryAddress = model.DeliveryAddress,
                    Note = model?.Note,
                    OrderDate = DateTime.Now,
                    OrderStatus = "Pending",
                    TotalCost = 0
                };
                await _dbContext.AddAsync(newOrder);
                await _dbContext.SaveChangesAsync();
                foreach (var cartItem in cartItemList)
                {
                    var modelCart = cartItem.Model;
                    OrderItem newOrderItem = new OrderItem()
                    {
                        OrderId = newOrder.OrderId,
                        ModelId = modelCart.ModelId,
                        Quantity = cartItem.Quantity,
                        Cost = cartItem.Quantity >= 50 ? modelCart.SecondaryPrice : modelCart.PrimaryPrice
                    };
                    newOrder.TotalCost += newOrderItem.Cost;
                    await _dbContext.AddAsync(newOrderItem);

                }
                _dbContext.Update(newOrder);
                await _dbContext.SaveChangesAsync();
                await _notificationService
                    .AddNotification(userExist.Id, "Đặt hàng thành công", "Yêu cầu đặt hàng của bạn đã được gửi đến hệ thống. Vui lòng kiểm tra danh sách đơn hàng để xem trạng thái của đơn hàng của bạn.");
                return Created("new order created", new
                {
                    OrderId = newOrder.OrderId,
                    UserId = newOrder.UserId,
                    DeliveryAddress = newOrder.DeliveryAddress,
                    Note = newOrder?.Note,
                    OrderDate = newOrder.OrderDate,
                    OrderStatus = newOrder.OrderStatus,
                    TotalCost = newOrder.TotalCost,
                    OrderItems = newOrder.OrderItems.Select(od => new
                    {
                        OrderItemId = od.OrderItemId,
                        OrderId = od.OrderId,
                        ModelId = od.ModelId,
                        Quantity = od.Quantity,
                        Cost = od.Cost
                    })
                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                  new Response("Error", "An error occurs when adding order"));
            }
        }

        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                User userExist = await _authService.GetCurrentLoggedInUser();
                if (userExist == null) return Unauthorized();
                var orderExist = userExist.Orders.FirstOrDefault(o => o.OrderId == orderId);
                if (orderExist == null) return NotFound(new Response("Error", $"There is no order with id = {orderId} in user's order list"));
                if (!orderExist.OrderStatus.Equals("Pending") && !orderExist.OrderStatus.Equals("Processing")) return BadRequest(new Response("Error", $"Cannot cancel order in {orderExist.OrderStatus}"));
                orderExist.OrderStatus = "Canceled";
                _dbContext.Update(orderExist);
                await _dbContext.SaveChangesAsync();
                await _notificationService.AddNotification(userExist.Id, "Hủy đơn hàng thành công", "Đơn hàng của bạn đã được hủy thành công bởi hệ thống");
                return Ok(new Response("Success", $"Cancel order with id = {orderId} successfully"));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                  new Response("Error", "An error occurs when canceling order"));
            }
        }

        public async Task<IActionResult> UpdateOrderStatus(ChangeOrderStatusDTO model)
        {
            try
            {
                Order orderExist = await _dbContext.Orders.FindAsync(model.OrderId);
                if (orderExist == null) return NotFound(new Response("Error", $"The order with id = {model.OrderId} was not found"));
                orderExist.OrderStatus = model.Status;
                _dbContext.Update(orderExist);
                _dbContext.SaveChangesAsync();
                return Ok(new
                {
                    OrderId = orderExist.OrderId,
                    UserId = orderExist.UserId,
                    DeliveryAddress = orderExist.DeliveryAddress,
                    Note = orderExist?.Note,
                    OrderDate = orderExist.OrderDate,
                    OrderStatus = orderExist.OrderStatus,
                    TotalCost = orderExist.TotalCost,
                    OrderItems = orderExist.OrderItems.Select(od => new
                    {
                        OrderItemId = od.OrderItemId,
                        OrderId = od.OrderId,
                        ModelId = od.ModelId,
                        Quantity = od.Quantity,
                        Cost = od.Cost
                    })
                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                 new Response("Error", "An error occurs when change order status"));
            }

        }
    }
}
