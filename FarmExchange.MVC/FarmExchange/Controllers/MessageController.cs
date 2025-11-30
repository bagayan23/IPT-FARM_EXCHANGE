using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmExchange.Data;
using FarmExchange.Models;
using FarmExchange.ViewModels;
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

            var allMessages = sentMessages.Concat(receivedMessages);

            var conversations = allMessages
                .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
                .Select(g => {
                    var partnerId = g.Key;
                    var lastMessage = g.OrderByDescending(m => m.CreatedAt).First();
                    var unreadCount = g.Count(m => m.RecipientId == userId && !m.IsRead);
                    var partnerName = lastMessage.SenderId == userId ? lastMessage.Recipient.FullName : lastMessage.Sender.FullName;

                    return new ConversationViewModel
                    {
                        PartnerId = partnerId,
                        PartnerName = partnerName,
                        LastMessage = lastMessage,
                        UnreadCount = unreadCount
                    };
                })
                .OrderByDescending(c => c.LastMessage.CreatedAt)
                .ToList();

            ViewBag.Profile = profile;
            ViewBag.AllUsers = await _context.Profiles
                .Where(p => p.Id != userId)
                .ToListAsync();

            ViewBag.Farmers = await _context.Profiles
                .Where(p => p.UserType == UserType.Farmer)
                .ToListAsync();

            return View(conversations);
        }

        [HttpGet]
        public async Task<IActionResult> Conversation(Guid partnerId)
        {
            var userId = GetCurrentUserId();
            var partner = await _context.Profiles.FindAsync(partnerId);

            if (partner == null)
            {
                return RedirectToAction("Index");
            }

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m => (m.SenderId == userId && m.RecipientId == partnerId) ||
                           (m.SenderId == partnerId && m.RecipientId == userId))
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            // Mark received messages as read
            var unreadMessages = messages.Where(m => m.RecipientId == userId && !m.IsRead).ToList();
            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            ViewBag.CurrentUser = await _context.Profiles.FindAsync(userId);
            ViewBag.Partner = partner;

            return View(messages);
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
                Subject = subject ?? "No Subject",
                Content = content,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message sent successfully!";
            return RedirectToAction("Conversation", new { partnerId = recipientId });
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
        public async Task<IActionResult> Edit(Guid id, string content)
        {
            var userId = GetCurrentUserId();
            var message = await _context.Messages.FindAsync(id);

            if (message == null || message.SenderId != userId)
            {
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(content))
            {
                message.Content = content;
                message.IsEdited = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Message updated successfully!";

                // Determine partner ID for redirection
                var partnerId = message.SenderId == userId ? message.RecipientId : message.SenderId;
                return RedirectToAction("Conversation", new { partnerId = partnerId });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            var message = await _context.Messages.FindAsync(id);

            if (message != null && message.SenderId == userId)
            {
                message.IsDeleted = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Message deleted successfully!";

                // Determine partner ID for redirection
                var partnerId = message.SenderId == userId ? message.RecipientId : message.SenderId;
                return RedirectToAction("Conversation", new { partnerId = partnerId });
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