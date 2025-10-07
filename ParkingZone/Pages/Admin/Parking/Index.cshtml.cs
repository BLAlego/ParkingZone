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

    public class IndexModel : PageModel
    {
        private readonly ParkingZone.Data.ParkingZoneContext _context;

        public IndexModel(ParkingZone.Data.ParkingZoneContext context)
        {
            _context = context;
        }

        public IList<Models.Parking> Parking { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Parking = await _context.Parking
                .Include(p => p.Space)
                .Include(p => p.Vehicle).ToListAsync();
        }
    }
}
