using OracleCMS.Common.Utility.Extensions;
using OracleCMS.Common.Utility.Models;
using OracleCMS.CarStocks.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OracleCMS.Common.Core.Queries;
using OracleCMS.CarStocks.Core.CarStocks;

namespace OracleCMS.CarStocks.Web.Areas.Admin.Queries.Entities;

public record GetBatchUploadJobsQuery : BaseQuery, IRequest<PagedListResponse<UploadProcessorState>>
{
}

public class GetBatchUploadJobsQueryHandler(ApplicationContext context) : IRequestHandler<GetBatchUploadJobsQuery, PagedListResponse<UploadProcessorState>>
{

    public Task<PagedListResponse<UploadProcessorState>> Handle(GetBatchUploadJobsQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(context.UploadProcessor.AsNoTracking().ToPagedResponse(request.SearchColumns, request.SearchValue,
                                                               request.SortColumn, request.SortOrder,
                                                               request.PageNumber, request.PageSize));
}
