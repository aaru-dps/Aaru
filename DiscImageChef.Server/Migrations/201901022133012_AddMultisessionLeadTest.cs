using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class AddMultisessionLeadTest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestedMedias", "CanReadingIntersessionLeadIn",  c => c.Boolean());
            AddColumn("dbo.TestedMedias", "CanReadingIntersessionLeadOut", c => c.Boolean());
            AddColumn("dbo.TestedMedias", "IntersessionLeadInData",        c => c.Binary());
            AddColumn("dbo.TestedMedias", "IntersessionLeadOutData",       c => c.Binary());
        }

        public override void Down()
        {
            DropColumn("dbo.TestedMedias", "IntersessionLeadOutData");
            DropColumn("dbo.TestedMedias", "IntersessionLeadInData");
            DropColumn("dbo.TestedMedias", "CanReadingIntersessionLeadOut");
            DropColumn("dbo.TestedMedias", "CanReadingIntersessionLeadIn");
        }
    }
}