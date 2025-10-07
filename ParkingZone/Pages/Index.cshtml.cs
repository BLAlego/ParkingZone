using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParkingZone.Data;

namespace ParkingZone.Pages
{
    [Authorize] // exige login
    public class IndexModel : PageModel
    {
        private readonly ParkingZoneContext _db;

        public IndexModel(ParkingZoneContext db) => _db = db;

        public string? UserName { get; set; }
        public string? UserRole { get; set; }

        // KPIs ligeros (se llenan sólo si hacen sentido para el rol)
        public int KpiUsers { get; set; }
        public int KpiSpaces { get; set; }
        public int KpiActiveReservations { get; set; }
        public int KpiAvailableSpaces { get; set; }

        public async Task OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                UserName = User.Identity!.Name;
                UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            }

            // KPIs según rol
            // Admin: usuarios, espacios, reservas activas
            // Trabajador: reservas activas, espacios disponibles
            // Client: sin KPIs (tiene CTAs)
            if (User.IsInRole("admin"))
            {
                (KpiUsers, KpiSpaces, KpiActiveReservations) = await LoadAdminKpisAsync();
            }
            else if (User.IsInRole("Trabajador"))
            {
                (KpiActiveReservations, KpiAvailableSpaces) = await LoadWorkerKpisAsync();
            }
        }

        private async Task<(int users, int spaces, int active)> LoadAdminKpisAsync()
        {
            var users = await _db.Users.AsNoTracking().CountAsync();
            var spaces = await _db.Spaces.AsNoTracking().CountAsync();
            var active = await _db.Reservations.AsNoTracking().CountAsync(r => r.Status == Models.ReservationStatus.active);
            return (users, spaces, active);
        }

        private async Task<(int active, int available)> LoadWorkerKpisAsync()
        {
            var active = await _db.Reservations.AsNoTracking().CountAsync(r => r.Status == Models.ReservationStatus.active);
            var available = await _db.Spaces.AsNoTracking().CountAsync(s => s.Available);
            return (active, available);
        }
    }
}
