using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParkingZone.Data;

namespace ParkingZone.Pages.Admin.Space
{
    [Authorize(Roles = "admin")]
    public class CreateModel : PageModel
    {
        private readonly ParkingZoneContext _context;

        public CreateModel(ParkingZoneContext context) => _context = context;

        [BindProperty]
        public Models.Space Space { get; set; } = new();

        public void OnGet()
        {
            // Defaults sensatos
            Space.Available = true;
            if (Space.Level <= 0) Space.Level = 1;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Normalizaciones
            Space.Code = (Space.Code ?? string.Empty).Trim().ToUpperInvariant();
            if (Space.Level <= 0) Space.Level = 1;
            if (Space.HourlyRate < 0) Space.HourlyRate = 0; // corrige negativos

            // Validaciones de negocio
            if (string.IsNullOrWhiteSpace(Space.Code))
                ModelState.AddModelError("Space.Code", "Code is required.");

            if (Space.HourlyRate <= 0)
                ModelState.AddModelError("Space.HourlyRate", "Hourly rate must be greater than 0.");

            // Unicidad de Code
            var exists = await _context.Spaces
                .AnyAsync(s => s.Code == Space.Code);
            if (exists)
                ModelState.AddModelError("Space.Code", "This code already exists.");

            if (!ModelState.IsValid)
                return Page();

            _context.Spaces.Add(Space);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
