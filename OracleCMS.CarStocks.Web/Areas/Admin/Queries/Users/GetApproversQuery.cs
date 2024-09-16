using OracleCMS.CarStocks.Core.Identity;
using OracleCMS.CarStocks.Infrastructure.Data;
using OracleCMS.Common.Core.Queries;
using OracleCMS.Common.Utility.Extensions;
using OracleCMS.Common.Utility.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace OracleCMS.CarStocks.Web.Areas.Admin.Queries.Users;

public record GetApproversQuery(string CurrentSelectedApprover, IList<string> AllSelectedApprovers) : BaseQuery, IRequest<PagedListResponse<ApplicationUser>>
{
}

public class GetApproversQueryHandler : IRequestHandler<GetApproversQuery, PagedListResponse<ApplicationUser>>
{
    private readonly IdentityContext _context;

    public GetApproversQueryHandler(IdentityContext context)
    {
        _context = context;
    }

    public Task<PagedListResponse<ApplicationUser>> Handle(GetApproversQuery request, CancellationToken cancellationToken)
    {
        var excludedUsers = request.AllSelectedApprovers.Where(l => l != request.CurrentSelectedApprover);
        var query = _context.Users.Where(l => !excludedUsers.Contains(l.Id)).AsNoTracking();
        return Task.FromResult(query.ToPagedResponse(request.SearchColumns, request.SearchValue,
                                                       request.SortColumn, request.SortOrder,
                                                       request.PageNumber, request.PageSize));
    }
}
