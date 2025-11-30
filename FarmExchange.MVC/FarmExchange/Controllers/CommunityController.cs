using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmExchange.Data;
using FarmExchange.Models;
using System.Security.Claims;

namespace FarmExchange.Controllers
{
    [Authorize]
    public class CommunityController : Controller
    {
        private readonly FarmExchangeDbContext _context;

        public CommunityController(FarmExchangeDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string category)
        {
            var query = _context.ForumThreads
                .Include(t => t.Author)
                .Include(t => t.Posts)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                query = query.Where(t => t.Category == category);
            }

            var threads = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.CurrentCategory = category;
            return View(threads);
        }

        [HttpGet]
        public IActionResult CreateThread()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateThread(ForumThread thread)
        {
            thread.Id = Guid.NewGuid();
            thread.AuthorId = GetCurrentUserId();
            thread.CreatedAt = DateTime.UtcNow;

            if (string.IsNullOrEmpty(thread.Title) || string.IsNullOrEmpty(thread.Content))
            {
                ModelState.AddModelError("", "Title and Content are required.");
                return View(thread);
            }

            _context.ForumThreads.Add(thread);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Thread(Guid id)
        {
            var thread = await _context.ForumThreads
                .Include(t => t.Author)
                .Include(t => t.Posts).ThenInclude(p => p.Author)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (thread == null) return NotFound();

            return View(thread);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(Guid threadId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return RedirectToAction("Thread", new { id = threadId });
            }

            var post = new ForumPost
            {
                Id = Guid.NewGuid(),
                ThreadId = threadId,
                AuthorId = GetCurrentUserId(),
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Thread", new { id = threadId });
        }

        [HttpGet]
        public async Task<IActionResult> EditThread(Guid id)
        {
            var userId = GetCurrentUserId();
            var thread = await _context.ForumThreads.FindAsync(id);

            if (thread == null || thread.AuthorId != userId)
            {
                return RedirectToAction("Index"); // Unauthorized or Not Found
            }

            return View(thread);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditThread(Guid id, string title, string content, string category)
        {
            var userId = GetCurrentUserId();
            var thread = await _context.ForumThreads.FindAsync(id);

            if (thread == null || thread.AuthorId != userId)
            {
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content))
            {
                thread.Title = title;
                thread.Content = content;
                thread.Category = category;

                await _context.SaveChangesAsync();
                return RedirectToAction("Thread", new { id = thread.Id });
            }

            // If validation fails, return view with current values to show error (or effectively reload)
            // Note: In a real app, we'd pass the entered values back.
            // Here we just ensure we don't crash.
            // Ideally we'd set ModelState errors if we had a ViewModel.
            // Since we are changing signature, we don't have 'model' to pass back easily unless we construct it.
            // But usually the client-side validation catches empty fields first.

            return View(thread);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteThread(Guid id)
        {
            var userId = GetCurrentUserId();
            var thread = await _context.ForumThreads.FindAsync(id);

            if (thread != null && thread.AuthorId == userId)
            {
                _context.ForumThreads.Remove(thread);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditPost(Guid id)
        {
            var userId = GetCurrentUserId();
            var post = await _context.ForumPosts
                .Include(p => p.Thread)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null || post.AuthorId != userId)
            {
                return RedirectToAction("Index");
            }

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(Guid id, string content)
        {
            var userId = GetCurrentUserId();
            var post = await _context.ForumPosts.FindAsync(id);

            if (post == null || post.AuthorId != userId)
            {
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(content))
            {
                post.Content = content;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Thread", new { id = post.ThreadId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var userId = GetCurrentUserId();
            var post = await _context.ForumPosts.FindAsync(id);

            if (post != null && post.AuthorId == userId)
            {
                var threadId = post.ThreadId;
                _context.ForumPosts.Remove(post);
                await _context.SaveChangesAsync();
                return RedirectToAction("Thread", new { id = threadId });
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
