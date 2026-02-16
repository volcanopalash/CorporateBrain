using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
namespace CorporateBrain.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FIX: Don't use GetExecutingAssembly(). 
        // Point to a class that lives in the Application layer (like CreateUserDto).

        //var applicationAssembly = typeof(CreateUserDto).Assembly;

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
