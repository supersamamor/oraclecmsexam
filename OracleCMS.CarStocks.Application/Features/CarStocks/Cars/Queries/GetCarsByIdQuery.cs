using OracleCMS.Common.Core.Queries;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Queries;

public record GetCarsByIdQuery(string Id) : BaseQueryById(Id), IRequest<Option<CarsState>>;

public class GetCarsByIdQueryHandler(ApplicationContext context) : BaseQueryByIdHandler<ApplicationContext, CarsState, GetCarsByIdQuery>(context), IRequestHandler<GetCarsByIdQuery, Option<CarsState>>
{
		
}
