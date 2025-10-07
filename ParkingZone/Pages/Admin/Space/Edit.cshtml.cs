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

namespace ParkingZone.Pages.Admin.Space
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
        public Models.Space Space { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var space =  await _context.Spaces.FirstOrDefaultAsync(m => m.Id == id);
            if (space == null)
            {
                return NotFound();
            }
            Space = space;
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

            _context.Attach(Space).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpaceExists(Space.Id))
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

        private bool SpaceExists(int id)
        {
            return _context.Spaces.Any(e => e.Id == id);
        }
    }
}
