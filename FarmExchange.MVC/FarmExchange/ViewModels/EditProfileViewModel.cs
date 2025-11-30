using System.ComponentModel.DataAnnotations;

namespace FarmExchange.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [StringLength(10)]
        [Display(Name = "Extension Name")]
        public string? ExtensionName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        // --- ADDRESS FIELDS ---
        public bool UpdateAddress { get; set; } // Checkbox to enable address update

        [StringLength(50)]
        [Display(Name = "Unit/House No.")]
        public string? UnitNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Street Name")]
        public string? StreetName { get; set; }

        [StringLength(100)]
        public string? Region { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        [StringLength(100)]
        [Display(Name = "City/Municipality")]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Barangay { get; set; }

        [StringLength(10)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }
    }
}