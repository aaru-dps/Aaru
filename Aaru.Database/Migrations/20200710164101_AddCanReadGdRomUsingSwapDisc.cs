using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class AddCanReadGdRomUsingSwapDisc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AddColumn<bool>("CanReadGdRomUsingSwapDisc", "Devices", nullable: false,
                                             defaultValue: false);

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropColumn("CanReadGdRomUsingSwapDisc", "Devices");
    }
}