using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;

namespace ServerCore.Areas.Deployment
{
    public class DeploymentConfiguration
    {
        internal static void ConfigureDatabase(IConfiguration configuration, IServiceCollection services, IWebHostEnvironment env)
        {
            // Use SQL Database if in Azure, otherwise, use localdb
            // Using a DbContextFactory allows for factories to be used in Blazor components and this also registers the DbContext in the usual way as well behind the scenes
            if (env.IsStaging() && Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") == "PuzzleServerTestDeploy")
            {
                services.AddDbContextFactory<PuzzleServerContext>
                    (options => options.UseLazyLoadingProxies()
                        .UseSqlServer(configuration.GetConnectionString("PuzzleServerSQLConnectionString")));
            }
            else if (env.IsProduction() && (Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") == "puzzlehunt" || Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") == "puzzleday"))
            {
                services.AddDbContextFactory<PuzzleServerContext>
                    (options => options.UseLazyLoadingProxies()
                        .UseSqlServer(configuration.GetConnectionString("PuzzleServerSQLConnectionString")));
            }
            else
            {
                services.AddDbContextFactory<PuzzleServerContext>
                    (options => options.UseLazyLoadingProxies()
                        .UseSqlServer(configuration.GetConnectionString("PuzzleServerContextLocal")));
            }
        }
    }
}
