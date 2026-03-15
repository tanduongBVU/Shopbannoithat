using System.ComponentModel.DataAnnotations;

namespace Shopbannoithat.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string SenderEmail { get; set; }

        [Required]
        public string SenderRole { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public string CustomerEmail { get; set; }
    }
}