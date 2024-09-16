using OracleCMS.CarStocks.Application.Features.CarStocks.Dealers.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Dealers.Queries;
using OracleCMS.CarStocks.Web.Areas.CarStocks.Models;
using OracleCMS.CarStocks.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Pages.Dealers;

[Authorize(Policy = Permission.Dealers.Delete)]
public class DeleteModel : BasePageModel<DeleteModel>
{
    [BindProperty]
    public DealersViewModel Dealers { get; set; } = new();
	[BindProperty]
    public string? RemoveSubDetailId { get; set; }
    [BindProperty]
    public string? AsyncAction { get; set; }
    public async Task<IActionResult> OnGet(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        return await PageFrom(async () => await Mediatr.Send(new GetDealersByIdQuery(id)), Dealers);
    }

    public async Task<IActionResult> OnPost()
    {
        return await TryThenRedirectToPage(async () => await Mediatr.Send(new DeleteDealersCommand { Id = Dealers.Id }), "Index");
    }
}
