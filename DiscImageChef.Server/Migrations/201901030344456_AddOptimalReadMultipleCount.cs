using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class AddOptimalReadMultipleCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Devices", "OptimalMultipleSectorsRead", c => c.Int(false, defaultValue: 0));
        }

        public override void Down()
        {
            DropColumn("dbo.Devices", "OptimalMultipleSectorsRead");
        }
    }
}