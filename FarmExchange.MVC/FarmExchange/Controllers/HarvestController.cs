using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmExchange.Data;
using FarmExchange.Models;
using System.Security.Claims;

namespace FarmExchange.Controllers
{
    [Authorize]
    public class HarvestController : Controller
    {
        private readonly FarmExchangeDbContext _context;

        public HarvestController(FarmExchangeDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. PUBLIC BROWSING
        // ==========================================
        public async Task<IActionResult> Browse(string? search, string? category)
        {
            var query = _context.Harvests
                .Include(h => h.User)
                .Where(h => h.Status == "available" && h.QuantityAvailable > 0);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(h =>
                    h.Title.Contains(search) ||
                    h.Description!.Contains(search) ||
                    h.User.LastName.Contains(search) ||
                    h.User.FirstName.Contains(search) ||
                    h.User.Location!.Contains(search));
            }

            if (!string.IsNullOrEmpty(category) && category != "all")
            {
                query = query.Where(h => h.Category == category);
            }

            var harvests = await query.OrderByDescending(h => h.CreatedAt).ToListAsync();

            var userId = GetCurrentUserId();
            var profile = await _context.Profiles.FindAsync(userId);

            ViewBag.Profile = profile;
            ViewBag.SearchTerm = search;
            ViewBag.CategoryFilter = category;

            return View(harvests);
        }

        // ==========================================
        // 2. FARMER MANAGEMENT DASHBOARD
        // ==========================================
        public async Task<IActionResult> Manage()
        {
            var userId = GetCurrentUserId();
            var profile = await _context.Profiles.FindAsync(userId);

            if (profile?.UserType != UserType.Farmer)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // A. Get My Harvests
            var harvests = await _context.Harvests
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            // B. Get Incoming Orders (Pending)
            var incomingOrders = await _context.Transactions
                .Include(t => t.Harvest)
                .Include(t => t.Buyer)
                .Where(t => t.SellerId == userId && t.Status == "pending")
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            ViewBag.IncomingOrders = incomingOrders;

            return View(harvests);
        }

        // ==========================================
        // 3. CREATE HARVEST (With Image Upload)
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();
            var profile = _context.Profiles.Find(userId);

            if (profile?.UserType != UserType.Farmer) return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Harvest harvest, IFormFile? imageFile)
        {
            var userId = GetCurrentUserId();
            var profile = await _context.Profiles.FindAsync(userId);

            if (profile?.UserType != UserType.Farmer) return RedirectToAction("Index", "Dashboard");

            // Image Upload Logic
            if (imageFile != null && imageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageUrl", "Only image files (jpg, jpeg, png, gif) are allowed.");
                    return View(harvest);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                harvest.ImageUrl = "/uploads/" + uniqueFileName;
            }

            harvest.Id = Guid.NewGuid();
            harvest.UserId = userId;
            harvest.Status = "available";
            harvest.CreatedAt = DateTime.UtcNow;
            harvest.UpdatedAt = DateTime.UtcNow;

            _context.Harvests.Add(harvest);
            await _context.SaveChangesAsync();

            return RedirectToAction("Manage");
        }

        // ==========================================
        // 4. EDIT HARVEST (With Image Update)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var userId = GetCurrentUserId();
            var harvest = await _context.Harvests.FindAsync(id);

            if (harvest == null || harvest.UserId != userId) return RedirectToAction("Manage");

            return View(harvest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Harvest harvest, IFormFile? imageFile)
        {
            if (id != harvest.Id) return NotFound();

            var userId = GetCurrentUserId();
            var existingHarvest = await _context.Harvests.FindAsync(id);

            if (existingHarvest == null || existingHarvest.UserId != userId) return RedirectToAction("Manage");

            // Image Update Logic
            if (imageFile != null && imageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageUrl", "Only image files (jpg, jpeg, png, gif) are allowed.");
                    return View(harvest);
                }

                // Delete old image
                if (!string.IsNullOrEmpty(existingHarvest.ImageUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingHarvest.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                // Save new image
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                existingHarvest.ImageUrl = "/uploads/" + uniqueFileName;
            }

            existingHarvest.Title = harvest.Title;
            existingHarvest.Description = harvest.Description;
            existingHarvest.Category = harvest.Category;
            existingHarvest.Price = harvest.Price;
            existingHarvest.Unit = harvest.Unit;
            existingHarvest.QuantityAvailable = harvest.QuantityAvailable;

            if (existingHarvest.QuantityAvailable > 0 && existingHarvest.Status == "sold_out")
            {
                existingHarvest.Status = "available";
            }
            else if (existingHarvest.QuantityAvailable == 0)
            {
                existingHarvest.Status = "sold_out";
            }

            existingHarvest.HarvestDate = harvest.HarvestDate;
            existingHarvest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction("Manage");
        }

        // ==========================================
        // 5. DELETE HARVEST
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            var harvest = await _context.Harvests.FindAsync(id);

            if (harvest != null && harvest.UserId == userId)
            {
                // 1. Delete the Image File from the server (Keep your existing logic)
                if (!string.IsNullOrEmpty(harvest.ImageUrl))
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", harvest.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                // --- FIX STARTS HERE ---
                // 2. Find all transactions related to this harvest
                var relatedTransactions = _context.Transactions.Where(t => t.HarvestId == id);

                // 3. Delete those transactions first
                _context.Transactions.RemoveRange(relatedTransactions);
                // --- FIX ENDS HERE ---

                // 4. Now it is safe to delete the Harvest
                _context.Harvests.Remove(harvest);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Manage");
        }

        // ==========================================
        // 6. BUYER PURCHASE (Deducts Stock Immediately)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(Guid harvestId, decimal quantity)
        {
            var userId = GetCurrentUserId();

            var harvest = await _context.Harvests
                .FirstOrDefaultAsync(h => h.Id == harvestId);

            if (harvest == null || quantity <= 0 || quantity > harvest.QuantityAvailable)
            {
                TempData["Error"] = "Invalid quantity or out of stock.";
                return RedirectToAction("Browse");
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                HarvestId = harvestId,
                BuyerId = userId,
                SellerId = harvest.UserId,
                Quantity = quantity,
                TotalPrice = quantity * harvest.Price,
                Status = "pending",
                TransactionDate = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            // --- IMMEDIATE DEDUCTION ---
            // Example: 10 - 2 = 8.
            harvest.QuantityAvailable -= quantity;

            if (harvest.QuantityAvailable == 0)
            {
                harvest.Status = "sold_out";
            }

            // Force EF Core to track this change explicitly
            _context.Entry(harvest).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Order placed! Stock reserved waiting for approval.";
            return RedirectToAction("Browse");
        }

        // ==========================================
        // 7. FARMER APPROVE/REJECT (Restocks on Reject)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleTransaction(Guid transactionId, string action)
        {
            var userId = GetCurrentUserId();

            // Load Transaction AND the related Harvest to modify stock
            var transaction = await _context.Transactions
                .Include(t => t.Harvest)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            // Security: Must exist and current user must be the seller
            if (transaction == null || transaction.SellerId != userId)
            {
                TempData["Error"] = "Transaction not found or unauthorized.";
                return RedirectToAction("Manage");
            }

            // Only process pending orders
            if (transaction.Status == "pending")
            {
                if (action == "reject")
                {
                    // --- REJECT LOGIC (RESTOCK) ---
                    transaction.Status = "cancelled";

                    // Example: Stock was 8. Order was 2.
                    // 8 + 2 = 10.
                    transaction.Harvest.QuantityAvailable += transaction.Quantity;

                    // If it was marked sold out, make it available again
                    if (transaction.Harvest.Status == "sold_out" && transaction.Harvest.QuantityAvailable > 0)
                    {
                        transaction.Harvest.Status = "available";
                    }

                    // Force EF Core to track the Harvest update
                    _context.Entry(transaction.Harvest).State = EntityState.Modified;

                    TempData["Success"] = "Order rejected. Stock returned to inventory.";
                }
                else if (action == "approve")
                {
                    // --- APPROVE LOGIC (KEEP DEDUCTION) ---
                    transaction.Status = "completed";

                    // Stock remains deducted.
                    TempData["Success"] = "Order approved successfully!";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Manage");
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }
    }
}