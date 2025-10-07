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

    public class IndexModel : PageModel
    {
        private readonly ParkingZone.Data.ParkingZoneContext _context;

        public IndexModel(ParkingZone.Data.ParkingZoneContext context)
        {
            _context = context;
        }

        public IList<Models.Space> Space { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Space = await _context.Spaces.ToListAsync();
        }
    }
}
