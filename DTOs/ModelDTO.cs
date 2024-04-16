using System.ComponentModel.DataAnnotations;

namespace GlassECommerce.DTOs
{
    public class ModelDTO
    {
        [Required]
        public string ModelName { get; set; }
        [Required]
        public int UnitId { get; set; }
        [Required]
        public int ColorId { get; set; }
        public string? Specification { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "The primary price must be greater than 0")]
        public double PrimaryPrice { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "The secondary price must be greater than 0")]
        public double SecondaryPrice { get; set; }
        public string? Description { get; set; }
        public List<AttachmentDto>? Attachments { get; set; }

    }

    public class AttachmentDto
    {
        [Required]
        public string Path { get; set; }
        [Required]
        public string Type { get; set; }
    }
}
