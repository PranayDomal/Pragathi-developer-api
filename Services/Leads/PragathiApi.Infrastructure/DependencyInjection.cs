using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PragathiApi.Application.Interfaces;
using PragathiApi.Infrastructure.Persistence;
using PragathiApi.Infrastructure.Repositories;

namespace PragathiApi.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the database and everything that talks to it.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDb>(o => o.UseNpgsql(connectionString));

        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IRoomConfigurationRepository, RoomConfigurationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAdminRepository, AdminRepository>();

        return services;
    }
}