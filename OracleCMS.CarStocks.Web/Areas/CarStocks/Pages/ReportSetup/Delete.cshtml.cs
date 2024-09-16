using OracleCMS.CarStocks.Application.Features.CarStocks.Report.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Report.Queries;
using OracleCMS.CarStocks.Web.Areas.CarStocks.Models;
using OracleCMS.CarStocks.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Pages.ReportSetup;

[Authorize(Policy = Permission.ReportSetup.Delete)]
public class DeleteModel : BasePageModel<DeleteModel>
{
    [BindProperty]
    public ReportViewModel Report { get; set; } = new();
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
        return await PageFrom(async () => await Mediatr.Send(new GetReportByIdQuery(id)), Report);
    }

    public async Task<IActionResult> OnPost()
    {
        return await TryThenRedirectToPage(async () => await Mediatr.Send(new DeleteReportCommand { Id = Report.Id }), "Index");
    }
}
