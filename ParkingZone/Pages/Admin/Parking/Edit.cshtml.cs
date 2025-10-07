using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public class EditModel : PageModel
    {
        private readonly ParkingZone.Data.ParkingZoneContext _context;

        public EditModel(ParkingZone.Data.ParkingZoneContext context)
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

            var parking =  await _context.Parking.FirstOrDefaultAsync(m => m.SpaceId == id);
            if (parking == null)
            {
                return NotFound();
            }
            Parking = parking;
           ViewData["SpaceId"] = new SelectList(_context.Spaces, "Id", "Code");
           ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Plate");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Parking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParkingExists(Parking.SpaceId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ParkingExists(int id)
        {
            return _context.Parking.Any(e => e.SpaceId == id);
        }
    }
}
