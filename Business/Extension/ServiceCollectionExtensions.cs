using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Business.Interfaces.Builders;
using Business.Builders;

namespace Business.Extension
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplications(this IServiceCollection services, IConfiguration configuration)
        {
            var applicationAssembly = typeof(ServiceCollectionExtensions).Assembly;

            services.AddAutoMapper(cfg => { cfg.LicenseKey = configuration["AutoMapperLicenseKey"]; },
                applicationAssembly);
            services.AddMediatR(cfg =>
            {
                cfg.LicenseKey = configuration["AutoMapperLicenseKey"];
                cfg.RegisterServicesFromAssembly(applicationAssembly);
            });

            services.AddScoped<IBuilderFactory, BuilderFactory>();

            services.Scan(scan => scan
                .FromAssemblies(applicationAssembly)
                .AddClasses(classes => classes.AssignableTo<IBuilderMarker>())
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            services.Configure<BusinessSettings>(configuration.GetSection("BusinessSettings"));
        }
    }
}