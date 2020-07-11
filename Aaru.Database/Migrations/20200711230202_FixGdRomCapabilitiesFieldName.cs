using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class FixGdRomCapabilitiesFieldName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Lba400000AudioAudioData", "GdRomSwapDiscCapabilities");

            migrationBuilder.AddColumn<byte[]>("Lba400000AudioData", "GdRomSwapDiscCapabilities", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Lba400000AudioData", "GdRomSwapDiscCapabilities");

            migrationBuilder.AddColumn<byte[]>("Lba400000AudioAudioData", "GdRomSwapDiscCapabilities", "BLOB",
                                               nullable: true);
        }
    }
}