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

        [StringLength(255)]
        public string? Location { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }
    }
}