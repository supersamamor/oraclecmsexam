using OracleCMS.Common.Web.Utility.Extensions;
using OracleCMS.CarStocks.Web.Areas.Admin.Models;
using OracleCMS.CarStocks.Web.Areas.Admin.Queries.Users;
using OracleCMS.CarStocks.Infrastructure.Data;
using OracleCMS.CarStocks.Web.Models;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using OracleCMS.CarStocks.Core.Identity;

namespace OracleCMS.CarStocks.Web.Areas.Admin.Pages.Users;

[Authorize(Policy = Permission.Users.View)]
public class ViewModel : BasePageModel<ViewModel>
{
    readonly IdentityContext _context;
    readonly RoleManager<ApplicationRole> _roleManager;
    readonly UserManager<ApplicationUser> _userManager;

    public ViewModel(IdentityContext context, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }
    [BindProperty]
    public UserViewModel UserModel { get; set; } = new() { IsView = true };
    public async Task<IActionResult> OnGet(string id)
    {
        if (id == null)
        {
            return NotFound();
        }
        return await Mediatr.Send(new GetUserByIdQuery(id))
                             .ToActionResult(async user =>
                             {
                                 UserModel = await GetViewModel(user);
                                 UserModel.Roles = await GetRolesForUser(user);
                                 return Page();
                             }, none: null);
    }

    async Task<UserViewModel> GetViewModel(ApplicationUser user) =>
        await _context.GetEntityName(user.EntityId!).Match(
            entity => new UserViewModel
            {
                Id = user.Id,
                Name = user.Name ?? "",                
                Email = user.Email,
                Entity = entity,
                IsActive = user.IsActive,
                IsView = true,
            },
            () => new UserViewModel
            {
                Id = user.Id,
                Name = user.Name ?? "",               
                Email = user.Email,
                Entity = Core.Constants.Entities.Default,
                IsActive = user.IsActive,
                IsView = true,
            });

    async Task<IList<UserRoleViewModel>> GetRolesForUser(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        return _roleManager.Roles.Map(r => new UserRoleViewModel
        {
            Id = r.Id,
            Name = r.Name,
            Selected = userRoles.Any(c => c == r.Name)
        }).ToList();
    }
}
