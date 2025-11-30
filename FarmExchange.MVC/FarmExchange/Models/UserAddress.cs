using System.ComponentModel.DataAnnotations;

namespace FarmExchange.Models
{
    public class UserAddress
    {
        [Key]
        public int AddressID { get; set; }

        public Guid UserID { get; set; }

        [StringLength(50)]
        public string? UnitNumber { get; set; }

        [StringLength(100)]
        public string? StreetName { get; set; }

        [StringLength(100)]
        public string? Barangay { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        [StringLength(100)]
        public string? Region { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(50)]
        public string Country { get; set; } = "Philippines";
    }
}