using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Infrastructure.Efcore.Interceptors;
using Infrastructure.Efcore.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extension
{
    public static partial class ServiceCollectionExtensions
    {
        private static readonly Type[] _interceptors =
        [
            typeof(SqlExceptionHandlingInterceptor),
            typeof(CommandInterceptor)
        ];

        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register all interceptors as singletons
            foreach (var interceptorType in _interceptors)
            {
                services.AddSingleton(interceptorType);
            }

            services.AddDbContextPool<BillScannerDbContext>((serviceProvider, options) =>
            {
                var interceptorInstances = _interceptors
                    .Select(serviceProvider.GetRequiredService)
                    .Cast<IInterceptor>()
                    .ToArray();

                options
                    .UseNpgsql(configuration.GetConnectionString("BillScannerDb"))
                    .AddInterceptors(interceptorInstances)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserTokenService, UserTokenService>();
        }
    }
}