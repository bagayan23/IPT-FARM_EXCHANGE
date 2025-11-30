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

        [HttpGet]
        public async Task<IActionResult> GetSalesAnalytics(string period, DateTime? startDate, DateTime? endDate)
        {
            var userId = GetCurrentUserId();
            var query = _context.Transactions
                .Include(t => t.Harvest)
                .Where(t => t.SellerId == userId);

            DateTime now = DateTime.UtcNow;
            DateTime filterStartDate = DateTime.MinValue;
            DateTime filterEndDate = now;

            // Normalize period to lower case
            period = period?.ToLower() ?? "month";

            switch (period)
            {
                case "hour":
                    filterStartDate = now.AddHours(-1);
                    break;
                case "day":
                    filterStartDate = now.AddDays(-1);
                    break;
                case "week":
                    filterStartDate = now.AddDays(-7);
                    break;
                case "month":
                    filterStartDate = now.AddMonths(-1);
                    break;
                case "year":
                    filterStartDate = now.AddYears(-1);
                    break;
                case "custom":
                    if (startDate.HasValue) filterStartDate = startDate.Value;
                    if (endDate.HasValue) filterEndDate = endDate.Value;
                    break;
                default:
                    filterStartDate = now.AddMonths(-1);
                    break;
            }

            // Ensure endDate includes the whole day if it's just a date (though here it's DateTime)
            // But if coming from input type="date", it might be midnight.
            if (endDate.HasValue && endDate.Value.TimeOfDay == TimeSpan.Zero)
            {
                 filterEndDate = endDate.Value.AddDays(1).AddTicks(-1);
            }

            query = query.Where(t => t.TransactionDate >= filterStartDate && t.TransactionDate <= filterEndDate);

            var data = await query
                .GroupBy(t => t.Harvest.Title)
                .Select(g => new
                {
                    ItemName = g.Key,
                    TotalSales = g.Sum(t => t.TotalPrice),
                    TotalQuantity = g.Sum(t => t.Quantity)
                })
                .ToListAsync();

            return Json(data);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }
    }
}