using OracleCMS.Common.Core.Queries;
using OracleCMS.Common.Utility.Models;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using MediatR;
using OracleCMS.Common.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using OracleCMS.CarStocks.Application.DTOs;
using LanguageExt;

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Stocks.Queries;

public record GetStocksQuery : BaseQuery, IRequest<PagedListResponse<StocksListDto>>;

public class GetStocksQueryHandler(ApplicationContext context) : BaseQueryHandler<ApplicationContext, StocksListDto, GetStocksQuery>(context), IRequestHandler<GetStocksQuery, PagedListResponse<StocksListDto>>
{
	public override Task<PagedListResponse<StocksListDto>> Handle(GetStocksQuery request, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(Context.Set<StocksState>().Include(l=>l.Cars).Include(l=>l.Dealers)
			.AsNoTracking().Select(e => new StocksListDto()
			{
				Id = e.Id,
				LastModifiedDate = e.LastModifiedDate,
				CarID = e.Cars == null ? "" : e.Cars!.Id,
				DealerID = e.Dealers == null ? "" : e.Dealers!.Id,
				Quantity = e.Quantity,
			})
			.ToPagedResponse(request.SearchColumns, request.SearchValue,
				request.SortColumn, request.SortOrder,
				request.PageNumber, request.PageSize));
	}	
}
