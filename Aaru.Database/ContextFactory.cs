using Microsoft.EntityFrameworkCore.Design;

namespace Aaru.Database
{
    public class AaruContextFactory : IDesignTimeDbContextFactory<AaruContext>
    {
        public AaruContext CreateDbContext(string[] args) => AaruContext.Create("aaru.db");
    }
}