using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Organization.Infrastructure.Persistence;
using Organization.Application.Interfaces;
using Organization.Infrastructure.Services;

namespace Organization.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("OrganizationDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "OrganizationDatabase connection string is missing.");
        }

        services.AddDbContext<OrganizationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IEmployeeService, EmployeeService>();

        return services;
    }
}