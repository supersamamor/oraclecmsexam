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

public record EditDealersCommand : DealersState, IRequest<Validation<Error, DealersState>>;

public class EditDealersCommandHandler(ApplicationContext context,
                                 IMapper mapper,
                                 CompositeValidator<EditDealersCommand> validator) : BaseCommandHandler<ApplicationContext, DealersState, EditDealersCommand>(context, mapper, validator), IRequestHandler<EditDealersCommand, Validation<Error, DealersState>>
{ 
    
public async Task<Validation<Error, DealersState>> Handle(EditDealersCommand request, CancellationToken cancellationToken) =>
		await Validators.ValidateTAsync(request, cancellationToken).BindT(
			async request => await Edit(request, cancellationToken));
	
}

public class EditDealersCommandValidator : AbstractValidator<EditDealersCommand>
{
    readonly ApplicationContext _context;

    public EditDealersCommandValidator(ApplicationContext context)
    {
        _context = context;
		RuleFor(x => x.Id).MustAsync(async (id, cancellation) => await _context.Exists<DealersState>(x => x.Id == id, cancellationToken: cancellation))
                          .WithMessage("Dealers with id {PropertyValue} does not exists");
        
    }
}
