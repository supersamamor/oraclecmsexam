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

public record AddCarsCommand : CarsState, IRequest<Validation<Error, CarsState>>;

public class AddCarsCommandHandler(ApplicationContext context,
                                IMapper mapper,
                                CompositeValidator<AddCarsCommand> validator,
                                    IdentityContext identityContext) : BaseCommandHandler<ApplicationContext, CarsState, AddCarsCommand>(context, mapper, validator), IRequestHandler<AddCarsCommand, Validation<Error, CarsState>>
{
    public async Task<Validation<Error, CarsState>> Handle(AddCarsCommand request, CancellationToken cancellationToken) =>
		await Validators.ValidateTAsync(request, cancellationToken).BindT(
			async request => await AddCars(request, cancellationToken));


	public async Task<Validation<Error, CarsState>> AddCars(AddCarsCommand request, CancellationToken cancellationToken)
	{
		CarsState entity = Mapper.Map<CarsState>(request);
		_ = await Context.AddAsync(entity, cancellationToken);
		await Helpers.ApprovalHelper.AddApprovers(Context, identityContext, ApprovalModule.Cars, entity.Id, cancellationToken);
		_ = await Context.SaveChangesAsync(cancellationToken);
		return Success<Error, CarsState>(entity);
	}
	
}

public class AddCarsCommandValidator : AbstractValidator<AddCarsCommand>
{
    readonly ApplicationContext _context;

    public AddCarsCommandValidator(ApplicationContext context)
    {
        _context = context;

        RuleFor(x => x.Id).MustAsync(async (id, cancellation) => await _context.NotExists<CarsState>(x => x.Id == id, cancellationToken: cancellation))
                          .WithMessage("Cars with id {PropertyValue} already exists");
        
    }
}
