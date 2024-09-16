using AutoMapper;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using OracleCMS.Common.Core.Commands;
using OracleCMS.Common.Data;
using OracleCMS.Common.Utility.Validators;
using FluentValidation;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Approval.Commands;

public record AddApproverSetupCommand : ApproverSetupState, IRequest<Validation<Error, ApproverSetupState>>;

public class AddApproverSetupCommandHandler : BaseCommandHandler<ApplicationContext, ApproverSetupState, AddApproverSetupCommand>, IRequestHandler<AddApproverSetupCommand, Validation<Error, ApproverSetupState>>
{
    public AddApproverSetupCommandHandler(ApplicationContext context,
                                    IMapper mapper,
                                    CompositeValidator<AddApproverSetupCommand> validator) : base(context, mapper, validator)
    {
    }

    public async Task<Validation<Error, ApproverSetupState>> Handle(AddApproverSetupCommand request, CancellationToken cancellationToken) =>
        await Validators.ValidateTAsync(request, cancellationToken).BindT(
            async request => await AddApproverSetup(request, cancellationToken));


    public async Task<Validation<Error, ApproverSetupState>> AddApproverSetup(AddApproverSetupCommand request, CancellationToken cancellationToken)
    {
        ApproverSetupState entity = Mapper.Map<ApproverSetupState>(request);
        UpdateApproverAssignmentList(entity);
        _ = await Context.AddAsync(entity, cancellationToken);
        _ = await Context.SaveChangesAsync(cancellationToken);
        return Success<Error, ApproverSetupState>(entity);
    }

    private void UpdateApproverAssignmentList(ApproverSetupState entity)
    {
        if (entity.ApproverAssignmentList?.Count > 0)
        {
            foreach (var approverAssignment in entity.ApproverAssignmentList!)
            {
                Context.Entry(approverAssignment).State = EntityState.Added;
            }
        }
    }

}

public class AddApproverSetupCommandValidator : AbstractValidator<AddApproverSetupCommand>
{
    readonly ApplicationContext _context;

    public AddApproverSetupCommandValidator(ApplicationContext context)
    {
        _context = context;

        RuleFor(x => x.Id).MustAsync(async (id, cancellation) => await _context.NotExists<ApproverSetupState>(x => x.Id == id, cancellationToken: cancellation))
                          .WithMessage("ApproverSetup with id {PropertyValue} already exists");
        RuleFor(x => x.TableName).MustAsync(async (tableName, cancellation) => await _context.NotExists<ApproverSetupState>(x => x.TableName == tableName, cancellationToken: cancellation)).WithMessage("ApproverSetup with tableName {PropertyValue} already exists");

    }
}
