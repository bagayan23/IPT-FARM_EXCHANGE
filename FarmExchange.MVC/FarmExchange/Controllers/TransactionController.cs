using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmExchange.Data;
using FarmExchange.Models;
using System.Security.Claims;

namespace FarmExchange.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly FarmExchangeDbContext _context;

        public TransactionController(FarmExchangeDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? filter)
        {
            var userId = GetCurrentUserId();
            var profile = await _context.Profiles.FindAsync(userId);

            if (profile == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.Transactions
                .Include(t => t.Harvest)
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .AsQueryable();

            if (profile.UserType == UserType.Farmer)
            {
                query = query.Where(t => t.SellerId == userId);
            }
            else
            {
                query = query.Where(t => t.BuyerId == userId);
            }

            if (!string.IsNullOrEmpty(filter) && filter != "all")
            {
                query = query.Where(t => t.Status == filter);
            }

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            ViewBag.Profile = profile;
            ViewBag.Filter = filter ?? "all";


            var stats = new
            {
                Total = transactions.Count,
                Pending = transactions.Count(t => t.Status == "pending"),
                Completed = transactions.Count(t => t.Status == "completed"),
                TotalRevenue = transactions.Where(t => t.Status == "completed").Sum(t => t.TotalPrice)
            };

            ViewBag.Stats = stats;

            return View(transactions);
        }

        // --- UPDATED METHOD STARTS HERE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, string status)
        {
            var userId = GetCurrentUserId();

            // 1. Include Harvest data so we can modify stock
            var transaction = await _context.Transactions
                .Include(t => t.Harvest)
                .FirstOrDefaultAsync(t => t.Id == id);

            // Security Check
            if (transaction == null || transaction.SellerId != userId)
            {
                TempData["Error"] = "Transaction not found or unauthorized.";
                return RedirectToAction("Index");
            }

            // Only allow changes if currently pending
            if (transaction.Status == "pending")
            {
                // 2. If Cancelling, RESTORE STOCK
                if (status == "cancelled")
                {
                    transaction.Harvest.QuantityAvailable += transaction.Quantity;

                    // Ensure harvest is available again if it has stock
                    if (transaction.Harvest.QuantityAvailable > 0)
                    {
                        transaction.Harvest.Status = "available";
                    }

                    // Force EF Core to update the Harvest table
                    _context.Entry(transaction.Harvest).State = EntityState.Modified;

                    TempData["Success"] = "Order cancelled. Stock returned to inventory and harvest is now available for browsing.";
                }
                else if (status == "completed")
                {
                    // When completing, keep stock deducted
                    TempData["Success"] = "Transaction completed successfully!";
                }
                else
                {
                    TempData["Success"] = "Transaction status updated successfully!";
                }

                // 3. Update the Transaction Status
                transaction.Status = status;

                await _context.SaveChangesAsync();
            }
            else
            {
                TempData["Error"] = "Cannot change status of a completed or cancelled order.";
            }

            return RedirectToAction("Index");
        }
        // --- UPDATED METHOD ENDS HERE ---

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }
    }
}