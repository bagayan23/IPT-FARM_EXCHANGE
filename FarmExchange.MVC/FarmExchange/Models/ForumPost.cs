using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmExchange.Models
{
    public class ForumPost
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ThreadId { get; set; }

        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ThreadId")]
        public virtual ForumThread Thread { get; set; } = null!;

        [ForeignKey("AuthorId")]
        public virtual Profile Author { get; set; } = null!;
    }
}
