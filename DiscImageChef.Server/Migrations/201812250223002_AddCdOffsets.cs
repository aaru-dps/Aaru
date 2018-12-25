using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class AddCdOffsets : DbMigration
    {
        public override void Up()
        {
            CreateTable("dbo.CompactDiscOffsets",
                        c => new
                        {
                            Id           = c.Int(false, true),
                            AddedWhen    = c.DateTime(false, 0),
                            ModifiedWhen = c.DateTime(false, 0),
                            Manufacturer = c.String(unicode: false),
                            Model        = c.String(unicode: false),
                            Offset       = c.Short(false),
                            Submissions  = c.Int(false),
                            Agreement    = c.Single(false)
                        }).PrimaryKey(t => t.Id).Index(t => t.ModifiedWhen);

            AddColumn("dbo.Devices", "ModifiedWhen", c => c.DateTime(precision: 0));
            AddColumn("dbo.Devices", "CdOffset_Id",  c => c.Int());
            CreateIndex("dbo.Devices", "ModifiedWhen");
            CreateIndex("dbo.Devices", "CdOffset_Id");
            AddForeignKey("dbo.Devices", "CdOffset_Id", "dbo.CompactDiscOffsets", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Devices", "CdOffset_Id", "dbo.CompactDiscOffsets");
            DropIndex("dbo.Devices",            new[] {"CdOffset_Id"});
            DropIndex("dbo.Devices",            new[] {"ModifiedWhen"});
            DropIndex("dbo.CompactDiscOffsets", new[] {"ModifiedWhen"});
            DropColumn("dbo.Devices", "CdOffset_Id");
            DropColumn("dbo.Devices", "ModifiedWhen");
            DropTable("dbo.CompactDiscOffsets");
        }
    }
}