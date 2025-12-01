using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmExchange.Models
{
    public class Harvest
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(20)]
        public string Unit { get; set; } = "kg";

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity must be non-negative.")]
        public decimal QuantityAvailable { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "available";

        public DateTime? HarvestDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual Profile User { get; set; } = null!;

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}