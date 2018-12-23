using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class FixUnsignedFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestedMedias",       "BlocksSql",                          c => c.Long());
            AddColumn("dbo.TestedMedias",       "BlockSizeSql",                       c => c.Int());
            AddColumn("dbo.TestedMedias",       "LongBlockSizeSql",                   c => c.Int());
            AddColumn("dbo.TestedMedias",       "LBASectorsSql",                      c => c.Int());
            AddColumn("dbo.TestedMedias",       "LBA48SectorsSql",                    c => c.Long());
            AddColumn("dbo.TestedMedias",       "LogicalAlignmentSql",                c => c.Short());
            AddColumn("dbo.TestedMedias",       "NominalRotationRateSql",             c => c.Short());
            AddColumn("dbo.TestedMedias",       "PhysicalBlockSizeSql",               c => c.Int());
            AddColumn("dbo.TestedMedias",       "UnformattedBPTSql",                  c => c.Short());
            AddColumn("dbo.TestedMedias",       "UnformattedBPSSql",                  c => c.Short());
            AddColumn("dbo.Chs",                "CylindersSql",                       c => c.Short(false));
            AddColumn("dbo.Chs",                "HeadsSql",                           c => c.Short(false));
            AddColumn("dbo.Chs",                "SectorsSql",                         c => c.Short(false));
            AddColumn("dbo.FireWires",          "VendorIDSql",                        c => c.Int(false));
            AddColumn("dbo.FireWires",          "ProductIDSql",                       c => c.Int(false));
            AddColumn("dbo.Pcmcias",            "ManufacturerCodeSql",                c => c.Short());
            AddColumn("dbo.Pcmcias",            "CardCodeSql",                        c => c.Short());
            AddColumn("dbo.BlockDescriptors",   "BlocksSql",                          c => c.Long());
            AddColumn("dbo.BlockDescriptors",   "BlockLengthSql",                     c => c.Int());
            AddColumn("dbo.MmcFeatures",        "BlocksPerReadableUnitSql",           c => c.Short());
            AddColumn("dbo.MmcFeatures",        "LogicalBlockSizeSql",                c => c.Int());
            AddColumn("dbo.MmcFeatures",        "PhysicalInterfaceStandardNumberSql", c => c.Int());
            AddColumn("dbo.MmcFeatures",        "VolumeLevelsSql",                    c => c.Short());
            AddColumn("dbo.Sscs",               "MaxBlockLengthSql",                  c => c.Int());
            AddColumn("dbo.Sscs",               "MinBlockLengthSql",                  c => c.Int());
            AddColumn("dbo.SupportedDensities", "BitsPerMmSql",                       c => c.Int(false));
            AddColumn("dbo.SupportedDensities", "WidthSql",                           c => c.Short(false));
            AddColumn("dbo.SupportedDensities", "TracksSql",                          c => c.Short(false));
            AddColumn("dbo.SupportedDensities", "CapacitySql",                        c => c.Int(false));
            AddColumn("dbo.SscSupportedMedias", "WidthSql",                           c => c.Short(false));
            AddColumn("dbo.SscSupportedMedias", "LengthSql",                          c => c.Short(false));
            AddColumn("dbo.Usbs",               "VendorIDSql",                        c => c.Short(false));
            AddColumn("dbo.Usbs",               "ProductIDSql",                       c => c.Short(false));
        }

        public override void Down()
        {
            DropColumn("dbo.Usbs",               "ProductIDSql");
            DropColumn("dbo.Usbs",               "VendorIDSql");
            DropColumn("dbo.SscSupportedMedias", "LengthSql");
            DropColumn("dbo.SscSupportedMedias", "WidthSql");
            DropColumn("dbo.SupportedDensities", "CapacitySql");
            DropColumn("dbo.SupportedDensities", "TracksSql");
            DropColumn("dbo.SupportedDensities", "WidthSql");
            DropColumn("dbo.SupportedDensities", "BitsPerMmSql");
            DropColumn("dbo.Sscs",               "MinBlockLengthSql");
            DropColumn("dbo.Sscs",               "MaxBlockLengthSql");
            DropColumn("dbo.MmcFeatures",        "VolumeLevelsSql");
            DropColumn("dbo.MmcFeatures",        "PhysicalInterfaceStandardNumberSql");
            DropColumn("dbo.MmcFeatures",        "LogicalBlockSizeSql");
            DropColumn("dbo.MmcFeatures",        "BlocksPerReadableUnitSql");
            DropColumn("dbo.BlockDescriptors",   "BlockLengthSql");
            DropColumn("dbo.BlockDescriptors",   "BlocksSql");
            DropColumn("dbo.Pcmcias",            "CardCodeSql");
            DropColumn("dbo.Pcmcias",            "ManufacturerCodeSql");
            DropColumn("dbo.FireWires",          "ProductIDSql");
            DropColumn("dbo.FireWires",          "VendorIDSql");
            DropColumn("dbo.Chs",                "SectorsSql");
            DropColumn("dbo.Chs",                "HeadsSql");
            DropColumn("dbo.Chs",                "CylindersSql");
            DropColumn("dbo.TestedMedias",       "UnformattedBPSSql");
            DropColumn("dbo.TestedMedias",       "UnformattedBPTSql");
            DropColumn("dbo.TestedMedias",       "PhysicalBlockSizeSql");
            DropColumn("dbo.TestedMedias",       "NominalRotationRateSql");
            DropColumn("dbo.TestedMedias",       "LogicalAlignmentSql");
            DropColumn("dbo.TestedMedias",       "LBA48SectorsSql");
            DropColumn("dbo.TestedMedias",       "LBASectorsSql");
            DropColumn("dbo.TestedMedias",       "LongBlockSizeSql");
            DropColumn("dbo.TestedMedias",       "BlockSizeSql");
            DropColumn("dbo.TestedMedias",       "BlocksSql");
        }
    }
}