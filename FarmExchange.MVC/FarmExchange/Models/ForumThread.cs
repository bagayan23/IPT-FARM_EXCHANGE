using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmExchange.Models
{
    public class ForumThread
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        [StringLength(50)]
        public string Category { get; set; } = "General"; // e.g., "Sustainable Farming", "Events", "General"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AuthorId")]
        public virtual Profile Author { get; set; } = null!;

        public virtual ICollection<ForumPost> Posts { get; set; } = new List<ForumPost>();
    }
}
