using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

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
