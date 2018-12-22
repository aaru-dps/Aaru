using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class AddStatistics : DbMigration
    {
        public override void Up()
        {
            CreateTable("dbo.Commands",
                        c => new {Id = c.Int(false, true), Name = c.String(unicode: false), Count = c.Long(false)})
               .PrimaryKey(t => t.Id);

            CreateTable("dbo.DeviceStats",
                        c => new
                        {
                            Id           = c.Int(false, true),
                            Manufacturer = c.String(unicode: false),
                            Model        = c.String(unicode: false),
                            Revision     = c.String(unicode: false),
                            Bus          = c.String(unicode: false)
                        }).PrimaryKey(t => t.Id);

            CreateTable("dbo.Filesystems",
                        c => new {Id = c.Int(false, true), Name = c.String(unicode: false), Count = c.Long(false)})
               .PrimaryKey(t => t.Id);

            CreateTable("dbo.Filters",
                        c => new {Id = c.Int(false, true), Name = c.String(unicode: false), Count = c.Long(false)})
               .PrimaryKey(t => t.Id);

            CreateTable("dbo.MediaFormats",
                        c => new {Id = c.Int(false, true), Name = c.String(unicode: false), Count = c.Long(false)})
               .PrimaryKey(t => t.Id);

            CreateTable("dbo.Media",
                        c => new
                        {
                            Id    = c.Int(false, true),
                            Type  = c.String(unicode: false),
                            Real  = c.Boolean(false),
                            Count = c.Long(false)
                        }).PrimaryKey(t => t.Id);

            CreateTable("dbo.OperatingSystems",
                        c => new
                        {
                            Id      = c.Int(false, true),
                            Name    = c.String(unicode: false),
                            Version = c.String(unicode: false),
                            Count   = c.Long(false)
                        }).PrimaryKey(t => t.Id);

            CreateTable("dbo.Partitions",
                        c => new {Id = c.Int(false, true), Name = c.String(unicode: false), Count = c.Long(false)})
               .PrimaryKey(t => t.Id);

            CreateTable("dbo.Versions",
                        c => new {Id = c.Int(false, true), Value = c.String(unicode: false), Count = c.Long(false)})
               .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.Versions");
            DropTable("dbo.Partitions");
            DropTable("dbo.OperatingSystems");
            DropTable("dbo.Media");
            DropTable("dbo.MediaFormats");
            DropTable("dbo.Filters");
            DropTable("dbo.Filesystems");
            DropTable("dbo.DeviceStats");
            DropTable("dbo.Commands");
        }
    }
}