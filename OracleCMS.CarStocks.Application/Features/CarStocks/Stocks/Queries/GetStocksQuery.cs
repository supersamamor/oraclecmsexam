using OracleCMS.Common.Core.Queries;
using OracleCMS.Common.Utility.Models;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using MediatR;
using OracleCMS.Common.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using OracleCMS.CarStocks.Application.DTOs;
using LanguageExt;
using System.ComponentModel.DataAnnotations;
using OracleCMS.Common.Identity.Abstractions;

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Stocks.Queries;

public record GetStocksQuery : BaseQuery, IRequest<PagedListResponse<StocksListDto>>
{
    [Required]
    public string DealerId { get; set; } = "";
}

public class GetStocksQueryHandler(ApplicationContext context, IdentityContext identityContext, IAuthenticatedUser authenticatedUser) : BaseQueryHandler<ApplicationContext, StocksListDto, GetStocksQuery>(context), IRequestHandler<GetStocksQuery, PagedListResponse<StocksListDto>>
{
    public override async Task<PagedListResponse<StocksListDto>> Handle(GetStocksQuery request, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<StocksState>()
            .Include(l => l.Cars).Include(l => l.Dealers).AsNoTracking();
        var listOfRoleIds = await identityContext.UserRoles.Where(l => l.UserId == authenticatedUser.UserId)
            .Select(l => l.RoleId).ToListAsync(cancellationToken);
        var listOfRoles = await identityContext.Roles.Where(l => listOfRoleIds.Contains(l.Id)).Select(l => l.Name)
            .ToListAsync(cancellationToken);
        if (!listOfRoles.Contains(Core.Constants.Roles.Admin))
        {
            query = query.Where(l => l.DealerID == request.DealerId);
        }
        return query.Select(e => new StocksListDto()
        {
            Id = e.Id,
            LastModifiedDate = e.LastModifiedDate,
            CarMake = e.Cars == null ? "" : e.Cars!.Make,
            CarModel = e.Cars == null ? "" : e.Cars!.Model,
            DealerName = e.Dealers == null ? "" : e.Dealers!.DealerName,
            Quantity = e.Quantity,
        })
        .ToPagedResponse(request.SearchColumns, request.SearchValue,
                    request.SortColumn, request.SortOrder,
                    request.PageNumber, request.PageSize);
    }
}
