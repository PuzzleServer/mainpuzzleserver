using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.Areas.Deployment;
using ServerCore.DataModel;

[assembly: HostingStartup(typeof(ServerCore.Areas.Identity.IdentityHostingStartup))]
namespace ServerCore.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                // Set up to use Azure settings
                IWebHostEnvironment env = context.HostingEnvironment;
                IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();
                context.Configuration = configBuilder.Build();

                DeploymentConfiguration.ConfigureDatabase(context.Configuration, services, env);

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<PuzzleServerContext>();
            });
        }
    }
}