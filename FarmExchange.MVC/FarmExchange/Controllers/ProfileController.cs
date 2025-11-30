using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmExchange.Data;
using FarmExchange.Models;
using FarmExchange.ViewModels; // Added

namespace FarmExchange.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly FarmExchangeDbContext _context;

        public ProfileController(FarmExchangeDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var profile = await _context.Profiles
                .Include(p => p.Harvests)
                .Include(p => p.Addresses) // Include addresses for display
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profile == null)
            {
                return NotFound();
            }

            // Fetch active harvests (available)
            ViewBag.ActiveHarvests = await _context.Harvests
                .Where(h => h.UserId == id && h.Status == "available")
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var profile = await _context.Profiles
                .Include(p => p.Addresses)
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (profile == null) return NotFound();

            var currentAddress = profile.Addresses.FirstOrDefault();

            var viewModel = new EditProfileViewModel
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                MiddleName = profile.MiddleName,
                ExtensionName = profile.ExtensionName,
                Phone = profile.Phone,

                // Populate address fields for display (even if we don't bind them back directly without "UpdateAddress" flag)
                UnitNumber = currentAddress?.UnitNumber,
                StreetName = currentAddress?.StreetName,
                Barangay = currentAddress?.Barangay,
                City = currentAddress?.City,
                Province = currentAddress?.Province,
                Region = currentAddress?.Region,
                PostalCode = currentAddress?.PostalCode
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
<<<<<<< HEAD
            
            // Verify the user is editing their own profile
            if (model.Id != userId)
            {
                return Forbid();
            }

            var profile = await _context.Profiles.FindAsync(userId);
=======
            var profile = await _context.Profiles
                .Include(p => p.Addresses)
                .FirstOrDefaultAsync(p => p.Id == userId);
>>>>>>> d08f6f5e1972d5ba31ba4ecede33ea79e762e894

            if (profile == null) return NotFound();

            if (ModelState.IsValid)
            {
                // 1. Update Profile Info
                profile.FirstName = model.FirstName;
                profile.LastName = model.LastName;
                profile.MiddleName = model.MiddleName;
                profile.ExtensionName = model.ExtensionName;
                profile.Phone = model.Phone;
                // Bio Removed as requested

<<<<<<< HEAD
=======
                // 2. Update Address if requested
                if (model.UpdateAddress)
                {
                    // Clear existing addresses (Simplification: User has 1 address)
                    // Or find the first one
                    var address = profile.Addresses.FirstOrDefault();
                    if (address == null)
                    {
                        address = new UserAddress { UserID = userId };
                        _context.UserAddresses.Add(address);
                    }

                    address.UnitNumber = model.UnitNumber;
                    address.StreetName = model.StreetName;
                    address.Barangay = model.Barangay;
                    address.City = model.City;
                    address.Province = model.Province;
                    address.Region = model.Region;
                    address.PostalCode = model.PostalCode;

                    // Update Legacy Location String
                    profile.Location = string.IsNullOrEmpty(model.Province)
                        ? $"{model.Barangay}, {model.City}, {model.Region}"
                        : $"{model.Barangay}, {model.City}, {model.Province}";
                }

>>>>>>> d08f6f5e1972d5ba31ba4ecede33ea79e762e894
                profile.UpdatedAt = DateTime.UtcNow;

                try
                {
                    _context.Profiles.Update(profile);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction("Details", new { id = userId });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while saving your profile. Please try again.";
                    ModelState.AddModelError("", "An error occurred while saving your profile. Please try again.");
                }
            }

            return View(model);
        }
    }
}