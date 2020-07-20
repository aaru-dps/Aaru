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

            migrationBuilder.CreateTable("ModePage_2A", table => new
            {
                Id                        = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                AccurateCDDA              = table.Column<bool>(),
                AudioPlay                 = table.Column<bool>(),
                BCK                       = table.Column<bool>(),
                BUF                       = table.Column<bool>(),
                BufferSize                = table.Column<ushort>(),
                C2Pointer                 = table.Column<bool>(),
                CDDACommand               = table.Column<bool>(),
                CMRSupported              = table.Column<ushort>(),
                Composite                 = table.Column<bool>(),
                CurrentSpeed              = table.Column<ushort>(),
                CurrentWriteSpeed         = table.Column<ushort>(),
                CurrentWriteSpeedSelected = table.Column<ushort>(),
                DeinterlaveSubchannel     = table.Column<bool>(),
                DigitalPort1              = table.Column<bool>(),
                DigitalPort2              = table.Column<bool>(),
                Eject                     = table.Column<bool>(),
                ISRC                      = table.Column<bool>(),
                LSBF                      = table.Column<bool>(),
                LeadInPW                  = table.Column<bool>(),
                Length                    = table.Column<byte>(),
                LoadingMechanism          = table.Column<byte>(),
                Lock                      = table.Column<bool>(),
                LockState                 = table.Column<bool>(),
                MaxWriteSpeed             = table.Column<ushort>(),
                MaximumSpeed              = table.Column<ushort>(),
                Method2                   = table.Column<bool>(),
                Mode2Form1                = table.Column<bool>(),
                Mode2Form2                = table.Column<bool>(),
                MultiSession              = table.Column<bool>(),
                PS                        = table.Column<bool>(),
                PreventJumper             = table.Column<bool>(),
                RCK                       = table.Column<bool>(),
                ReadBarcode               = table.Column<bool>(),
                ReadCDR                   = table.Column<bool>(),
                ReadCDRW                  = table.Column<bool>(),
                ReadDVDR                  = table.Column<bool>(),
                ReadDVDRAM                = table.Column<bool>(),
                ReadDVDROM                = table.Column<bool>(),
                RotationControlSelected   = table.Column<byte>(),
                SCC                       = table.Column<bool>(),
                SDP                       = table.Column<bool>(),
                SSS                       = table.Column<bool>(),
                SeparateChannelMute       = table.Column<bool>(),
                SeparateChannelVolume     = table.Column<bool>(),
                Subchannel                = table.Column<bool>(),
                SupportedVolumeLevels     = table.Column<ushort>(),
                TestWrite                 = table.Column<bool>(),
                UPC                       = table.Column<bool>(),
                WriteCDR                  = table.Column<bool>(),
                WriteCDRW                 = table.Column<bool>(),
                WriteDVDR                 = table.Column<bool>(),
                WriteDVDRAM               = table.Column<bool>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ModePage_2A", x => x.Id);
            });
        }
    }
}