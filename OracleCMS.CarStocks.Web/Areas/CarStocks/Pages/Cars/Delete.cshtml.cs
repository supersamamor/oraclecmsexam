using OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Queries;
using OracleCMS.CarStocks.Web.Areas.CarStocks.Models;
using OracleCMS.CarStocks.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Pages.Cars;

[Authorize(Policy = Permission.Cars.Delete)]
public class DeleteModel : BasePageModel<DeleteModel>
{
    [BindProperty]
    public CarsViewModel Cars { get; set; } = new();
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
        return await PageFrom(async () => await Mediatr.Send(new GetCarsByIdQuery(id)), Cars);
    }

    public async Task<IActionResult> OnPost()
    {
        return await TryThenRedirectToPage(async () => await Mediatr.Send(new DeleteCarsCommand { Id = Cars.Id }), "Index");
    }
}
