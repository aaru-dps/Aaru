namespace DiscImageChef.Server.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UseBinaryDataForIdentifyInquiryAndModesInReports : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Mmcs", "ModeSense2A_Id", "ModePage_2A");
            DropIndex("dbo.Mmcs", new[] { "ModeSense2A_Id" });
            AddColumn("dbo.Mmcs", "ModeSense2AData", c => c.Binary());
            DropColumn("dbo.Mmcs", "ModeSense2A_Id");
            DropTable("dbo.ModePage_2A");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ModePage_2A",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PS = c.Boolean(nullable: false),
                        MultiSession = c.Boolean(nullable: false),
                        Mode2Form2 = c.Boolean(nullable: false),
                        Mode2Form1 = c.Boolean(nullable: false),
                        AudioPlay = c.Boolean(nullable: false),
                        ISRC = c.Boolean(nullable: false),
                        UPC = c.Boolean(nullable: false),
                        C2Pointer = c.Boolean(nullable: false),
                        DeinterlaveSubchannel = c.Boolean(nullable: false),
                        Subchannel = c.Boolean(nullable: false),
                        AccurateCDDA = c.Boolean(nullable: false),
                        CDDACommand = c.Boolean(nullable: false),
                        LoadingMechanism = c.Byte(nullable: false),
                        Eject = c.Boolean(nullable: false),
                        PreventJumper = c.Boolean(nullable: false),
                        LockState = c.Boolean(nullable: false),
                        Lock = c.Boolean(nullable: false),
                        SeparateChannelMute = c.Boolean(nullable: false),
                        SeparateChannelVolume = c.Boolean(nullable: false),
                        Method2 = c.Boolean(nullable: false),
                        ReadCDRW = c.Boolean(nullable: false),
                        ReadCDR = c.Boolean(nullable: false),
                        WriteCDRW = c.Boolean(nullable: false),
                        WriteCDR = c.Boolean(nullable: false),
                        DigitalPort2 = c.Boolean(nullable: false),
                        DigitalPort1 = c.Boolean(nullable: false),
                        Composite = c.Boolean(nullable: false),
                        SSS = c.Boolean(nullable: false),
                        SDP = c.Boolean(nullable: false),
                        Length = c.Byte(nullable: false),
                        LSBF = c.Boolean(nullable: false),
                        RCK = c.Boolean(nullable: false),
                        BCK = c.Boolean(nullable: false),
                        TestWrite = c.Boolean(nullable: false),
                        ReadBarcode = c.Boolean(nullable: false),
                        ReadDVDRAM = c.Boolean(nullable: false),
                        ReadDVDR = c.Boolean(nullable: false),
                        ReadDVDROM = c.Boolean(nullable: false),
                        WriteDVDRAM = c.Boolean(nullable: false),
                        WriteDVDR = c.Boolean(nullable: false),
                        LeadInPW = c.Boolean(nullable: false),
                        SCC = c.Boolean(nullable: false),
                        BUF = c.Boolean(nullable: false),
                        RotationControlSelected = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Mmcs", "ModeSense2A_Id", c => c.Int());
            DropColumn("dbo.Mmcs", "ModeSense2AData");
            CreateIndex("dbo.Mmcs", "ModeSense2A_Id");
            AddForeignKey("dbo.Mmcs", "ModeSense2A_Id", "dbo.ModePage_2A", "Id");
        }
    }
}
