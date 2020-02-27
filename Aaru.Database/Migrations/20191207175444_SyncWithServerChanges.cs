using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class SyncWithServerChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>("CanReadCdScrambled", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadCdScrambledData", "TestedMedia", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("CanReadCdScrambled", "TestedMedia");

            migrationBuilder.DropColumn("ReadCdScrambledData", "TestedMedia");
        }
    }
}