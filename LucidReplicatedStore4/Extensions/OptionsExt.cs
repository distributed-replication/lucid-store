using LucidBase.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LucidStore.Extensions
{
    public static class OptionsExt
    {
        public static void ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppSettings>(options => configuration.GetSection("AppSettings").Bind(options));
            services.Configure<LucidUrls>(options => configuration.GetSection("LucidUrls").Bind(options));
        }
    }
}
