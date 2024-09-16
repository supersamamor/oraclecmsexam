using OracleCMS.CarStocks.Application.Features.CarStocks.Approval.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Approval.Queries;
using OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Queries;
using OracleCMS.CarStocks.Web.Areas.CarStocks.Models;
using OracleCMS.CarStocks.Web.Models;
using OracleCMS.CarStocks.Core.CarStocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Pages.Cars;

[Authorize(Policy = Permission.Cars.Approve)]
public class ApproveModel : BasePageModel<ApproveModel>
{
    [BindProperty]
    public CarsViewModel Cars { get; set; } = new();
    [BindProperty]
    public string? ApprovalStatus { get; set; }
	[BindProperty]
	public string? ApprovalRemarks { get; set; }
    public async Task<IActionResult> OnGet(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        _ = (await Mediatr.Send(new GetApprovalStatusPerApproverByIdQuery(id, ApprovalModule.Cars))).Select(l => ApprovalStatus = l);
        return await PageFrom(async () => await Mediatr.Send(new GetCarsByIdQuery(id)), Cars);
    }

    public async Task<IActionResult> OnPost(string handler)
    {
        if (handler == "Approve")
        {
            return await Approve();
        }
        else if (handler == "Reject")
        {
            return await Reject();
        }
        return Page();
    }
    private async Task<IActionResult> Approve()
    {
        return await TryThenRedirectToPage(async () => await Mediatr.Send(new ApproveCommand(Cars.Id, ApprovalRemarks, ApprovalModule.Cars)), "Approve", true);
    }
    private async Task<IActionResult> Reject()
    {
        return await TryThenRedirectToPage(async () => await Mediatr.Send(new RejectCommand(Cars.Id, ApprovalRemarks, ApprovalModule.Cars)), "Approve", true);
    }
}
