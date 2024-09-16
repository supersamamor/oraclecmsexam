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

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Commands;

public record DeleteCarsCommand : BaseCommand, IRequest<Validation<Error, CarsState>>;

public class DeleteCarsCommandHandler(ApplicationContext context,
                                   IMapper mapper,
                                   CompositeValidator<DeleteCarsCommand> validator) : BaseCommandHandler<ApplicationContext, CarsState, DeleteCarsCommand>(context, mapper, validator), IRequestHandler<DeleteCarsCommand, Validation<Error, CarsState>>
{ 
    public async Task<Validation<Error, CarsState>> Handle(DeleteCarsCommand request, CancellationToken cancellationToken) =>
        await Validators.ValidateTAsync(request, cancellationToken).BindT(
            async request => await Delete(request.Id, cancellationToken));
}


public class DeleteCarsCommandValidator : AbstractValidator<DeleteCarsCommand>
{
    readonly ApplicationContext _context;

    public DeleteCarsCommandValidator(ApplicationContext context)
    {
        _context = context;
        RuleFor(x => x.Id).MustAsync(async (id, cancellation) => await _context.Exists<CarsState>(x => x.Id == id, cancellationToken: cancellation))
                          .WithMessage("Cars with id {PropertyValue} does not exists");
    }
}
