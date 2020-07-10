using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class MakeCanReadGdRomUsingSwapDiscNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AlterColumn<bool>("CanReadGdRomUsingSwapDisc", "Devices", nullable: true,
                                               oldClrType: typeof(bool), oldType: "INTEGER");

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AlterColumn<bool>("CanReadGdRomUsingSwapDisc", "Devices", "INTEGER", nullable: false,
                                               oldClrType: typeof(bool), oldNullable: true);
    }
}