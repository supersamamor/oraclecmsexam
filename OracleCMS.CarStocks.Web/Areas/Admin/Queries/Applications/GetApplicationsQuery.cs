using OracleCMS.Common.Utility.Extensions;
using OracleCMS.Common.Utility.Models;
using OracleCMS.CarStocks.Infrastructure.Data;
using OracleCMS.CarStocks.Core.Oidc;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OracleCMS.Common.Core.Queries;

namespace OracleCMS.CarStocks.Web.Areas.Admin.Queries.Applications;

public record GetApplicationsQuery : BaseQuery, IRequest<PagedListResponse<OidcApplication>>
{
}

public class GetApplicationsQueryHandler : IRequestHandler<GetApplicationsQuery, PagedListResponse<OidcApplication>>
{
    private readonly IdentityContext _context;

    public GetApplicationsQueryHandler(IdentityContext context)
    {
        _context = context;
    }

    public Task<PagedListResponse<OidcApplication>> Handle(GetApplicationsQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(_context.Set<OidcApplication>()
                      .AsNoTracking()
                      .ToPagedResponse(request.SearchColumns, request.SearchValue, request.SortColumn,
                                       request.SortOrder, request.PageNumber, request.PageSize));
}
