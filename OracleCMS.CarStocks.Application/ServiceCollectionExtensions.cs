using OracleCMS.Common.Utility.Validators;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace OracleCMS.CarStocks.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
		{           
			configuration.RegisterServicesFromAssemblies([Assembly.GetExecutingAssembly()]);
		});
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(CompositeValidator<>));
        return services;
    }
}
