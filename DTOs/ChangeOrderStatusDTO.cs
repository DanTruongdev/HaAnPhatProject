using System.ComponentModel.DataAnnotations;

namespace GlassECommerce.DTOs
{
    public class ChangeOrderStatusDTO
    {
        // private readonly List<string> validStatus = new List<string>() {"Pending", "Processing", "Delivering", "Delivered", "Canceled"};
        [Required]
        public int OrderId { get; set; }
        [Required]
        [RegularExpression("^Pending$|^Processing$|^Delivering$^Delivered$|^Canceled$|",
            ErrorMessage = "The order status must be \"Pending\", \"Processing\", \"Delivering\", \"Delivered\" or \"Canceled\"")]
        public string? Status { get; set; }
    }
}
