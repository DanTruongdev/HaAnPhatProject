using System.ComponentModel.DataAnnotations;

namespace GlassECommerce.Models
{
    public class FeedbackAttachment
    {
        [Key]
        public int FeedbackAttachmentId { get; set; }
        [Required]
        public int FeedbackId { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        public string Type { get; set; }
        //
        public virtual Feedback Feedback { get; set; }
    }
}
