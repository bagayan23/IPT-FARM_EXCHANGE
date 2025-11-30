using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmExchange.Data;
using FarmExchange.Models;
using System.Security.Claims;

namespace FarmExchange.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly FarmExchangeDbContext _context;

        public MessageController(FarmExchangeDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var profile = await _context.Profiles.FindAsync(userId);

            var sentMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m => m.SenderId == userId)
                .ToListAsync();

            var receivedMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m => m.RecipientId == userId)
                .ToListAsync();

            var allMessages = sentMessages.Concat(receivedMessages)
                .OrderByDescending(m => m.CreatedAt)
                .ToList();

            ViewBag.Profile = profile;
            ViewBag.AllUsers = await _context.Profiles
                .Where(p => p.Id != userId)
                .ToListAsync();

            ViewBag.Farmers = await _context.Profiles
                .Where(p => p.UserType == UserType.Farmer)
                .ToListAsync();

            return View(allMessages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(Guid recipientId, string subject, string content)
        {
            var userId = GetCurrentUserId();

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = userId,
                RecipientId = recipientId,
                Subject = subject,
                Content = content,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message sent successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();
            var message = await _context.Messages.FindAsync(id);

            if (message != null && message.RecipientId == userId)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var userId = GetCurrentUserId();
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null || (message.SenderId != userId && message.RecipientId != userId))
            {
                return RedirectToAction("Index");
            }

            if (message.RecipientId == userId && !message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            var profile = await _context.Profiles.FindAsync(userId);
            ViewBag.Profile = profile;

            return View(message);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var userId = GetCurrentUserId();
            var message = await _context.Messages.FindAsync(id);

            if (message == null || message.SenderId != userId)
            {
                return RedirectToAction("Index");
            }

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, string subject, string content)
        {
            var userId = GetCurrentUserId();
            var message = await _context.Messages.FindAsync(id);

            if (message == null || message.SenderId != userId)
            {
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(content))
            {
                message.Subject = subject;
                message.Content = content;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Message updated successfully!";
                return RedirectToAction("Details", new { id = message.Id });
            }

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            var message = await _context.Messages.FindAsync(id);

            if (message != null && message.SenderId == userId)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Message deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }
    }
}