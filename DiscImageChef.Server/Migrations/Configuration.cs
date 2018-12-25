using System.Data.Entity.Migrations;
using DiscImageChef.Server.Models;

namespace DiscImageChef.Server.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<DicServerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DicServerContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}