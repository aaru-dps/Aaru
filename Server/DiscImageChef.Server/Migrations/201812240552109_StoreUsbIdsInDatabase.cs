using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class StoreUsbIdsInDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable("dbo.UsbProducts",
                        c => new
                        {
                            Id           = c.Int(false, true),
                            ProductId    = c.Int(false),
                            Product      = c.String(unicode: false),
                            AddedWhen    = c.DateTime(false, 0),
                            ModifiedWhen = c.DateTime(false, 0),
                            VendorId     = c.Int(false)
                        }).PrimaryKey(t => t.Id).ForeignKey("dbo.UsbVendors", t => t.VendorId, true)
                          .Index(t => t.VendorId);

            CreateTable("dbo.UsbVendors",
                        c => new
                        {
                            Id           = c.Int(false, true),
                            Vendor       = c.String(unicode: false),
                            AddedWhen    = c.DateTime(false, 0),
                            ModifiedWhen = c.DateTime(false, 0)
                        }).PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.UsbProducts", "VendorId", "dbo.UsbVendors");
            DropIndex("dbo.UsbProducts", new[] {"VendorId"});
            DropTable("dbo.UsbVendors");
            DropTable("dbo.UsbProducts");
        }
    }
}