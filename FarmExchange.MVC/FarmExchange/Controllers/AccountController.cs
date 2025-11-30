using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FarmExchange.Data;
using FarmExchange.Models;
using FarmExchange.ViewModels;
// Add this for BCrypt
using BCrypt.Net;

namespace FarmExchange.Controllers
{
    public class AccountController : Controller
    {
        private readonly FarmExchangeDbContext _context;

        public AccountController(FarmExchangeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Profiles
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email address is already in use.");
                    return View(model);
                }

                // --- 1. HASH THE PASSWORD ---
                // We do NOT save model.Password directly. We hash it.
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                var profile = new Profile
                {
                    Id = Guid.NewGuid(),
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    ExtensionName = model.ExtensionName,
                    Email = model.Email,

                    // Save the HASH, not the plain text
                    PasswordHash = passwordHash,

                    UserType = model.UserType,
                    Phone = model.Phone,
                    Location = model.Location,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Profiles.Add(profile);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // --- 2. VERIFY THE PASSWORD ---

                // First, find the user by Email ONLY
                var user = await _context.Profiles
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                // Then, check if the password matches the hash
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    // Logic is same as before...
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        // Using FullName (Computed Property) for display
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim("UserType", user.UserType.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = true };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    if (user.UserType == UserType.Farmer)
                    {
                        return RedirectToAction("Manage", "Harvest");
                    }
                    else
                    {
                        return RedirectToAction("Browse", "Harvest");
                    }
                }

                // If user is null OR password check failed
                ModelState.AddModelError("", "Invalid email or password.");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}