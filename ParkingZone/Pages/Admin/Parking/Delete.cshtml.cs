using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParkingZone.Data;
using ParkingZone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingZone.Pages.Admin.Parking
{
    [Authorize(Roles = "admin")]

    public class DeleteModel : PageModel
    {
        private readonly ParkingZone.Data.ParkingZoneContext _context;

        public DeleteModel(ParkingZone.Data.ParkingZoneContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.Parking Parking { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parking = await _context.Parking.FirstOrDefaultAsync(m => m.SpaceId == id);

            if (parking is not null)
            {
                Parking = parking;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parking = await _context.Parking.FindAsync(id);
            if (parking != null)
            {
                Parking = parking;
                _context.Parking.Remove(Parking);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
