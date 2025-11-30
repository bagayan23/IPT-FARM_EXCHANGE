using System.ComponentModel.DataAnnotations;
using FarmExchange.Models;

namespace FarmExchange.ViewModels
{
    public class SignUpViewModel
    {
        // --- REPLACED FULL NAME WITH THESE 4 FIELDS ---

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; } // Added '?' to make it optional

        [StringLength(10)]
        [Display(Name = "Extension Name")]
        public string? ExtensionName { get; set; } // Added '?' to make it optional

        // ----------------------------------------------

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Account Type")]
        public UserType UserType { get; set; }

        // --- ADDRESS FIELDS ---

        [StringLength(50)]
        [Display(Name = "Unit/House No.")]
        public string? UnitNumber { get; set; }

        [Required(ErrorMessage = "Street Name is required")]
        [StringLength(100)]
        [Display(Name = "Street Name")]
        public string? StreetName { get; set; }

        [Required(ErrorMessage = "Region is required")]
        [StringLength(100)]
        public string? Region { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        [Required(ErrorMessage = "City/Municipality is required")]
        [StringLength(100)]
        [Display(Name = "City/Municipality")]
        public string? City { get; set; }

        [Required(ErrorMessage = "Barangay is required")]
        [StringLength(100)]
        public string? Barangay { get; set; }

        [Required(ErrorMessage = "Postal Code is required")]
        [StringLength(10)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        // ----------------------

        [StringLength(255)]
        public string? Location { get; set; } // Kept for legacy compatibility if needed, but not used in UI

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }
    }
}