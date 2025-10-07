using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingZone.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public async Task OnPost()
        {
            await HttpContext.SignOutAsync();
            Response.Redirect("/Index");
        }
    }
}
