using OracleCMS.Common.Core.Queries;
using OracleCMS.Common.Utility.Models;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using MediatR;
using OracleCMS.Common.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using OracleCMS.CarStocks.Application.DTOs;
using LanguageExt;

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Queries;

public record GetCarsQuery : BaseQuery, IRequest<PagedListResponse<CarsListDto>>;

public class GetCarsQueryHandler(ApplicationContext context) : BaseQueryHandler<ApplicationContext, CarsListDto, GetCarsQuery>(context), IRequestHandler<GetCarsQuery, PagedListResponse<CarsListDto>>
{
	public override Task<PagedListResponse<CarsListDto>> Handle(GetCarsQuery request, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(Context.Set<CarsState>()
            .AsNoTracking().Select(e => new CarsListDto()
            {
                Id = e.Id,
                LastModifiedDate = e.LastModifiedDate,
                Make = e.Make,
                Model = e.Model,
                Year = e.Year,
            })
            .ToPagedResponse(request.SearchColumns, request.SearchValue,
                request.SortColumn, request.SortOrder,
                request.PageNumber, request.PageSize));
	}	
}
