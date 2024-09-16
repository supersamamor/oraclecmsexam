using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using OracleCMS.Common.Core.Queries;
using OracleCMS.Common.Utility.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OracleCMS.Common.Utility.Extensions;

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Approval.Queries;

public record GetApproverSetupQuery : BaseQuery, IRequest<PagedListResponse<ApproverSetupState>>;

public class GetApproverSetupQueryHandler : BaseQueryHandler<ApplicationContext, ApproverSetupState, GetApproverSetupQuery>, IRequestHandler<GetApproverSetupQuery, PagedListResponse<ApproverSetupState>>
{
    public GetApproverSetupQueryHandler(ApplicationContext context) : base(context)
    {
    }
    public override Task<PagedListResponse<ApproverSetupState>> Handle(GetApproverSetupQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Context.ApproverSetup.Where(l=>l.ApprovalSetupType == ApprovalSetupTypes.Modular).AsNoTracking().ToPagedResponse(request.SearchColumns, request.SearchValue,
                                                                 request.SortColumn, request.SortOrder,
                                                                 request.PageNumber, request.PageSize));
    }

}
