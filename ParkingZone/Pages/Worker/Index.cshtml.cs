using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingZone.Pages.Worker
{
    [Authorize(Roles = "worker")]

    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
