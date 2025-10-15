using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParkingZone.Data;
using ParkingZone.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ParkingZone.Pages.Client
{
    [Authorize(Roles = "client")]
    [ValidateAntiForgeryToken]
    public class ReservationModel : PageModel
    {
        private readonly ParkingZoneContext _context;
        public ReservationModel(ParkingZoneContext context) => _context = context;

        [BindProperty] public Reservation Reservation { get; set; } = new();
        public Reservation? ActiveReservation { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            ActiveReservation = await _context.Reservations
                .Include(r => r.Space)
                .Where(r => r.UserId == userId && r.Status == ReservationStatus.active)
                .OrderByDescending(r => r.EntryTime)
                .FirstOrDefaultAsync();

            if (ActiveReservation != null &&
                DateTime.UtcNow > ActiveReservation.EntryTime.AddMinutes(30))
            {
                ActiveReservation.Status = ReservationStatus.finished;
                _context.Update(ActiveReservation);
                await _context.SaveChangesAsync();
                ActiveReservation = null;
            }

            if (ActiveReservation != null) return Page();

            var activeIds = await _context.Reservations.AsNoTracking()
                .Where(r => r.Status == ReservationStatus.active)
                .Select(r => r.SpaceId)
                .Distinct()
                .ToListAsync();

            var spaces = await _context.Spaces.AsNoTracking()
                .OrderBy(s => s.Code)
                .Select(s => new
                {
                    id = s.Id,
                    code = s.Code,
                    state = activeIds.Contains(s.Id)
                        ? "busy"
                        : (s.Available ? "available" : "reserved")
                })
                .ToListAsync();

            ViewData["SpacesJson"] = JsonSerializer.Serialize(spaces);
            return Page();
        }

        // GET para polling del mapa
        public async Task<IActionResult> OnGetMapAsync()
        {
            var activeIds = await _context.Reservations.AsNoTracking()
                .Where(r => r.Status == ReservationStatus.active)
                .Select(r => r.SpaceId)
                .Distinct()
                .ToListAsync();

            var spaces = await _context.Spaces.AsNoTracking()
                .OrderBy(s => s.Code)
                .Select(s => new
                {
                    id = s.Id,
                    code = s.Code,
                    state = activeIds.Contains(s.Id)
                        ? "busy"
                        : (s.Available ? "available" : "reserved")
                })
                .ToListAsync();

            return new JsonResult(spaces);
        }

        // POST crear reserva
        public async Task<IActionResult> OnPostCreateAsync([FromBody] JsonElement data)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!data.TryGetProperty("spaceId", out var p)) return BadRequest("Solicitud inválida.");
            var spaceId = p.GetInt32();

            var already = await _context.Reservations
                .AnyAsync(r => r.UserId == userId && r.Status == ReservationStatus.active);
            if (already) return BadRequest("Ya tienes una reserva activa.");

            var space = await _context.Spaces.FirstOrDefaultAsync(s => s.Id == spaceId);
            if (space == null) return BadRequest("Espacio inexistente.");
            if (!space.Available) return BadRequest("Espacio no disponible.");

            var occupied = await _context.Reservations
                .AnyAsync(r => r.SpaceId == spaceId && r.Status == ReservationStatus.active);
            if (occupied) return BadRequest("El espacio está ocupado.");

            var reservation = new Reservation
            {
                UserId = userId,
                SpaceId = spaceId,
                EntryTime = DateTime.UtcNow,
                Status = ReservationStatus.active
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return new OkResult();
        }

        // POST cancelar reserva activa
        public async Task<IActionResult> OnPostCancelAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var active = await _context.Reservations
                .Where(r => r.UserId == userId && r.Status == ReservationStatus.active)
                .FirstOrDefaultAsync();

            if (active != null)
            {
                active.Status = ReservationStatus.cancelled;
                _context.Update(active);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}

