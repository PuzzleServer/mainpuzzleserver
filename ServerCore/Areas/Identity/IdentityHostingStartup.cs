using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                // Use SQL Database if in Azure, otherwise, use localdb
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    // Set up to use Azure settings
                    IHostingEnvironment env = context.HostingEnvironment;
                    IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                        .SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables();
                    context.Configuration = configBuilder.Build();

                    services.AddDbContext<PuzzleServerContext>
                        (options => options.UseLazyLoadingProxies()
                            .UseSqlServer(context.Configuration.GetConnectionString("PuzzleServerSQLConnectionString")));
                }
                else
                {
                    services.AddDbContext<PuzzleServerContext>
                        (options => options.UseLazyLoadingProxies()
                            .UseSqlServer(context.Configuration.GetConnectionString("PuzzleServerContextLocal")));
                }

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<PuzzleServerContext>();
            });
        }
    }
}