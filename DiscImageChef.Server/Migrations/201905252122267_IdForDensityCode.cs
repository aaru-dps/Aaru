using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class IdForDensityCode : DbMigration
    {
        public override void Up()
        {
            RenameTable("DensityCodes", "DensityCodes_old");

            CreateTable("dbo.DensityCodes",
                        c => new {Code = c.Int(false), SscSupportedMedia_Id = c.Int(), Id = c.Int(false, true)})
               .PrimaryKey(t => t.Id).ForeignKey("dbo.SscSupportedMedias", t => t.SscSupportedMedia_Id)
               .Index(t => t.SscSupportedMedia_Id);

            Sql("INSERT INTO DensityCodes (Code, SscSupportedMedia_Id) SELECT Code, SscSupportedMedia_Id FROM DensityCodes_old");

            DropTable("DensityCodes_old");
        }

        public override void Down()
        {
            RenameTable("DensityCodes", "DensityCodes_old");

            CreateTable("dbo.DensityCodes", c => new {Code = c.Int(false, true), SscSupportedMedia_Id = c.Int()})
               .PrimaryKey(t => t.Code).ForeignKey("dbo.SscSupportedMedias", t => t.SscSupportedMedia_Id)
               .Index(t => t.SscSupportedMedia_Id);

            Sql("INSERT INTO DensityCodes (Code, SscSupportedMedia_Id) SELECT Code, SscSupportedMedia_Id FROM DensityCodes_old");

            DropTable("DensityCodes_old");
        }
    }
}