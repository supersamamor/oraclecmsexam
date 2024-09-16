using OracleCMS.CarStocks.Application.Features.CarStocks.Stocks.Queries;
using OracleCMS.CarStocks.Web.Areas.CarStocks.Models;
using OracleCMS.CarStocks.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Pages.Stocks;

[Authorize(Policy = Permission.Stocks.View)]
public class DetailsModel : BasePageModel<DetailsModel>
{
    public StocksViewModel Stocks { get; set; } = new();
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
        return await PageFrom(async () => await Mediatr.Send(new GetStocksByIdQuery(id)), Stocks);
    }
}
