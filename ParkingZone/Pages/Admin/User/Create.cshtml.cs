using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;            // PasswordHasher
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParkingZone.Data;
using ParkingZone.Models;
using System.ComponentModel.DataAnnotations;

namespace ParkingZone.Pages.Admin.User
{
    [Authorize(Roles = "admin")]

    public class CreateModel : PageModel
    {
        private readonly ParkingZoneContext _context;

        public CreateModel(ParkingZoneContext context) => _context = context;

        [BindProperty]
        public Models.User User { get; set; } = new();

        [BindProperty, Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [BindProperty, Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public void OnGet()
        {
            // defaults
            User.IsActive = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Normaliza entradas
            User.Name = (User.Name ?? string.Empty).Trim();
            User.Email = (User.Email ?? string.Empty).Trim().ToLowerInvariant();

            // Email único
            if (await _context.Users.AnyAsync(u => u.Email == User.Email))
                ModelState.AddModelError("User.Email", "Email already exists.");

            if (!ModelState.IsValid)
                return Page(); // al recargar, los fields de password se limpian por seguridad (comportamiento normal)

            // Hashear password (sin paquetes externos)
            var hasher = new PasswordHasher<object?>();
            User.HashPassword = hasher.HashPassword(null, Password);

            // Server-side fields
            User.RegistrationDate = DateTime.UtcNow;

            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
