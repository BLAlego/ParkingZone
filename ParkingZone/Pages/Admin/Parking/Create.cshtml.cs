using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ParkingZone.Data;
using ParkingZone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingZone.Pages.Admin.Parking
{

    [Authorize(Roles = "admin")]

    public class CreateModel : PageModel
    {
        private readonly ParkingZone.Data.ParkingZoneContext _context;

        public CreateModel(ParkingZone.Data.ParkingZoneContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["SpaceId"] = new SelectList(_context.Spaces, "Id", "Code");
        ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Plate");
            return Page();
        }

        [BindProperty]
        public Models.Parking Parking { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Parking.Add(Parking);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
