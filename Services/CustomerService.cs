using Castle.Core.Internal;
using GlassECommerce.Data;
using GlassECommerce.DTOs;
using GlassECommerce.Models;
using GlassECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlassECommerce.Services
{
    public class CustomerService : ControllerBase, ICustomerService
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly IAuthenticationService _authService;

        public CustomerService(ApplicationDbContext dbContext, IAuthenticationService authService)
        {
            _dbContext = dbContext;
            _authService = authService;
        }

        //cart
        public async Task<IActionResult> AddToCart(CartDTO model)
        {
            var userExist = await _authService.GetCurrentLoggedInUser();

            if (userExist == null) return Unauthorized();
            var modelExist = await _dbContext.Models.FindAsync(model.ModelId);
            if (modelExist == null) return NotFound(new Response("Error", $"The model with id = {model.ModelId} was not found"));
            var cartItemExist = userExist.CartItems.FirstOrDefault(c => c.ModelId == modelExist.ModelId);
            try
            {
                if (cartItemExist == null)
                {
                    CartItem newCartItem = new CartItem()
                    {
                        UserId = userExist.Id,
                        ModelId = model.ModelId,
                        Quantity = model.Quantity,
                    };
                    await _dbContext.AddAsync(newCartItem);
                }
                else
                {
                    cartItemExist.Quantity += model.Quantity;
                    _dbContext.Update(cartItemExist);
                }
                await _dbContext.SaveChangesAsync();
                return Ok(new Response("Success", "Add to cart successfully"));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
              new Response("Error", "An error occurs when adding model to cart"));
            }
        }

        public async Task<IActionResult> EditCart(int cartId, CartDTO model)
        {
            var userExist = await _authService.GetCurrentLoggedInUser();
            if (userExist == null) return Unauthorized();
            var cartItemExist = userExist.CartItems.FirstOrDefault(c => c.CartItemId == cartId);
            if (cartItemExist == null) return NotFound(new Response("Error", $"The cart item with id = {cartId} was not found"));
            var modelExist = await _dbContext.Models.FindAsync(model.ModelId);
            if (modelExist == null) return NotFound(new Response("Error", $"The model with id = {model.ModelId} was not found"));

            try
            {

                cartItemExist.Quantity = model.Quantity;
                _dbContext.Update(cartItemExist);
                await _dbContext.SaveChangesAsync();
                return Ok(new Response("Success", "update cart item successfully"));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
              new Response("Error", "An error occurs when updating cart item"));
            }
        }
        //public async Task<IActionResult> GetAllCarts()
        //{
        //    var userExist = await _authService.GetCurrentLoggedInUser();
        //    if (userExist == null) return Unauthorized();
        //    var carts = await _dbContext.CartItems.Skip(5*(page-1)).Take(5).AsNoTracking().ToListAsync();
        //    if (carts.IsNullOrEmpty()) return Ok(new List<CartItem>());
        //    return Ok(carts.Select(c => new
        //    {
        //        cartItemId = c.CartItemId,
        //        UserId = c.UserId,
        //        ModelId = c.ModelId,
        //        Quantity = c.Quantity
        //    }));
        //}
        public async Task<IActionResult> RemoveCartItem(int cartId)
        {
            var userExist = await _authService.GetCurrentLoggedInUser();
            if (userExist == null) return Unauthorized();
            CartItem deleteCartItem = await _dbContext.CartItems.FindAsync(cartId);
            if (deleteCartItem == null) return NotFound(new Response("Error", $"The cart with id = {cartId} was not found"));
            try
            {
                _dbContext.Remove(deleteCartItem);
                await _dbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status204NoContent,
                    new Response("Success", $"remove cart item with id = {cartId} successfully"));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response("Error", "An error occurs when removing cart item"));
            }
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
                await AddNotification(userExist.Id, "Đặt hàng thành công", "Yêu cầu đặt hàng của bạn đã được gửi đến hệ thống. Vui lòng kiểm tra danh sách đơn hàng để xem trạng thái của đơn hàng của bạn.");
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
                await AddNotification(userExist.Id, "Hủy đơn hàng thành công", "Đơn hàng của bạn đã được hủy thành công bởi hệ thống");
                return Ok(new Response("Success", $"Cancel order with id = {orderId} successfully"));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                  new Response("Error", "An error occurs when canceling order"));
            }
        }


        public async Task<IActionResult> GetAllNotifications()
        {
            User userExist = await _authService.GetCurrentLoggedInUser();
            if (userExist == null) return Unauthorized();
            var notifications = userExist.Notifications?.OrderByDescending(n => n.CreationDate).ToList();
            if (!notifications.Any()) return Ok(new List<Notification>());
            return Ok(notifications.Select(n => new
            {
                NotificationId = n.NotificationId,
                User = userExist.ToString(),
                Title = n.Title,
                Content = n.Content,
                CreationDate = n.CreationDate
            }));

        }

        public async Task<bool> AddNotification(string userId, string title, string content)
        {
            try
            {
                bool userExist = await _dbContext.Users.AnyAsync(u => u.Id.Equals(userId));
                if (!userExist) return false;
                Notification newNotification = new Notification()
                {
                    UserId = userId,
                    Title = title,
                    Content = content,
                    CreationDate = DateTime.Now
                };
                await _dbContext.AddAsync(newNotification);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IActionResult> GetFeedbackByProductId(int productId)
        {
            var feedback = await _dbContext.Feedbacks.Where(f => f.ProductId == productId).AsNoTracking().OrderByDescending(f => f.CreationDate).ToListAsync();
            if (!feedback.Any()) return Ok(new List<Feedback>());
            return Ok(feedback.Select(f => new
            {
                FeedbackId = f.FeedbackId,
                User = f.IsAnonymous ? f.User.ToString() : "Ẩn danh",
                Title = f.Title,
                Content = f.Content,
                Star = f.Star,
                CreationDate = f.CreationDate,
                Attachments = f.FeedbackAttachments?.Select(fa => new
                {
                    path = fa.Path,
                    type = fa.Type,
                })
            }));
        }

        public async Task<IActionResult> AddFeedback(FeedbackDTO model)
        {
            User userExist = await _authService.GetCurrentLoggedInUser();
            if (userExist == null) return Unauthorized();
            Order userOrder = userExist?.Orders.FirstOrDefault(o => o.OrderId == model.OrderId);
            if (userOrder == null) return BadRequest(new Response("Error", $"There is no order with id = {model.OrderId} in user's order list"));
            if (!userOrder.OrderStatus.Equals("Delivered")) return BadRequest(new Response("Error", "The order status must be \"Delivered\" to give feedback"));
            if (userOrder.DeliveredDate?.AddMonths(1) < DateTime.Now) return BadRequest(new Response("Error", $"Feedback can only be added after 1 month from the date of delivery"));
            var orderItemExist = userOrder.OrderItems.FirstOrDefault(oi => oi.Model.Product.ProductId == model.ProductId);
            if (orderItemExist == null) return BadRequest(new Response("Error", $"There is no product with id = {model.ProductId} in user's order list"));
            Product productExist = orderItemExist.Model.Product;
            try
            {
                Feedback newFeedback = new Feedback()
                {
                    UserId = userExist.Id,
                    OrderId = model.OrderId,
                    ProductId = model.ProductId,
                    Title = model.Title,
                    Content = model.Content,
                    Star = model.Star,
                    IsAnonymous = model.IsAnonymous,
                    AllowEdit = true,
                    CreationDate = DateTime.Now
                };
                await _dbContext.AddAsync(newFeedback);
                await _dbContext.SaveChangesAsync();
                if (model.Attachments.Any())
                {
                    foreach (var attachment in model.Attachments)
                    {
                        FeedbackAttachment newAttachment = new FeedbackAttachment()
                        {
                            FeedbackAttachmentId = newFeedback.FeedbackId,
                            Path = attachment.Path,
                            Type = attachment.Type
                        };
                        await _dbContext.AddAsync(newAttachment);
                    }
                    await _dbContext.SaveChangesAsync();
                }
                var relatedFeedbacks = await _dbContext.Feedbacks.Where(f => f.ProductId == model.ProductId).ToListAsync();
                if (relatedFeedbacks.IsNullOrEmpty()) productExist.VoteStar = model.Star;
                else
                {
                    var totalStart = relatedFeedbacks.Sum(f => f.Star) + model.Star;
                    var totalVote = relatedFeedbacks.Count() + 1;
                    productExist.VoteStar = Math.Round((double)totalStart / totalVote * 100, 2);
                }
                _dbContext.Update(productExist);
                await _dbContext.SaveChangesAsync();
                return Created("new feedback created", new
                {
                    User = newFeedback.IsAnonymous ? newFeedback.User.ToString() : "Ẩn danh",
                    OrderId = newFeedback.OrderId,
                    ProductId = newFeedback.ProductId,
                    Title = newFeedback.Title,
                    Content = newFeedback.Content,
                    Star = newFeedback.Star,
                    CreationDate = newFeedback.CreationDate,
                    Attachments = newFeedback.FeedbackAttachments.Select(fa => new
                    {
                        Path = fa.Path,
                        Type = fa.Type,
                    })
                });

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                 new Response("Error", "An error occurs when adding feedback"));
            }
        }
        public async Task<IActionResult> EditFeedback(int feedbackId, FeedbackDTO model)
        {
            User userExist = await _authService.GetCurrentLoggedInUser();
            if (userExist == null) return Unauthorized();
            if (userExist.Feedbacks.Any()) return BadRequest(new Response("Error", "The user has no any feedback"));
            Feedback userFeedback = userExist.Feedbacks.FirstOrDefault(f => f.FeedbackId == feedbackId);
            if (userFeedback == null) return NotFound(new Response("Error", $"The user has no feedback with {feedbackId}"));
            if (!userFeedback.AllowEdit) return BadRequest(new Response("Error", "The feedback can be edited one time"));
            Product productExist = userFeedback.Product;
            try
            {
                userFeedback.Title = model.Title;
                userFeedback.Content = model.Content;
                userFeedback.Star = model.Star;
                userFeedback.IsAnonymous = model.IsAnonymous;
                userFeedback.AllowEdit = false;

                _dbContext.Update(userFeedback);
                await _dbContext.SaveChangesAsync();
                if (model.Attachments.Any())
                {
                    if (userFeedback.FeedbackAttachments.Any()) userFeedback.FeedbackAttachments.Clear();
                    foreach (var attachment in model.Attachments)
                    {
                        FeedbackAttachment newAttachment = new FeedbackAttachment()
                        {
                            FeedbackAttachmentId = userFeedback.FeedbackId,
                            Path = attachment.Path,
                            Type = attachment.Type
                        };
                        await _dbContext.AddAsync(newAttachment);
                    }
                    await _dbContext.SaveChangesAsync();
                }
                var relatedFeedbacks = await _dbContext.Feedbacks.Where(f => f.ProductId == model.ProductId).ToListAsync();
                if (relatedFeedbacks.IsNullOrEmpty()) productExist.VoteStar = model.Star;
                else
                {
                    var totalStart = relatedFeedbacks.Sum(f => f.Star) + model.Star;
                    var totalVote = relatedFeedbacks.Count() + 1;
                    productExist.VoteStar = Math.Round((double)totalStart / totalVote * 100, 2);
                }
                _dbContext.Update(productExist);
                await _dbContext.SaveChangesAsync();
                return Ok(new
                {
                    User = userFeedback.IsAnonymous ? userFeedback.User.ToString() : "Ẩn danh",
                    OrderId = userFeedback.OrderId,
                    ProductId = userFeedback.ProductId,
                    Title = userFeedback.Title,
                    Content = userFeedback.Content,
                    Star = userFeedback.Star,
                    CreationDate = userFeedback.CreationDate,
                    Attachments = userFeedback.FeedbackAttachments.Select(fa => new
                    {
                        Path = fa.Path,
                        Type = fa.Type,
                    })
                });

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                 new Response("Error", "An error occurs when adding feedback"));
            }
        }


    }
}
