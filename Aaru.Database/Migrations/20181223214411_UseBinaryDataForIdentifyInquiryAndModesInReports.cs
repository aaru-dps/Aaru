using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class UseBinaryDataForIdentifyInquiryAndModesInReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("ModePage_2A");

            migrationBuilder.AddColumn<byte[]>("ModeSense2AData", "Mmc", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("ModeSense2AData", "Mmc");

            migrationBuilder.CreateTable("ModePage_2A",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<int>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             AccurateCDDA              = table.Column<bool>(nullable: false),
                                             AudioPlay                 = table.Column<bool>(nullable: false),
                                             BCK                       = table.Column<bool>(nullable: false),
                                             BUF                       = table.Column<bool>(nullable: false),
                                             BufferSize                = table.Column<ushort>(nullable: false),
                                             C2Pointer                 = table.Column<bool>(nullable: false),
                                             CDDACommand               = table.Column<bool>(nullable: false),
                                             CMRSupported              = table.Column<ushort>(nullable: false),
                                             Composite                 = table.Column<bool>(nullable: false),
                                             CurrentSpeed              = table.Column<ushort>(nullable: false),
                                             CurrentWriteSpeed         = table.Column<ushort>(nullable: false),
                                             CurrentWriteSpeedSelected = table.Column<ushort>(nullable: false),
                                             DeinterlaveSubchannel     = table.Column<bool>(nullable: false),
                                             DigitalPort1              = table.Column<bool>(nullable: false),
                                             DigitalPort2              = table.Column<bool>(nullable: false),
                                             Eject                     = table.Column<bool>(nullable: false),
                                             ISRC                      = table.Column<bool>(nullable: false),
                                             LSBF                      = table.Column<bool>(nullable: false),
                                             LeadInPW                  = table.Column<bool>(nullable: false),
                                             Length                    = table.Column<byte>(nullable: false),
                                             LoadingMechanism          = table.Column<byte>(nullable: false),
                                             Lock                      = table.Column<bool>(nullable: false),
                                             LockState                 = table.Column<bool>(nullable: false),
                                             MaxWriteSpeed             = table.Column<ushort>(nullable: false),
                                             MaximumSpeed              = table.Column<ushort>(nullable: false),
                                             Method2                   = table.Column<bool>(nullable: false),
                                             Mode2Form1                = table.Column<bool>(nullable: false),
                                             Mode2Form2                = table.Column<bool>(nullable: false),
                                             MultiSession              = table.Column<bool>(nullable: false),
                                             PS                        = table.Column<bool>(nullable: false),
                                             PreventJumper             = table.Column<bool>(nullable: false),
                                             RCK                       = table.Column<bool>(nullable: false),
                                             ReadBarcode               = table.Column<bool>(nullable: false),
                                             ReadCDR                   = table.Column<bool>(nullable: false),
                                             ReadCDRW                  = table.Column<bool>(nullable: false),
                                             ReadDVDR                  = table.Column<bool>(nullable: false),
                                             ReadDVDRAM                = table.Column<bool>(nullable: false),
                                             ReadDVDROM                = table.Column<bool>(nullable: false),
                                             RotationControlSelected   = table.Column<byte>(nullable: false),
                                             SCC                       = table.Column<bool>(nullable: false),
                                             SDP                       = table.Column<bool>(nullable: false),
                                             SSS                       = table.Column<bool>(nullable: false),
                                             SeparateChannelMute       = table.Column<bool>(nullable: false),
                                             SeparateChannelVolume     = table.Column<bool>(nullable: false),
                                             Subchannel                = table.Column<bool>(nullable: false),
                                             SupportedVolumeLevels     = table.Column<ushort>(nullable: false),
                                             TestWrite                 = table.Column<bool>(nullable: false),
                                             UPC                       = table.Column<bool>(nullable: false),
                                             WriteCDR                  = table.Column<bool>(nullable: false),
                                             WriteCDRW                 = table.Column<bool>(nullable: false),
                                             WriteDVDR                 = table.Column<bool>(nullable: false),
                                             WriteDVDRAM               = table.Column<bool>(nullable: false)
                                         },
                                         constraints: table => { table.PrimaryKey("PK_ModePage_2A", x => x.Id); });
        }
    }
}