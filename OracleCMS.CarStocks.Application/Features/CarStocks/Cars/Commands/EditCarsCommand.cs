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

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Commands;

public record EditCarsCommand : CarsState, IRequest<Validation<Error, CarsState>>;

public class EditCarsCommandHandler(ApplicationContext context,
                                 IMapper mapper,
                                 CompositeValidator<EditCarsCommand> validator) : BaseCommandHandler<ApplicationContext, CarsState, EditCarsCommand>(context, mapper, validator), IRequestHandler<EditCarsCommand, Validation<Error, CarsState>>
{ 
    
public async Task<Validation<Error, CarsState>> Handle(EditCarsCommand request, CancellationToken cancellationToken) =>
		await Validators.ValidateTAsync(request, cancellationToken).BindT(
			async request => await Edit(request, cancellationToken));
	
}

public class EditCarsCommandValidator : AbstractValidator<EditCarsCommand>
{
    readonly ApplicationContext _context;

    public EditCarsCommandValidator(ApplicationContext context)
    {
        _context = context;
		RuleFor(x => x.Id).MustAsync(async (id, cancellation) => await _context.Exists<CarsState>(x => x.Id == id, cancellationToken: cancellation))
                          .WithMessage("Cars with id {PropertyValue} does not exists");
        
    }
}
