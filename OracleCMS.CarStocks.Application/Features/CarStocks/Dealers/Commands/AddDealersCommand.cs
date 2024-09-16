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

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Dealers.Commands;

public record AddDealersCommand : DealersState, IRequest<Validation<Error, DealersState>>;

public class AddDealersCommandHandler(ApplicationContext context,
                                IMapper mapper,
                                CompositeValidator<AddDealersCommand> validator,
                                    IdentityContext identityContext) : BaseCommandHandler<ApplicationContext, DealersState, AddDealersCommand>(context, mapper, validator), IRequestHandler<AddDealersCommand, Validation<Error, DealersState>>
{
    
public async Task<Validation<Error, DealersState>> Handle(AddDealersCommand request, CancellationToken cancellationToken) =>
		await Validators.ValidateTAsync(request, cancellationToken).BindT(
			async request => await Add(request, cancellationToken));
	
}

public class AddDealersCommandValidator : AbstractValidator<AddDealersCommand>
{
    readonly ApplicationContext _context;

    public AddDealersCommandValidator(ApplicationContext context)
    {
        _context = context;

        RuleFor(x => x.Id).MustAsync(async (id, cancellation) => await _context.NotExists<DealersState>(x => x.Id == id, cancellationToken: cancellation))
                          .WithMessage("Dealers with id {PropertyValue} already exists");
        
    }
}
