using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using OracleCMS.Common.Core.Base.Models;
using OracleCMS.Common.Data;
using OracleCMS.Common.Utility.Extensions;
using OracleCMS.Common.Web.Utility.Extensions;
using OracleCMS.Common.Web.Utility.Helpers;
using OracleCMS.CarStocks.Application.Features.CarStocks.UploadProcessor.Commands;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using static LanguageExt.Prelude;

namespace OracleCMS.CarStocks.Web.Models;

public class BasePageModel<T> : PageModel where T : class
{
    private ILogger<T>? _logger;
    private IStringLocalizer<SharedResource>? _localizer;
    private INotyfService? _notyfService;
    private IMediator? _mediatr;
    private IMapper? _mapper;
    private string? _traceId;
    private IConfiguration? _configuration;

    protected ILogger<T> Logger => _logger ??= HttpContext.RequestServices.GetService<ILogger<T>>()!;
    protected IStringLocalizer<SharedResource> Localizer => _localizer ??= HttpContext.RequestServices.GetService<IStringLocalizer<SharedResource>>()!;
    protected INotyfService NotyfService => _notyfService ??= HttpContext.RequestServices.GetService<INotyfService>()!;
    protected IMediator Mediatr => _mediatr ??= HttpContext.RequestServices.GetService<IMediator>()!;
    protected IMapper Mapper => _mapper ??= HttpContext.RequestServices.GetService<IMapper>()!;
    protected string TraceId => _traceId ??= Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    protected IConfiguration Configuration => _configuration ??= HttpContext.RequestServices.GetService<IConfiguration>()!;
    /// <summary>
    /// Maps <typeparamref name="TEntity"/> to <typeparamref name="TModel"/> and returns the page.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="e"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    protected IActionResult MapAndReturnPage<TEntity, TModel>(TEntity e, TModel model)
    {
        Mapper.Map(e, model);
        return Page();
    }

    /// <summary>
    /// Executes the function <paramref name="f"/>. If the function throws an exception
    /// or returns a validation error, return the page and display the errors.
    /// Otherwise, redirect to the index page.
    /// </summary>
    /// <typeparam name="TEntity">An instance of <see cref="IEntity"/></typeparam>
    /// <param name="f"></param>
    /// <param name="pageName">The name of the index page. Defaults to "Index".</param>
    /// <returns></returns>
    protected async Task<IActionResult> TryThenRedirectToIndexPage<TEntity>(Func<Task<Validation<Error, TEntity>>> f, string pageName = "Index")
        where TEntity : IEntity =>
        await TryAsync(() => f()).ToValidation(ex => ToError(ex))
                                 .BindT(t => t)
                                 .ToActionResult(success: succ => NotifyAndRedirectToPage(succ, pageName), fail: errors => PageWithErrors(errors));

    /// <summary>
    /// Executes the function <paramref name="f"/>. If the function throws an exception
    /// or returns a validation error, return the page and display the errors.
    /// Otherwise, redirect to the details page.
    /// </summary>
    /// <typeparam name="TEntity">An instance of <see cref="IEntity"/></typeparam>
    /// <param name="f"></param>
    /// <param name="pageName">The name of the index page. Defaults to "Details".</param>
    /// <returns></returns>
    protected async Task<IActionResult> TryThenRedirectToDetailsPage<TEntity>(Func<Task<Validation<Error, TEntity>>> f, string pageName = "Details")
        where TEntity : IEntity =>
        await TryAsync(() => f()).ToValidation(ex => ToError(ex))
                                 .BindT(t => t)
                                 .ToActionResult(success: succ => NotifyAndRedirectToPage(succ, pageName, new { id = succ.Id }),
                                                 fail: errors => PageWithErrors(errors));

    private Error ToError(Exception ex)
    {
        Logger.LogError(ex, "Exception encountered");
        return Error.New(Localizer[$"Something went wrong. Please contact the system administrator."] + $" TraceId = {HttpContext.TraceIdentifier}");
    }

    private IActionResult PageWithErrors(Seq<Error> errors)
    {
        Logger.LogError("Error encountered. Errors: {Errors}", errors.Join().ToString());
        errors.Iter(error => ModelState.AddModelError("", error.ToString()));
        return base.Page();
    }

    private IActionResult NotifyAndRedirectToPage<TEntity>(TEntity succ, string pageName, object? routeValues = null) where TEntity : IEntity
    {
        Logger.LogInformation("Details of affected record. ID: {ID}, Record: {Record}", succ.Id, succ.ToString());
        NotyfService.Success(Localizer["Transaction successful"]);
        return routeValues is null ? RedirectToPage(pageName) : RedirectToPage(pageName, routeValues);
    }
    #region Old Methods  
    protected async Task<IActionResult> PageFrom<TEntity, TModel>(Func<Task<Option<TEntity>>> f, TModel model) =>
        await f().ToActionResult(
            e =>
            {
                Mapper.Map(e, model);
                return Page();
            },
            none: null);

    protected async Task<IActionResult> TryThenRedirectToPage<TEntity>(Func<Task<Validation<Error, TEntity>>> f, string pageName, bool isDetailsPage = false)
        where TEntity : IEntity =>
        await TryAsync(() => f()).IfFail(ex =>
        {
            Logger.LogError(ex, "Exception encountered");
            return Fail<Error, TEntity>(Localizer[$"Something went wrong. Please contact the system administrator."] + $" TraceId = {HttpContext.TraceIdentifier}");
        }).ToActionResult(
            success: succ =>
            {
                NotyfService.Success(Localizer["Transaction successful"]);
                Logger.LogInformation("Details of affected record. ID: {ID}, Record: {Record}", succ.Id, succ.ToString());
                return isDetailsPage ? RedirectToPage(pageName, new { id = succ.Id }) : RedirectToPage(pageName);
            },
            fail: errors =>
            {
                errors.Iter(error => ModelState.AddModelError("", error.ToString()));
                Logger.LogError("Error encountered. Errors: {Errors}", errors.Join().ToString());
                return Page();
            });
    #endregion
    protected async Task<string> UploadFile<TUploadModel>(string moduleName, string fieldName, string id, IFormFile? formFile, string? fileName = null)
    {
        string filePath = "";
        if (formFile != null)
        {
            var permittedExtensions = Configuration.GetValue<string>("UsersUpload:DocumentPermitedExtensions")?.Split(',').ToArray();
            var fileSizeLimit = Configuration.GetValue<long>("UsersUpload:FileSizeLimit");
            var targetFilePath = Configuration.GetValue<string>("UsersUpload:UploadFilesPath") + "\\" + moduleName
                + (string.IsNullOrEmpty(id) ? "" : "\\" + id)
                + (string.IsNullOrEmpty(fieldName) ? "" : "\\" + fieldName);
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = formFile.FileName;
            }
            filePath = Path.Combine(targetFilePath, fileName);
            bool exists = Directory.Exists(targetFilePath);
            if (!exists)
                Directory.CreateDirectory(targetFilePath);
            (await FileHelper.ProcessFormFile<TUploadModel, string>(formFile!,
                permittedExtensions,
                fileSizeLimit,
                cancellationToken: new CancellationToken(),
                f: s =>
                {
                    byte[] bytes = s.ToArray();
                    using (var fileStream = System.IO.File.Create(filePath))
                        fileStream.Write(bytes);
                    return filePath;
                })).Match(Succ: succ =>
                    {
                        Logger.LogInformation("Successfully uploaded the file: {File}", succ);
                        filePath = succ;
                    },
                    Fail: errors =>
                    {
                        errors.Iter(error => ModelState.AddModelError("", error.ToString()));
                        Logger.LogError("Error encountered. Errors: {Errors}", errors.Join().ToString());
                        NotyfService.Error($"Error encountered. Errors: {errors.Join()}");
                        filePath = "";
                    });

        }
        return filePath;
    }
    protected async Task<IActionResult> BatchUploadAsync<PageModel, EntityState>(IFormFile? batchUploadForm, string subFolder, string pageName)
    {
        if (batchUploadForm == null)
        {
            NotyfService.Error(Localizer["Please select a file to be uploaded."]);
            return Page();
        }
        try
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(batchUploadForm.FileName);
            var filePath = await UploadFile<PageModel>(WebConstants.BatchUpload, subFolder, "", batchUploadForm, fileName);
            if (!string.IsNullOrEmpty(filePath))
            {
                _ = await Mediatr.Send(new UploadProcessorCommand { FilePath = filePath, FileType = Core.Constants.FileType.Excel, Module = typeof(EntityState).Name, UploadType = Core.Constants.UploadProcessingType.PerFile });
                NotyfService.Success(Localizer["Successfully uploaded. Please wait for the file to be processed."]);
            }         
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception encountered");
            NotyfService.Error(Localizer["Something went wrong. Please contact the system administrator."]);
            return Page();
        }
        return RedirectToPage(pageName);
    }
	protected void AddModelError(Seq<Error> errors, bool notifyError = false)
	{
		var errorMessage = "";
		foreach (var error in errors)
		{
			ModelState.AddModelError("", error.ToString());
			errorMessage += error.ToString();
		}
		Logger.LogError("Errors: {Errors}", errorMessage);
		if (notifyError)
		{
			NotyfService.Error(errorMessage);
		}
	}
}

public class BasePageModel<TContext, TPageModel> : BasePageModel<TPageModel>
    where TPageModel : class
    where TContext : DbContext
{
    TContext? _context;
    protected TContext Context => _context ??= HttpContext.RequestServices.GetService<TContext>()!;

    /// <summary>
    /// Create a <see cref="SelectList"/> based on <typeparamref name="TEntity"/>.
    /// An instance of <typeparamref name="TEntity"/> whose Id is equal to the provided <paramref name="id"/> will be retrieved from the context
    /// and will be set as the selected option.
    /// </summary>
    /// <typeparam name="TEntity">An instance of <see cref="BaseEntity"/></typeparam>
    /// <param name="id">The Id of <typeparamref name="TEntity"/> that will be retrieved from the context</param>
    /// <param name="defaultItem">The function to map <typeparamref name="TEntity"/> to <see cref="SelectListItem"/></param>
    /// <returns></returns>
    protected async Task<SelectList> CreateDefaultOption<TEntity>(string id, Func<TEntity, SelectListItem> defaultItem)
        where TEntity : BaseEntity =>
        await Context.GetSingle<TEntity>(e => e.Id == id)
                     .MatchAsync(Some: e => Task.FromResult(new SelectList(new List<SelectListItem> { defaultItem(e) }, "Value", "Text", e.Id)),
                                 None: () => new SelectList(new List<SelectListItem>(), "Value", "Text"));
}

