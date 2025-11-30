using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmExchange.Data;
using FarmExchange.Models;
using System.Security.Claims;

namespace FarmExchange.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly FarmExchangeDbContext _context;

        public DashboardController(FarmExchangeDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var profile = await _context.Profiles.FindAsync(userId);

            if (profile == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Profile = profile;

            if (profile.UserType == UserType.Farmer)
            {
                ViewBag.Harvests = await _context.Harvests
                    .Where(h => h.UserId == userId)
                    .OrderByDescending(h => h.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                ViewBag.Sales = await _context.Transactions
                    .Include(t => t.Harvest)
                    .Include(t => t.Buyer)
                    .Where(t => t.SellerId == userId)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(5)
                    .ToListAsync();
            }
            else
            {
                ViewBag.Purchases = await _context.Transactions
                    .Include(t => t.Harvest)
                    .Include(t => t.Seller)
                    .Where(t => t.BuyerId == userId)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(5)
                    .ToListAsync();
            }

            ViewBag.UnreadMessages = await _context.Messages
                .CountAsync(m => m.RecipientId == userId && !m.IsRead);

            return View();
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }
    }
}