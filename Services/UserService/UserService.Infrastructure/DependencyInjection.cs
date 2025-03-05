using BuildingBlocks.RepositoryBase.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Data;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        if(configuration is null) { throw new ArgumentNullException(nameof(configuration)); }
        services.AddTransient(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });
        
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        return services;
    }
}