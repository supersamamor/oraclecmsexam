using OracleCMS.CarStocks.Application.Features.CarStocks.Approval.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Approval.Queries;
using OracleCMS.CarStocks.Web.Areas.CarStocks.Models;
using OracleCMS.CarStocks.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Pages.ApproverSetup;

[Authorize(Policy = Permission.ApproverSetup.Edit)]
public class EditModel : BasePageModel<EditModel>
{
    [BindProperty]
    public ApproverSetupViewModel ApproverSetup { get; set; } = new();
    [BindProperty]
    public string? RemoveSubDetailId { get; set; }
    [BindProperty]
    public string? AsyncAction { get; set; }
    [BindProperty]
    public string? MoveUpDownId { get; set; }
    public async Task<IActionResult> OnGet(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        return await PageFrom(async () => await Mediatr.Send(new GetApproverSetupByIdQuery(id)), ApproverSetup);
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        return await TryThenRedirectToPage(async () => await Mediatr.Send(Mapper.Map<EditApproverSetupCommand>(ApproverSetup)), "Details", true);
    }
    public IActionResult OnPostChangeFormValue()
    {
        ModelState.Clear();
        if (AsyncAction == "AddApproverAssignment")
        {
            return AddApproverAssignment();
        }
        if (AsyncAction == "RemoveApproverAssignment")
        {
            return RemoveApproverAssignment();
        }
        if (AsyncAction == "MoveUpApproverAssignment")
        {
            return MoveApproverAssignment(true);
        }
        if (AsyncAction == "MoveDownApproverAssignment")
        {
            return MoveApproverAssignment(false);
        }
        return Partial("_InputFieldsPartial", ApproverSetup);
    }

    private IActionResult AddApproverAssignment()
    {
        ModelState.Clear();
        if (ApproverSetup!.ApproverAssignmentList == null) { ApproverSetup!.ApproverAssignmentList = new List<ApproverAssignmentViewModel>(); }
        ApproverSetup!.ApproverAssignmentList!.Add(new ApproverAssignmentViewModel() { ApproverSetupId = ApproverSetup.Id, Sequence = ApproverSetup!.ApproverAssignmentList.Count });
        return Partial("_InputFieldsPartial", ApproverSetup);
    }
    private IActionResult RemoveApproverAssignment()
    {
        ModelState.Clear();
        ApproverSetup.ApproverAssignmentList = ApproverSetup!.ApproverAssignmentList!.Where(l => l.Id != RemoveSubDetailId).ToList();
        return Partial("_InputFieldsPartial", ApproverSetup);
    }
    private IActionResult MoveApproverAssignment(bool moveUp)
    {
        ModelState.Clear();
        int? currentSequence = ApproverSetup!.ApproverAssignmentList!.Where(l => l.Id == MoveUpDownId).FirstOrDefault()?.Sequence;
        if (currentSequence != null)
        {
            int newSequence = (int)currentSequence + 1;
            if (moveUp)
            {
                newSequence = (int)currentSequence - 1;
            }
            _ = ApproverSetup!.ApproverAssignmentList!.Where(c => c.Sequence == newSequence).Select(c => { c.Sequence = (int)currentSequence; return c; }).ToList();
            _ = ApproverSetup!.ApproverAssignmentList!.Where(c => c.Id == MoveUpDownId).Select(c => { c.Sequence = newSequence; return c; }).ToList();
        }
        return Partial("_InputFieldsPartial", ApproverSetup);
    }
}
