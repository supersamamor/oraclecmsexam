using OracleCMS.CarStocks.Application.Features.CarStocks.Report.Commands;
using OracleCMS.CarStocks.Web.Areas.CarStocks.Models;
using OracleCMS.CarStocks.Web.Models;
using OracleCMS.CarStocks.Web.Service;
using LanguageExt.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static LanguageExt.Prelude;
namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Pages.ReportSetup;

[Authorize(Policy = Permission.ReportSetup.Create)]
public class AddModel(AIReportQueryGenerationServices aiReportQueryGenerationServices) : BasePageModel<AddModel>
{
    [BindProperty]
    public ReportViewModel Report { get; set; } = new();
    [BindProperty]
    public string? RemoveSubDetailId { get; set; }
    [BindProperty]
    public string? AsyncAction { get; set; }
    public IActionResult OnGet()
    {

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string handler)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        if (handler == "Save")
        {
            return await TryThenRedirectToPage(async () => await Mediatr.Send(Mapper.Map<AddReportCommand>(Report)), "Details", true);
        }
        else if (handler == "GenerateQueryFromAI")
        {
            try
            {
                if (string.IsNullOrEmpty(Report.ReportDescription))
                {
                    NotyfService.Warning(Localizer["Report description is required if you want to generate query via AI."]);
                    return Page();
                }
                var chatGPTResult = await aiReportQueryGenerationServices.SQLReportQueryGeneration(Report.ReportDescription, Report.ReportOrChartType, token: new CancellationToken());
                var command = Mapper.Map<AddReportWithSQLQueryFromAICommand>(Report);
                command.SQLQuery = chatGPTResult ?? "";
                var result = await TryAsync(async () => await Mediatr.Send(command))
               .IfFail(ex =>
               {
                   Logger.LogError(ex, "Exception in OnPostAsync");
                   return Fail<Error, Core.CarStocks.ReportState>(Localizer[$"Something went wrong. Please contact the system administrator."] + $" TraceId = {HttpContext.TraceIdentifier}");
               });
                return result.Match<IActionResult>(
                    Succ: entity =>
                    {
                        NotyfService.Success(Localizer["Report query generated successfully."]);
                        return RedirectToPage("Edit", new { id = entity.Id });
                    },
                    Fail: errors =>
                    {
                        AddModelError(errors, notifyError: true);
                        return Page();
                    });
            }
            catch (Exception ex)
            {
                NotyfService.Error(Localizer["An error occurred while generating query from AI."]);
                Logger.LogError(ex, "An error occurred while generating query from AI.");
                return Page();
            }
        }
        return Page();
    }
    public IActionResult OnPostChangeFormValue()
    {
        ModelState.Clear();
        if (AsyncAction == "AddReportQueryFilter")
        {
            return AddReportQueryFilter();
        }
        if (AsyncAction == "RemoveReportQueryFilter")
        {
            return RemoveReportQueryFilter();
        }


        return Partial("_InputFieldsPartial", Report);
    }

    private IActionResult AddReportQueryFilter()
    {
        ModelState.Clear();
        if (Report!.ReportQueryFilterList == null) { Report!.ReportQueryFilterList = new List<ReportQueryFilterViewModel>(); }
        Report!.ReportQueryFilterList!.Add(new ReportQueryFilterViewModel() { ReportId = Report.Id });
        return Partial("_InputFieldsPartial", Report);
    }
    private IActionResult RemoveReportQueryFilter()
    {
        ModelState.Clear();
        Report.ReportQueryFilterList = Report!.ReportQueryFilterList!.Where(l => l.Id != RemoveSubDetailId).ToList();
        return Partial("_InputFieldsPartial", Report);
    }

}
