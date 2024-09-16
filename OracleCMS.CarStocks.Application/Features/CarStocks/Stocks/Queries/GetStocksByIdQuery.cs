using OracleCMS.Common.Core.Queries;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Stocks.Queries;

public record GetStocksByIdQuery(string Id) : BaseQueryById(Id), IRequest<Option<StocksState>>;

public class GetStocksByIdQueryHandler(ApplicationContext context) : BaseQueryByIdHandler<ApplicationContext, StocksState, GetStocksByIdQuery>(context), IRequestHandler<GetStocksByIdQuery, Option<StocksState>>
{
	
	public override async Task<Option<StocksState>> Handle(GetStocksByIdQuery request, CancellationToken cancellationToken = default)
	{
		return await Context.Stocks.Include(l=>l.Cars).Include(l=>l.Dealers)
			.Where(e => e.Id == request.Id).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
	}
	
}
