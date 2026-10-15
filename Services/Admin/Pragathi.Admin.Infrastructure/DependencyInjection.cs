using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pragathi.Admin.Application.Interfaces;
using Pragathi.Admin.Infrastructure.Data;
using Pragathi.Admin.Infrastructure.Repositories;
using Pragathi.Admin.Infrastructure.Security;

namespace Pragathi.Admin.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AdminDb>(o => o.UseNpgsql(connectionString));
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        return services;
    }
}