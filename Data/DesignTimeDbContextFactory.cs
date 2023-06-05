using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
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
            var optionsBuilder = new DbContextOptionsBuilder<PuzzleServerContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PuzzleServer;Trusted_Connection=True;MultipleActiveResultSets=true");
            optionsBuilder.UseLazyLoadingProxies(true);

            return new PuzzleServerContext(optionsBuilder.Options);
        }
    }
}
