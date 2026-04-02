using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeDiscipline.Application.Interfaces;
using PrimeDiscipline.Domain.Interfaces;
using PrimeDiscipline.Infrastructure.BackgroundServices;
using PrimeDiscipline.Infrastructure.Persistence;
using PrimeDiscipline.Infrastructure.Persistence.Repositories;
using PrimeDiscipline.Infrastructure.Persistence.Settings;
using PrimeDiscipline.Infrastructure.Security;

namespace PrimeDiscipline.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<MongoDbContext>();

        services.AddScoped<IUserRepository,     UserRepository>();
        services.AddScoped<IHabitRepository,    HabitRepository>();
        services.AddScoped<IHabitLogRepository, HabitLogRepository>();
        services.AddScoped<ISessionRepository,  SessionRepository>();

        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();

        services.AddHostedService<HabitWindowWorker>();

        return services;
    }
}
