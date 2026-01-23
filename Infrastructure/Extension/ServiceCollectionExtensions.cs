using CloudinaryDotNet;
using Infrastructure.Efcore.Interceptors;
using Infrastructure.Efcore.Persistence;
using Infrastructure.MarkerInterfaces;
using Infrastructure.Services.FileStorage.Cloudinary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extension
{
    public static class ServiceCollectionExtensions
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

            services.Scan(scan => scan
                .FromAssemblyOf<IInfrastructureMarker>()
                // Register Scoped Services
                .AddClasses(classes => classes.AssignableTo<IScopedService>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()

                // Register Transient Services
                .AddClasses(classes => classes.AssignableTo<ITransientService>())
                .AsImplementedInterfaces()
                .WithTransientLifetime()

                // Register Singletons
                .AddClasses(classes => classes.AssignableTo<ISingletonService>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<CloudinarySettings>();
                var account = new Account(
                    settings.CloudName,
                    settings.ApiKey,
                    settings.ApiSecret
                );
                return new Cloudinary(account);
            });
        }

        public static IServiceCollection AddSettings(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Find all classes implementing IAppSettings
            var settingsTypes = typeof(IInfrastructureMarker).Assembly
                .GetTypes()
                .Where(t => typeof(IAppSettings).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

            // Call services.Configure<T>(configuration.GetSection("Name")) for each type
            foreach (var type in settingsTypes)
            {
                // Use the Class Name as the Section Name (Convention)
                var sectionName = type.Name;
                var section = configuration.GetSection(sectionName);

                // Find the generic Configure method
                var configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
                    .GetMethods()
                    .First(m => m.Name == "Configure" && m.GetParameters().Length == 2);

                // Make it generic for the current type <CurrentSettingsType>
                var genericMethod = configureMethod.MakeGenericMethod(type);

                // Invoke: services.Configure<CurrentSettingsType>(section)
                genericMethod.Invoke(null, new object[] { services, section });
            }

            return services;
        }
    }
}