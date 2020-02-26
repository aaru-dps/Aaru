using Microsoft.EntityFrameworkCore.Design;

namespace DiscImageChef.Database
{
    public class DicContextFactory : IDesignTimeDbContextFactory<DicContext>
    {
        public DicContext CreateDbContext(string[] args) => DicContext.Create("discimagechef.db");
    }
}