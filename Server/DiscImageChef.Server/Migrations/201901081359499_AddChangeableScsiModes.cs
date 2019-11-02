using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class AddChangeableScsiModes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Scsis", "ModeSense6CurrentData",     c => c.Binary());
            AddColumn("dbo.Scsis", "ModeSense10CurrentData",    c => c.Binary());
            AddColumn("dbo.Scsis", "ModeSense6ChangeableData",  c => c.Binary());
            AddColumn("dbo.Scsis", "ModeSense10ChangeableData", c => c.Binary());
        }

        public override void Down()
        {
            DropColumn("dbo.Scsis", "ModeSense10ChangeableData");
            DropColumn("dbo.Scsis", "ModeSense6ChangeableData");
            DropColumn("dbo.Scsis", "ModeSense10CurrentData");
            DropColumn("dbo.Scsis", "ModeSense6CurrentData");
        }
    }
}