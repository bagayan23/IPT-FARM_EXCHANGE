using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmExchange.Data;
using FarmExchange.Models;

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

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Profile model,
            string? unitNumber,
            string? streetName,
            string? barangay,
            string? city,
            string? province,
            string? region,
            string? postalCode)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var profile = await _context.Profiles
                .Include(p => p.Addresses)
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (profile == null) return NotFound();

            if (ModelState.IsValid)
            {
                profile.FirstName = model.FirstName;
                profile.LastName = model.LastName;
                profile.MiddleName = model.MiddleName;
                profile.ExtensionName = model.ExtensionName;
                profile.Phone = model.Phone;
                profile.Bio = model.Bio;

                // Update Address
                var address = profile.Addresses.FirstOrDefault();
                if (address == null)
                {
                    address = new UserAddress { UserID = profile.Id };
                    _context.UserAddresses.Add(address);
                }

                // If location fields are provided, update them
                if (!string.IsNullOrEmpty(region) && !string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(barangay))
                {
                    address.UnitNumber = unitNumber;
                    address.StreetName = streetName;
                    address.Barangay = barangay;
                    address.City = city;
                    address.Province = province;
                    address.Region = region;
                    address.PostalCode = postalCode;
                    address.Country = "Philippines"; // Default

                    // Update Profile.Location summary
                    profile.Location = string.IsNullOrEmpty(province)
                        ? $"{barangay}, {city}, {region}"
                        : $"{barangay}, {city}, {province}";
                }

                profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = userId });
            }

            return View(profile);
        }
    }
}