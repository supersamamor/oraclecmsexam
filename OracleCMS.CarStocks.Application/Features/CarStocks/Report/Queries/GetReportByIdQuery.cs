using OracleCMS.Common.Core.Queries;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Report.Queries;

public record GetReportByIdQuery(string Id) : BaseQueryById(Id), IRequest<Option<ReportState>>;

public class GetReportByIdQueryHandler : BaseQueryByIdHandler<ApplicationContext, ReportState, GetReportByIdQuery>, IRequestHandler<GetReportByIdQuery, Option<ReportState>>
{
    public GetReportByIdQueryHandler(ApplicationContext context) : base(context)
    {
    }

    public override async Task<Option<ReportState>> Handle(GetReportByIdQuery request, CancellationToken cancellationToken = default)
    {
        return await Context.Report         
            .Include(l => l.ReportQueryFilterList)
            .Include(l => l.ReportRoleAssignmentList)
            .Where(e => e.Id == request.Id).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

}
