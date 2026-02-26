using CorporateBrain.Application.Common.Interfaces;
using CorporateBrain.Infrastructure.Authentication;
using CorporateBrain.Infrastructure.Persistence;
using CorporateBrain.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CorporateBrain.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Register the Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // 2. Register Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // 3. Register Unit of Work (Interface matches Implementation)
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Register the JWT Provider
        services.AddSingleton<IJwtProvider, JwtProvider>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddSingleton<IAiChatServices, SemanticKernelServices>();

        return services;
    }

}
