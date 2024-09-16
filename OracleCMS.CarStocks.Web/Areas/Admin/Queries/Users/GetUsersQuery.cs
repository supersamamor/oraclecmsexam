using OracleCMS.Common.Utility.Extensions;
using OracleCMS.Common.Utility.Models;
using OracleCMS.CarStocks.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OracleCMS.Common.Core.Queries;
using OracleCMS.CarStocks.Core.Identity;

namespace OracleCMS.CarStocks.Web.Areas.Admin.Queries.Users;

public record GetUsersQuery : BaseQuery, IRequest<PagedListResponse<ApplicationUser>>
{
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedListResponse<ApplicationUser>>
{
    private readonly IdentityContext _context;

    public GetUsersQueryHandler(IdentityContext context)
    {
        _context = context;
    }

    public Task<PagedListResponse<ApplicationUser>> Handle(GetUsersQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(_context.Users.AsNoTracking().ToPagedResponse(request.SearchColumns, request.SearchValue,
                                                            request.SortColumn, request.SortOrder,
                                                            request.PageNumber, request.PageSize));
}
