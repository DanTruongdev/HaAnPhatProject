using System.ComponentModel.DataAnnotations;

namespace GlassECommerce.DTOs
{
    public class OrderDTO
    {
        [Required]
        public List<int> CartItemList { get; set; }
        [Required]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Delivery address  must be greater than 1 and less than 500 characters")]
        public string DeliveryAddress { get; set; }
        public string? Note { get; set; }
    }
}
