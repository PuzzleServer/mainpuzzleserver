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
                services.AddDbContext<PuzzleServerContext>(options =>
                    options.UseLazyLoadingProxies()
                    .UseSqlServer(
                        context.Configuration.GetConnectionString("PuzzleServerContext")));

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<PuzzleServerContext>();
            });
        }
    }
}