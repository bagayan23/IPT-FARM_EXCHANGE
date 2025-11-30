using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmExchange.Models
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid HarvestId { get; set; }

        [Required]
        public Guid BuyerId { get; set; }

        [Required]
        public Guid SellerId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "pending";

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string? Notes { get; set; }

        [ForeignKey("HarvestId")]
        public virtual Harvest Harvest { get; set; } = null!;

        [ForeignKey("BuyerId")]
        public virtual Profile Buyer { get; set; } = null!;

        [ForeignKey("SellerId")]
        public virtual Profile Seller { get; set; } = null!;
    }
}