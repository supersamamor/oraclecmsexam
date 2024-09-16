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

public record EditStocksCommand : StocksState, IRequest<Validation<Error, StocksState>>;

public class EditStocksCommandHandler(ApplicationContext context,
                                 IMapper mapper,
                                 CompositeValidator<EditStocksCommand> validator) : BaseCommandHandler<ApplicationContext, StocksState, EditStocksCommand>(context, mapper, validator), IRequestHandler<EditStocksCommand, Validation<Error, StocksState>>
{ 
    
public async Task<Validation<Error, StocksState>> Handle(EditStocksCommand request, CancellationToken cancellationToken) =>
		await Validators.ValidateTAsync(request, cancellationToken).BindT(
			async request => await Edit(request, cancellationToken));
	
}

public class EditStocksCommandValidator : AbstractValidator<EditStocksCommand>
{
    readonly ApplicationContext _context;

    public EditStocksCommandValidator(ApplicationContext context)
    {
        _context = context;
		RuleFor(x => x.Id).MustAsync(async (id, cancellation) => await _context.Exists<StocksState>(x => x.Id == id, cancellationToken: cancellation))
                          .WithMessage("Stocks with id {PropertyValue} does not exists");
        
    }
}
