using AutoMapper;
using OracleCMS.Common.Core.Commands;
using OracleCMS.Common.Data;
using OracleCMS.Common.Utility.Validators;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using FluentValidation;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Report.Commands;

public record DeleteReportCommand : BaseCommand, IRequest<Validation<Error, ReportState>>;

public class DeleteReportCommandHandler : BaseCommandHandler<ApplicationContext, ReportState, DeleteReportCommand>, IRequestHandler<DeleteReportCommand, Validation<Error, ReportState>>
{
    public DeleteReportCommandHandler(ApplicationContext context,
                                       IMapper mapper,
                                       CompositeValidator<DeleteReportCommand> validator) : base(context, mapper, validator)
    {
    }

    public async Task<Validation<Error, ReportState>> Handle(DeleteReportCommand request, CancellationToken cancellationToken) =>
        await Validators.ValidateTAsync(request, cancellationToken).BindT(
            async request => await DeleteReport(request.Id, cancellationToken));

    public async Task<Validation<Error, ReportState>> DeleteReport(string id, CancellationToken cancellationToken)
    {
        var reportAIIntegrationList = await Context.ReportAIIntegration.Where(l => l.ReportId == id).ToListAsync(cancellationToken: cancellationToken);
        Context.RemoveRange(reportAIIntegrationList);
        var reportQueryFilterList = await Context.ReportQueryFilter.Where(l => l.ReportId == id).ToListAsync(cancellationToken: cancellationToken);
        Context.RemoveRange(reportQueryFilterList);
        var reportRoleAssignmentList = await Context.ReportRoleAssignment.Where(l => l.ReportId == id).ToListAsync(cancellationToken: cancellationToken);
        Context.RemoveRange(reportRoleAssignmentList);
        var entity = await Context.Report.Where(l => l.Id == id).SingleAsync(cancellationToken: cancellationToken);
        Context.Remove(entity);
        _ = await Context.SaveChangesAsync(cancellationToken);
        return Success<Error, ReportState>(entity);
    }
}


public class DeleteReportCommandValidator : AbstractValidator<DeleteReportCommand>
{
    readonly ApplicationContext _context;

    public DeleteReportCommandValidator(ApplicationContext context)
    {
        _context = context;
        RuleFor(x => x.Id).MustAsync(async (id, cancellation) => await _context.Exists<ReportState>(x => x.Id == id, cancellationToken: cancellation))
                          .WithMessage("Report with id {PropertyValue} does not exists");
    }
}
