using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class AddGdRomSwapDiscCapabilities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>("GdRomSwapDiscCapabilitiesId", "Reports", nullable: true);

            migrationBuilder.AddColumn<int>("GdRomSwapDiscCapabilitiesId", "Devices", nullable: true);

            migrationBuilder.CreateTable("GdRomSwapDiscCapabilities", table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                RecognizedSwapDisc = table.Column<bool>(nullable: false),
                SwapDiscLeadOutPMIN = table.Column<byte>(nullable: false),
                SwapDiscLeadOutPSEC = table.Column<byte>(nullable: false),
                SwapDiscLeadOutPFRAM = table.Column<byte>(nullable: false),
                SwapDiscLeadOutStart = table.Column<int>(nullable: false),
                Lba0Readable = table.Column<bool>(nullable: false), Lba0Data = table.Column<byte[]>(nullable: true),
                Lba0Sense = table.Column<byte[]>(nullable: true),
                Lba0DecodedSense = table.Column<string>(nullable: true),
                Lba0ScrambledReadable = table.Column<bool>(nullable: false),
                Lba0ScrambledData = table.Column<byte[]>(nullable: true),
                Lba0ScrambledSense = table.Column<byte[]>(nullable: true),
                Lba0ScrambledDecodedSense = table.Column<string>(nullable: true),
                Lba44990Readable = table.Column<bool>(nullable: false),
                Lba44990Data = table.Column<byte[]>(nullable: true),
                Lba44990Sense = table.Column<byte[]>(nullable: true),
                Lba44990DecodedSense = table.Column<string>(nullable: true),
                Lba44990ReadableCluster = table.Column<int>(nullable: false),
                Lba45000Readable = table.Column<bool>(nullable: false),
                Lba45000Data = table.Column<byte[]>(nullable: true),
                Lba45000Sense = table.Column<byte[]>(nullable: true),
                Lba45000DecodedSense = table.Column<string>(nullable: true),
                Lba45000ReadableCluster = table.Column<int>(nullable: false),
                Lba50000Readable = table.Column<bool>(nullable: false),
                Lba50000Data = table.Column<byte[]>(nullable: true),
                Lba50000Sense = table.Column<byte[]>(nullable: true),
                Lba50000DecodedSense = table.Column<string>(nullable: true),
                Lba50000ReadableCluster = table.Column<int>(nullable: false),
                Lba100000Readable = table.Column<bool>(nullable: false),
                Lba100000Data = table.Column<byte[]>(nullable: true),
                Lba100000Sense = table.Column<byte[]>(nullable: true),
                Lba100000DecodedSense = table.Column<string>(nullable: true),
                Lba100000ReadableCluster = table.Column<int>(nullable: false),
                Lba400000Readable = table.Column<bool>(nullable: false),
                Lba400000Data = table.Column<byte[]>(nullable: true),
                Lba400000Sense = table.Column<byte[]>(nullable: true),
                Lba400000DecodedSense = table.Column<string>(nullable: true),
                Lba400000ReadableCluster = table.Column<int>(nullable: false),
                Lba450000Readable = table.Column<bool>(nullable: false),
                Lba450000Data = table.Column<byte[]>(nullable: true),
                Lba450000Sense = table.Column<byte[]>(nullable: true),
                Lba450000DecodedSense = table.Column<string>(nullable: true),
                Lba450000ReadableCluster = table.Column<int>(nullable: false),
                Lba44990PqReadable = table.Column<bool>(nullable: false),
                Lba44990PqData = table.Column<byte[]>(nullable: true),
                Lba44990PqSense = table.Column<byte[]>(nullable: true),
                Lba44990PqDecodedSense = table.Column<string>(nullable: true),
                Lba44990PqReadableCluster = table.Column<int>(nullable: false),
                Lba45000PqReadable = table.Column<bool>(nullable: false),
                Lba45000PqData = table.Column<byte[]>(nullable: true),
                Lba45000PqSense = table.Column<byte[]>(nullable: true),
                Lba45000PqDecodedSense = table.Column<string>(nullable: true),
                Lba45000PqReadableCluster = table.Column<int>(nullable: false),
                Lba50000PqReadable = table.Column<bool>(nullable: false),
                Lba50000PqData = table.Column<byte[]>(nullable: true),
                Lba50000PqSense = table.Column<byte[]>(nullable: true),
                Lba50000PqDecodedSense = table.Column<string>(nullable: true),
                Lba50000PqReadableCluster = table.Column<int>(nullable: false),
                Lba100000PqReadable = table.Column<bool>(nullable: false),
                Lba100000PqData = table.Column<byte[]>(nullable: true),
                Lba100000PqSense = table.Column<byte[]>(nullable: true),
                Lba100000PqDecodedSense = table.Column<string>(nullable: true),
                Lba100000PqReadableCluster = table.Column<int>(nullable: false),
                Lba400000PqReadable = table.Column<bool>(nullable: false),
                Lba400000PqData = table.Column<byte[]>(nullable: true),
                Lba400000PqSense = table.Column<byte[]>(nullable: true),
                Lba400000PqDecodedSense = table.Column<string>(nullable: true),
                Lba400000PqReadableCluster = table.Column<int>(nullable: false),
                Lba450000PqReadable = table.Column<bool>(nullable: false),
                Lba450000PqData = table.Column<byte[]>(nullable: true),
                Lba450000PqSense = table.Column<byte[]>(nullable: true),
                Lba450000PqDecodedSense = table.Column<string>(nullable: true),
                Lba450000PqReadableCluster = table.Column<int>(nullable: false),
                Lba44990RwReadable = table.Column<bool>(nullable: false),
                Lba44990RwData = table.Column<byte[]>(nullable: true),
                Lba44990RwSense = table.Column<byte[]>(nullable: true),
                Lba44990RwDecodedSense = table.Column<string>(nullable: true),
                Lba44990RwReadableCluster = table.Column<int>(nullable: false),
                Lba45000RwReadable = table.Column<bool>(nullable: false),
                Lba45000RwData = table.Column<byte[]>(nullable: true),
                Lba45000RwSense = table.Column<byte[]>(nullable: true),
                Lba45000RwDecodedSense = table.Column<string>(nullable: true),
                Lba45000RwReadableCluster = table.Column<int>(nullable: false),
                Lba50000RwReadable = table.Column<bool>(nullable: false),
                Lba50000RwData = table.Column<byte[]>(nullable: true),
                Lba50000RwSense = table.Column<byte[]>(nullable: true),
                Lba50000RwDecodedSense = table.Column<string>(nullable: true),
                Lba50000RwReadableCluster = table.Column<int>(nullable: false),
                Lba100000RwReadable = table.Column<bool>(nullable: false),
                Lba100000RwData = table.Column<byte[]>(nullable: true),
                Lba100000RwSense = table.Column<byte[]>(nullable: true),
                Lba100000RwDecodedSense = table.Column<string>(nullable: true),
                Lba100000RwReadableCluster = table.Column<int>(nullable: false),
                Lba400000RwReadable = table.Column<bool>(nullable: false),
                Lba400000RwData = table.Column<byte[]>(nullable: true),
                Lba400000RwSense = table.Column<byte[]>(nullable: true),
                Lba400000RwDecodedSense = table.Column<string>(nullable: true),
                Lba400000RwReadableCluster = table.Column<int>(nullable: false),
                Lba450000RwReadable = table.Column<bool>(nullable: false),
                Lba450000RwData = table.Column<byte[]>(nullable: true),
                Lba450000RwSense = table.Column<byte[]>(nullable: true),
                Lba450000RwDecodedSense = table.Column<string>(nullable: true),
                Lba450000RwReadableCluster = table.Column<int>(nullable: false),
                MinimumReadableSectorInHdArea = table.Column<uint>(nullable: false),
                MaximumReadableSectorInHdArea = table.Column<uint>(nullable: false),
                MaximumReadablePqInHdArea = table.Column<byte[]>(nullable: true),
                MaximumReadableRwInHdArea = table.Column<byte[]>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_GdRomSwapDiscCapabilities", x => x.Id);
            });

            migrationBuilder.CreateIndex("IX_Reports_GdRomSwapDiscCapabilitiesId", "Reports",
                                         "GdRomSwapDiscCapabilitiesId");

            migrationBuilder.CreateIndex("IX_Devices_GdRomSwapDiscCapabilitiesId", "Devices",
                                         "GdRomSwapDiscCapabilitiesId");

            migrationBuilder.AddForeignKey("FK_Devices_GdRomSwapDiscCapabilities_GdRomSwapDiscCapabilitiesId",
                                           "Devices", "GdRomSwapDiscCapabilitiesId", "GdRomSwapDiscCapabilities",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Reports_GdRomSwapDiscCapabilities_GdRomSwapDiscCapabilitiesId",
                                           "Reports", "GdRomSwapDiscCapabilitiesId", "GdRomSwapDiscCapabilities",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_Devices_GdRomSwapDiscCapabilities_GdRomSwapDiscCapabilitiesId",
                                            "Devices");

            migrationBuilder.DropForeignKey("FK_Reports_GdRomSwapDiscCapabilities_GdRomSwapDiscCapabilitiesId",
                                            "Reports");

            migrationBuilder.DropTable("GdRomSwapDiscCapabilities");

            migrationBuilder.DropIndex("IX_Reports_GdRomSwapDiscCapabilitiesId", "Reports");

            migrationBuilder.DropIndex("IX_Devices_GdRomSwapDiscCapabilitiesId", "Devices");

            migrationBuilder.DropColumn("GdRomSwapDiscCapabilitiesId", "Reports");

            migrationBuilder.DropColumn("GdRomSwapDiscCapabilitiesId", "Devices");
        }
    }
}