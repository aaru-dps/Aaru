using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class LinkDeviceStatsToReport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeviceStats", "Report_Id", c => c.Int());
            CreateIndex("dbo.DeviceStats", "Report_Id");
            AddForeignKey("dbo.DeviceStats", "Report_Id", "dbo.Devices", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.DeviceStats", "Report_Id", "dbo.Devices");
            DropIndex("dbo.DeviceStats", new[] {"Report_Id"});
            DropColumn("dbo.DeviceStats", "Report_Id");
        }
    }
}