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
            var profile = await _context.Profiles.FindAsync(userId);

            if (profile == null) return NotFound();

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Profile model)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var profile = await _context.Profiles.FindAsync(userId);

            if (profile == null) return NotFound();

            // Only allow editing specific fields for now
            // Excluding Location to prevent sync issues with UserAddress

            // However, the user asked to edit "profile", let's allow Name, Phone, Bio.
            // Location is complex due to address split, but we can allow editing the SUMMARY string?
            // No, that would be confusing. Let's stick to Profile fields.

            if (ModelState.IsValid)
            {
                profile.FirstName = model.FirstName;
                profile.LastName = model.LastName;
                profile.MiddleName = model.MiddleName;
                profile.ExtensionName = model.ExtensionName;
                profile.Phone = model.Phone;

                // We're not updating Location here because it's derived from Address
                // If the user wants to update address, that's a separate complex flow.
                // But for "Edit Profile", Name/Phone is standard.

                profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = userId });
            }

            return View(profile);
        }
    }
}