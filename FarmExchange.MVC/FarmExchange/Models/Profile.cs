using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for [NotMapped]

namespace FarmExchange.Models
{
    public class Profile
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? MiddleName { get; set; }

        [StringLength(10)]
        public string? ExtensionName { get; set; }

        // --- NEW COMPUTED PROPERTY ---
        [NotMapped] // This tells EF Core: "Do not look for this column in SQL"
        public string FullName
        {
            get
            {
                // Format: LastName, FirstName MiddleName ExtensionName
                // Example: Doe, John A. Jr.

                var fullName = $"{LastName}, {FirstName}";

                if (!string.IsNullOrEmpty(MiddleName))
                {
                    fullName += $" {MiddleName}";
                }

                if (!string.IsNullOrEmpty(ExtensionName))
                {
                    fullName += $" {ExtensionName}";
                }

                return fullName;
            }
        }
        // -----------------------------

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserType UserType { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public virtual ICollection<Transaction> BuyerTransactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Transaction> SellerTransactions { get; set; } = new List<Transaction>();

        public virtual ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    }
}