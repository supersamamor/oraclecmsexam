using OracleCMS.CarStocks.Application.Features.CarStocks.Approval.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Approval.Queries;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Web.Areas.CarStocks.Models;
using OracleCMS.CarStocks.Web.Models;
using DataTables.AspNetCore.Mvc.Binder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Pages.ApproverSetup;

[Authorize(Policy = Permission.ApproverSetup.View)]
public class PendingApprovalsModel : BasePageModel<PendingApprovalsModel>
{
    public ApproverSetupViewModel ApproverSetup { get; set; } = new();

    [DataTablesRequest]
    public DataTablesRequest? DataRequest { get; set; }
    [BindProperty]
    public string TableName { get; set; } = "";
    public IActionResult OnGet(string tableName)
    {
        TableName = tableName;
        return Page();
    }

    public async Task<IActionResult> OnPostListAllAsync(string tableName)
    {     
        var dataRequest = DataRequest!.ToQuery<GetPendingApprovalsQuery>();
        dataRequest.TableName = tableName;
        var result = await Mediatr.Send(dataRequest);
        return new JsonResult(result.Data
            .Select(e => new
            {
                e.DataId,
                e.RecordName,
                e.ApprovalId,
                ApprovalStatus = GetApprovalStatus(e.ApprovalStatus),
                EmailSendingStatus = GetSendingStatus(e.EmailSendingStatus),
                e.EmailSendingRemarks,
                e.EmailSendingDateTime,
                e.LastModifiedDate
            })
            .ToDataTablesResponse(DataRequest, result.TotalCount, result.MetaData.TotalItemCount)); ;
    }

    public async Task<IActionResult> OnGetSelect2Data([FromQuery] Select2Request request)
    {
        var result = await Mediatr.Send(request.ToQuery<GetApproverSetupQuery>(nameof(ApproverSetupState.TableName)));
        return new JsonResult(result.ToSelect2Response(e => new Select2Result { Id = e.Id, Text = e.TableName! }));
    }
    public async Task<IActionResult> OnGetResendApproval(string approvalId, string tableName)
    {
        TableName = tableName;
        if (!ModelState.IsValid)
        {
            return Page();
        }
        await Mediatr.Send(new ResendCommand(approvalId));
        NotyfService.Success(Localizer["Transaction successful"]);
        return RedirectToPage("PendingApprovals", new { tableName });
    }
    private static string GetApprovalStatus(string approvalStatus)
    {    
        switch (approvalStatus)
        {
            case ApprovalStatus.New:
                return @"<span class=""badge bg-secondary"">" + approvalStatus + "</span>";
            case ApprovalStatus.ForApproval:
                return @"<span class=""badge bg-info"">" + approvalStatus + "</span>";
            case ApprovalStatus.PartiallyApproved:
                return @"<span class=""badge bg-primary"">" + approvalStatus + "</span>";
            case ApprovalStatus.Approved:
                return @"<span class=""badge bg-success"">" + approvalStatus + "</span>";
            case ApprovalStatus.Rejected:
                return @"<span class=""badge bg-danger"">" + approvalStatus + "</span>";
            default:
                break;
        }
        return approvalStatus;
    }
    private static string GetSendingStatus(string sendingStatus)
    {
        switch (sendingStatus)
        {
            case SendingStatus.Pending:
                return @"<span class=""badge bg-primary"">" + sendingStatus + "</span>";
            case SendingStatus.Done:
                return @"<span class=""badge bg-success"">" + sendingStatus + "</span>";
            case SendingStatus.Failed:
                return @"<span class=""badge bg-danger"">" + sendingStatus + "</span>";    
            default:
                break;
        }
        return sendingStatus;
    }
}
