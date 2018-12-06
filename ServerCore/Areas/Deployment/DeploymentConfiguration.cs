using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;

namespace ServerCore.Areas.Deployment
{
    public class DeploymentConfiguration
    {
        internal static void ConfigureDatabase(IConfiguration configuration, IServiceCollection services)
        {
            // Use SQL Database if in Azure, otherwise, use localdb
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {

                services.AddDbContext<PuzzleServerContext>
                    (options => options.UseLazyLoadingProxies()
                        .UseSqlServer(configuration.GetConnectionString("PuzzleServerSQLConnectionString")));
            }
            else
            {
                services.AddDbContext<PuzzleServerContext>
                    (options => options.UseLazyLoadingProxies()
                        .UseSqlServer(configuration.GetConnectionString("PuzzleServerContextLocal")));
            }
        }
    }
}
