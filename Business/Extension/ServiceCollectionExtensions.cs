using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            
            services.Configure<BusinessSettings>(configuration.GetSection("BusinessSettings"));
        }
    }
}