using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;   // PasswordHasher
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParkingZone.Data;

namespace ParkingZone.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ParkingZoneContext _db;

        public LoginModel(ParkingZoneContext db) => _db = db;

        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1) Validación básica
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter your email and password.";
                return Page();
            }

            // 2) Normaliza email y busca usuario activo
            var normalizedEmail = Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsActive);
            if (user == null)
            {
                ErrorMessage = "Invalid credentials.";
                return Page();
            }

            // 3) Verifica password hash (mismo algoritmo que usas al crear)
            var hasher = new PasswordHasher<object?>();
            var verification = hasher.VerifyHashedPassword(null, user.HashPassword, Password);
            if (verification == PasswordVerificationResult.Failed)
            {
                ErrorMessage = "Incorrect password.";
                return Page();
            }
            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.HashPassword = hasher.HashPassword(null, Password);
                await _db.SaveChangesAsync();
            }

            // 4) Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                    AllowRefresh = true
                });

            // 5) Redirect por rol
            // Roles esperados en tu sistema: "admin", "Trabajador", "client"
            var role = user.Role.ToString();
            return role switch
            {
                "admin" => RedirectToPage("/Admin/Index"),
                "Trabajador" => RedirectToPage("/Worker/Index"),
                "client" => RedirectToPage("/Client/Reservation"),
                _ => RedirectToPage("/Index")
            };
        }
    }
}
