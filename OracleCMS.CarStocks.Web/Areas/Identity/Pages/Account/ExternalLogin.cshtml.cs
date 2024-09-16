using OracleCMS.Common.Services.Shared.Interfaces;
using OracleCMS.CarStocks.Web.Areas.Admin.Commands.AuditTrail;
using OracleCMS.CarStocks.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using OracleCMS.CarStocks.Core.Identity;
using OracleCMS.CarStocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace OracleCMS.CarStocks.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel : BasePageModel<ExternalLoginModel>
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMailService _emailSender;
    private readonly ILogger<ExternalLoginModel> _logger;
    private readonly IMediator _mediator;
    private readonly IdentityContext _context;
    public ExternalLoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<ExternalLoginModel> logger,
        IMailService emailSender,
        IMediator mediator,
        IdentityContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _emailSender = emailSender;
        _mediator = mediator;
        _context = context;
    }

    [BindProperty]
    public InputModel? Input { get; set; }

    public string? ProviderDisplayName { get; set; }

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Full name")]
        public string? Name { get; set; }
		
        [Range(typeof(bool), "true", "true", ErrorMessage = "Please agree to the terms")]
        public bool TermsAccepted { get; set; }
    }

    public IActionResult OnGetAsync()
    {
        return RedirectToPage("./Login");
    }

    public IActionResult OnPost(string provider, string? returnUrl = null)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        if (remoteError != null)
        {
            ErrorMessage = $"Error from external provider: {remoteError}";
            _logger.LogError("Error from external provider: {RemoteError}", remoteError);
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information.";
            _logger.LogError("Error loading external login information.");
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName = info.Principal.FindFirstValue(ClaimTypes.Name);
        var user = await _userManager.FindByNameAsync(email);
        if (user != null && !user.IsActive)
        {
            await _mediator.Send(new AddAuditLogCommand() { UserId = user.Id, Type = "User is not active", TraceId = TraceId });
            _logger.LogWarning("User is not active, Email = {Email}", email);
            return RedirectToPage("./NotActive");
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            return await SuccessRedirect(email, user!, info, returnUrl);
        }
        if (result.IsLockedOut)
        {
            await _mediator.Send(new AddAuditLogCommand() { UserId = user!.Id, Type = "User account locked out", TraceId = TraceId });
            Logger.LogWarning("User account locked out, Email = {Email}", Input!.Email);
            return RedirectToPage("./Lockout");
        }
        else
        {
            var defaultEntity = _context.Entities.Where(l => l.Name == Core.Constants.Entities.Default).AsNoTracking().FirstOrDefault();
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Name = fullName,
                EntityId = defaultEntity!.Id,
                IsActive = true,
                EmailConfirmed = true,
            };
            var createUserResult = await _userManager.CreateAsync(user);
            _ = await _userManager.AddToRoleAsync(user, Core.Constants.Roles.User);
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            return await SuccessRedirect(email, user!, info, returnUrl);
        }
    }
	private async Task<IActionResult> SuccessRedirect(string email, ApplicationUser user, ExternalLoginInfo? info, string? returnUrl = null)
    {
        await _mediator.Send(new AddAuditLogCommand() { UserId = user!.Id, Type = "User logged in", TraceId = TraceId });
        _logger.LogInformation("User logged in, Email = {Email}, Provider = {LoginProvider}", email, info?.LoginProvider);
        NotyfService.Success($"Logged in as {email}");
        return LocalRedirect(returnUrl);
    }
    public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        // Get the information about the user from the external login provider
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information during confirmation.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        if (ModelState.IsValid)
        {
            var defaultEntity = _context.Entities.Where(l => l.Name == Core.Constants.Entities.Default).AsNoTracking().FirstOrDefault();
            var user = new ApplicationUser
            {
                UserName = Input?.Email,
                Email = Input?.Email,
                Name = Input?.Name,               
                EntityId = defaultEntity!.Id
            };

            var result = await _userManager.CreateAsync(user);
            _ = await _userManager.AddToRoleAsync(user, Core.Constants.Roles.User);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code },
                        protocol: Request.Scheme);

                    await _emailSender.SendAsync(new()
                    {
                        To = Input!.Email!,
                        Subject = "Confirm your email",
                        Body = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>."
                    });

                    // If account confirmation is required, we need to show the link if we don't have a real email sender
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("./RegisterConfirmation", new { Input.Email });
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ProviderDisplayName = info.ProviderDisplayName;
        ReturnUrl = returnUrl;
        return Page();
    }
}
