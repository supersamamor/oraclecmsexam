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

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Dealers.Commands;

public record DeleteDealersCommand : BaseCommand, IRequest<Validation<Error, DealersState>>;

public class DeleteDealersCommandHandler(ApplicationContext context,
                                   IMapper mapper,
                                   CompositeValidator<DeleteDealersCommand> validator) : BaseCommandHandler<ApplicationContext, DealersState, DeleteDealersCommand>(context, mapper, validator), IRequestHandler<DeleteDealersCommand, Validation<Error, DealersState>>
{ 
    public async Task<Validation<Error, DealersState>> Handle(DeleteDealersCommand request, CancellationToken cancellationToken) =>
        await Validators.ValidateTAsync(request, cancellationToken).BindT(
            async request => await Delete(request.Id, cancellationToken));
}


public class DeleteDealersCommandValidator : AbstractValidator<DeleteDealersCommand>
{
    readonly ApplicationContext _context;

    public DeleteDealersCommandValidator(ApplicationContext context)
    {
        _context = context;
        RuleFor(x => x.Id).MustAsync(async (id, cancellation) => await _context.Exists<DealersState>(x => x.Id == id, cancellationToken: cancellation))
                          .WithMessage("Dealers with id {PropertyValue} does not exists");
    }
}
