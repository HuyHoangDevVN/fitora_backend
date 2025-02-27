using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using InteractService.Application.Data;
using InteractService.Application.Services.IServices;
using InteractService.Infrastructure.Data;
using InteractService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InteractService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        if(configuration is null) { throw new ArgumentNullException(nameof(configuration)); }
        services.AddTransient(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IAuthorizeExtension, AuthorizeExtension>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });
        
   
        
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        return services;
    }
}