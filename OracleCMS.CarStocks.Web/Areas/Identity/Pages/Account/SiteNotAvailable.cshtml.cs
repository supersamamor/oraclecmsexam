using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace OracleCMS.CarStocks.Web.Areas.Identity.Pages.Account;
[AllowAnonymous]
public class SiteNotYetAvailableModel : PageModel
{
    public IActionResult OnGet()  {
        return Page();
    }
}
