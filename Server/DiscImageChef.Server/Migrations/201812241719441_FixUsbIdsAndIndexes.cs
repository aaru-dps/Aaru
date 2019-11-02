using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class FixUsbIdsAndIndexes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UsbVendors", "VendorId", c => c.Int(false));
            CreateIndex("dbo.UsbProducts", "ProductId");
            CreateIndex("dbo.UsbProducts", "ModifiedWhen");
            CreateIndex("dbo.UsbVendors",  "VendorId", true);
            CreateIndex("dbo.UsbVendors",  "ModifiedWhen");
        }

        public override void Down()
        {
            DropIndex("dbo.UsbVendors",  new[] {"ModifiedWhen"});
            DropIndex("dbo.UsbVendors",  new[] {"VendorId"});
            DropIndex("dbo.UsbProducts", new[] {"ModifiedWhen"});
            DropIndex("dbo.UsbProducts", new[] {"ProductId"});
            DropColumn("dbo.UsbVendors", "VendorId");
        }
    }
}