using OracleCMS.Common.Core.Queries;
using OracleCMS.Common.Utility.Models;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using MediatR;
using OracleCMS.Common.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using OracleCMS.CarStocks.Application.DTOs;
using LanguageExt;

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Dealers.Queries;

public record GetDealersQuery : BaseQuery, IRequest<PagedListResponse<DealersListDto>>;

public class GetDealersQueryHandler(ApplicationContext context) : BaseQueryHandler<ApplicationContext, DealersListDto, GetDealersQuery>(context), IRequestHandler<GetDealersQuery, PagedListResponse<DealersListDto>>
{
	public override Task<PagedListResponse<DealersListDto>> Handle(GetDealersQuery request, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(Context.Set<DealersState>()
			.AsNoTracking().Select(e => new DealersListDto()
			{
				Id = e.Id,
				LastModifiedDate = e.LastModifiedDate,
				DealerName = e.DealerName,
				DealerWebsite = e.DealerWebsite,
			})
			.ToPagedResponse(request.SearchColumns, request.SearchValue,
				request.SortColumn, request.SortOrder,
				request.PageNumber, request.PageSize));
	}	
}
