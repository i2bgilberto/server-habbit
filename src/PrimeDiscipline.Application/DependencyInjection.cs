using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PrimeDiscipline.Application.Behaviors;
using PrimeDiscipline.Application.Strategies;

namespace PrimeDiscipline.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Strategies
        services.AddSingleton<StandardHabitValidationStrategy>();
        services.AddSingleton<StreakHabitValidationStrategy>();
        services.AddSingleton<FlexibleHabitValidationStrategy>();
        services.AddSingleton<HabitValidationStrategyFactory>();

        return services;
    }
}
