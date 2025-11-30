using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmExchange.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SenderId { get; set; }

        [Required]
        public Guid RecipientId { get; set; }

        public Guid? HarvestId { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public bool IsEdited { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("SenderId")]
        public virtual Profile Sender { get; set; } = null!;

        [ForeignKey("RecipientId")]
        public virtual Profile Recipient { get; set; } = null!;

        [ForeignKey("HarvestId")]
        public virtual Harvest? Harvest { get; set; }
    }
}