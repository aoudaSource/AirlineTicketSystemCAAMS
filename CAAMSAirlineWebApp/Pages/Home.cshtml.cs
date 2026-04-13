using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CAAMSAirlineWebApp.Pages
{
    [Authorize]
    public class HomeModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
