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

            // Fetch reviews where this user is the seller
            var reviews = await _context.Reviews
                .Include(r => r.Buyer)
                .Where(r => r.SellerId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Reviews = reviews;

            // Calculate stats
            double averageRating = 0;
            if (reviews.Any())
            {
                averageRating = reviews.Average(r => r.Rating);
            }
            ViewBag.AverageRating = averageRating;
            ViewBag.ReviewCount = reviews.Count;

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
            
            // Verify the user is editing their own profile
            if (model.Id != userId)
            {
                return Forbid();
            }

            var profile = await _context.Profiles.FindAsync(userId);

            if (profile == null) return NotFound();

            if (ModelState.IsValid)
            {
                profile.FirstName = model.FirstName;
                profile.LastName = model.LastName;
                profile.MiddleName = model.MiddleName;
                profile.ExtensionName = model.ExtensionName;
                profile.Phone = model.Phone;
                profile.Bio = model.Bio;

                profile.UpdatedAt = DateTime.UtcNow;

                try
                {
                    _context.Profiles.Update(profile);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = userId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving your profile. Please try again.");
                }
            }

            return View(profile);
        }
    }
}