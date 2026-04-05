using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BMS.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
