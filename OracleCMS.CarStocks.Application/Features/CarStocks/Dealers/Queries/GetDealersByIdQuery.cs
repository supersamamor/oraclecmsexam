using OracleCMS.Common.Core.Queries;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Dealers.Queries;

public record GetDealersByIdQuery(string Id) : BaseQueryById(Id), IRequest<Option<DealersState>>;

public class GetDealersByIdQueryHandler(ApplicationContext context) : BaseQueryByIdHandler<ApplicationContext, DealersState, GetDealersByIdQuery>(context), IRequestHandler<GetDealersByIdQuery, Option<DealersState>>
{
		
}
