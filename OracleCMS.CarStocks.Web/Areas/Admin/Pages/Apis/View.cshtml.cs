using OracleCMS.Common.Web.Utility.Extensions;
using OracleCMS.CarStocks.Web.Areas.Admin.Models;
using OracleCMS.CarStocks.Web.Models;
using OracleCMS.CarStocks.Core.Oidc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using static LanguageExt.Prelude;

namespace OracleCMS.CarStocks.Web.Areas.Admin.Pages.Apis;

[Authorize(Policy = Permission.Apis.View)]
public class ViewModel : BasePageModel<ViewModel>
{
    private readonly OpenIddictScopeManager<OidcScope> _manager;

    public ViewModel(OpenIddictScopeManager<OidcScope> manager)
    {
        _manager = manager;
    }

    public ScopeViewModel Scope { get; set; } = new();

    public async Task<IActionResult> OnGet(string? name)
    {
        if (name == null)
        {
            return NotFound();
        }
        return await Optional(await _manager.FindByNameAsync(name))
            .ToActionResult(async scope =>
            {
                var descriptor = new OpenIddictScopeDescriptor();
                await _manager.PopulateAsync(descriptor, scope!);
                Scope = new()
                {
                    Name = descriptor.Name ?? "",
                    DisplayName = descriptor.DisplayName ?? "",
                    Resources = string.Join(" ", descriptor.Resources)
                };
                return Page();
            }, none: null);
    }
}
