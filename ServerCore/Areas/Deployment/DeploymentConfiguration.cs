using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;

namespace ServerCore.Areas.Deployment
{
    public class DeploymentConfiguration
    {
        internal static void ConfigureDatabase(IConfiguration configuration, IServiceCollection services, IHostingEnvironment env)
        {
            // Use SQL Database if in Azure, otherwise, use localdb
            if (env.IsStaging() && Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") == "PuzzleServerTestDeploy")
            {
                services.AddDbContext<PuzzleServerContext>
                    (options => options.UseLazyLoadingProxies()
                        .UseSqlServer(configuration.GetConnectionString("PuzzleServerSQLConnectionString"),
                        contextOptions => contextOptions.EnableRetryOnFailure()));
            }
            else if (env.IsProduction() && (Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") == "puzzlehunt" || Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") == "puzzleday"))
            {
                services.AddDbContext<PuzzleServerContext>
                    (options => options.UseLazyLoadingProxies()
                        .UseSqlServer(configuration.GetConnectionString("PuzzleServerSQLConnectionString"),
                        contextOptions => contextOptions.EnableRetryOnFailure()));
            }
            else
            {
                services.AddDbContext<PuzzleServerContext>
                    (options => options.UseLazyLoadingProxies()
                       .UseSqlServer(configuration.GetConnectionString("PuzzleServerContextLocal"),
                       contextOptions => contextOptions.EnableRetryOnFailure()));
            }
        }
    }
}
