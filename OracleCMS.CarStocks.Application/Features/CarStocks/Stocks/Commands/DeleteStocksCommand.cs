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

namespace OracleCMS.CarStocks.Application.Features.CarStocks.Stocks.Commands;

public record DeleteStocksCommand : BaseCommand, IRequest<Validation<Error, StocksState>>;

public class DeleteStocksCommandHandler(ApplicationContext context,
                                   IMapper mapper,
                                   CompositeValidator<DeleteStocksCommand> validator) : BaseCommandHandler<ApplicationContext, StocksState, DeleteStocksCommand>(context, mapper, validator), IRequestHandler<DeleteStocksCommand, Validation<Error, StocksState>>
{ 
    public async Task<Validation<Error, StocksState>> Handle(DeleteStocksCommand request, CancellationToken cancellationToken) =>
        await Validators.ValidateTAsync(request, cancellationToken).BindT(
            async request => await Delete(request.Id, cancellationToken));
}


public class DeleteStocksCommandValidator : AbstractValidator<DeleteStocksCommand>
{
    readonly ApplicationContext _context;

    public DeleteStocksCommandValidator(ApplicationContext context)
    {
        _context = context;
        RuleFor(x => x.Id).MustAsync(async (id, cancellation) => await _context.Exists<StocksState>(x => x.Id == id, cancellationToken: cancellation))
                          .WithMessage("Stocks with id {PropertyValue} does not exists");
    }
}
