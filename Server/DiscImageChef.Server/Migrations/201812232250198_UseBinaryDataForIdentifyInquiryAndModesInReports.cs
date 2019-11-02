using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class UseBinaryDataForIdentifyInquiryAndModesInReports : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Mmcs", "ModeSense2A_Id", "ModePage_2A");
            DropIndex("dbo.Mmcs", new[] {"ModeSense2A_Id"});
            AddColumn("dbo.Mmcs", "ModeSense2AData", c => c.Binary());
            DropColumn("dbo.Mmcs", "ModeSense2A_Id");
            DropTable("dbo.ModePage_2A");
        }

        public override void Down()
        {
            CreateTable("dbo.ModePage_2A",
                        c => new
                        {
                            Id                      = c.Int(false, true),
                            PS                      = c.Boolean(false),
                            MultiSession            = c.Boolean(false),
                            Mode2Form2              = c.Boolean(false),
                            Mode2Form1              = c.Boolean(false),
                            AudioPlay               = c.Boolean(false),
                            ISRC                    = c.Boolean(false),
                            UPC                     = c.Boolean(false),
                            C2Pointer               = c.Boolean(false),
                            DeinterlaveSubchannel   = c.Boolean(false),
                            Subchannel              = c.Boolean(false),
                            AccurateCDDA            = c.Boolean(false),
                            CDDACommand             = c.Boolean(false),
                            LoadingMechanism        = c.Byte(false),
                            Eject                   = c.Boolean(false),
                            PreventJumper           = c.Boolean(false),
                            LockState               = c.Boolean(false),
                            Lock                    = c.Boolean(false),
                            SeparateChannelMute     = c.Boolean(false),
                            SeparateChannelVolume   = c.Boolean(false),
                            Method2                 = c.Boolean(false),
                            ReadCDRW                = c.Boolean(false),
                            ReadCDR                 = c.Boolean(false),
                            WriteCDRW               = c.Boolean(false),
                            WriteCDR                = c.Boolean(false),
                            DigitalPort2            = c.Boolean(false),
                            DigitalPort1            = c.Boolean(false),
                            Composite               = c.Boolean(false),
                            SSS                     = c.Boolean(false),
                            SDP                     = c.Boolean(false),
                            Length                  = c.Byte(false),
                            LSBF                    = c.Boolean(false),
                            RCK                     = c.Boolean(false),
                            BCK                     = c.Boolean(false),
                            TestWrite               = c.Boolean(false),
                            ReadBarcode             = c.Boolean(false),
                            ReadDVDRAM              = c.Boolean(false),
                            ReadDVDR                = c.Boolean(false),
                            ReadDVDROM              = c.Boolean(false),
                            WriteDVDRAM             = c.Boolean(false),
                            WriteDVDR               = c.Boolean(false),
                            LeadInPW                = c.Boolean(false),
                            SCC                     = c.Boolean(false),
                            BUF                     = c.Boolean(false),
                            RotationControlSelected = c.Byte(false)
                        }).PrimaryKey(t => t.Id);

            AddColumn("dbo.Mmcs", "ModeSense2A_Id", c => c.Int());
            DropColumn("dbo.Mmcs", "ModeSense2AData");
            CreateIndex("dbo.Mmcs", "ModeSense2A_Id");
            AddForeignKey("dbo.Mmcs", "ModeSense2A_Id", "dbo.ModePage_2A", "Id");
        }
    }
}