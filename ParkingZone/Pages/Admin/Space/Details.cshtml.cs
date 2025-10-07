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

namespace ParkingZone.Pages.Admin.Space
{
    [Authorize(Roles = "admin")]

    public class DetailsModel : PageModel
    {
        private readonly ParkingZone.Data.ParkingZoneContext _context;

        public DetailsModel(ParkingZone.Data.ParkingZoneContext context)
        {
            _context = context;
        }

        public Models.Space Space { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var space = await _context.Spaces.FirstOrDefaultAsync(m => m.Id == id);

            if (space is not null)
            {
                Space = space;

                return Page();
            }

            return NotFound();
        }
    }
}
