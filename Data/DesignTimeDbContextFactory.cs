using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;

namespace ServerCore
{
    /// <summary>
    /// Helps EF Core tools create a PuzzleServerContext when the site isn't running, e.g. for scaffolding.
    /// Never runs at runtime.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PuzzleServerContext>
    {
        public PuzzleServerContext CreateDbContext(string[] args)
        {
            var services = new ServiceCollection();

            services.AddDbContext<PuzzleServerContext>(options =>
            {
                options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PuzzleServer;Trusted_Connection=True;MultipleActiveResultSets=true");
                options.UseLazyLoadingProxies(true);
            });

            // Register Identity so the model matches runtime configuration from IdentityHostingStartup.
            // Without this, IdentityDbContext.OnModelCreating won't pick up the Identity options
            // that determine column sizes for AspNetUserTokens, AspNetUserLogins, etc.
            services.AddDefaultIdentity<IdentityUser>(options =>
                options.Stores.MaxLengthForKeys = 450)
                .AddEntityFrameworkStores<PuzzleServerContext>();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<PuzzleServerContext>();
        }
    }
}
