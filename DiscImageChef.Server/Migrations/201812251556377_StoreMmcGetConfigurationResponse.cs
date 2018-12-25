using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class StoreMmcGetConfigurationResponse : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MmcFeatures", "BinaryData", c => c.Binary());
        }

        public override void Down()
        {
            DropColumn("dbo.MmcFeatures", "BinaryData");
        }
    }
}