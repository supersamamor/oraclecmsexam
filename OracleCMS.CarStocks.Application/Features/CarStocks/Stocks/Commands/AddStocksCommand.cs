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

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Stocks.Commands;

public record AddStocksCommand : StocksState, IRequest<Validation<Error, StocksState>>;

public class AddStocksCommandHandler(ApplicationContext context,
                                IMapper mapper,
                                CompositeValidator<AddStocksCommand> validator,
                                    IdentityContext identityContext) : BaseCommandHandler<ApplicationContext, StocksState, AddStocksCommand>(context, mapper, validator), IRequestHandler<AddStocksCommand, Validation<Error, StocksState>>
{
    
public async Task<Validation<Error, StocksState>> Handle(AddStocksCommand request, CancellationToken cancellationToken) =>
		await Validators.ValidateTAsync(request, cancellationToken).BindT(
			async request => await Add(request, cancellationToken));
	
}

public class AddStocksCommandValidator : AbstractValidator<AddStocksCommand>
{
    readonly ApplicationContext _context;

    public AddStocksCommandValidator(ApplicationContext context)
    {
        _context = context;

        RuleFor(x => x.Id).MustAsync(async (id, cancellation) => await _context.NotExists<StocksState>(x => x.Id == id, cancellationToken: cancellation))
                          .WithMessage("Stocks with id {PropertyValue} already exists");
        
    }
}
