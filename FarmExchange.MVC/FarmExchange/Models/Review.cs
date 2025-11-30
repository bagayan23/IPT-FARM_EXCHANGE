using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmExchange.Models
{
    public class Review
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid BuyerId { get; set; }

        [Required]
        public Guid SellerId { get; set; }

        public Guid? TransactionId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("BuyerId")]
        public virtual Profile Buyer { get; set; } = null!;

        [ForeignKey("SellerId")]
        public virtual Profile Seller { get; set; } = null!;

        [ForeignKey("TransactionId")]
        public virtual Transaction? Transaction { get; set; }
    }
}
