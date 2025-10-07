using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingZone.Pages.Client
{
    [Authorize(Roles = "client")]

    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
