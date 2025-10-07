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
    [Authorize(Roles = "client")]

    public class DetailsModel : PageModel
    {
        private readonly ParkingZone.Data.ParkingZoneContext _context;

        public DetailsModel(ParkingZone.Data.ParkingZoneContext context)
        {
            _context = context;
        }

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
    }
}
