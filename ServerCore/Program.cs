using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace ServerCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            PuzzleStateHelper.ServiceProvider = host.Services;
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
             .Build();
    }
}
