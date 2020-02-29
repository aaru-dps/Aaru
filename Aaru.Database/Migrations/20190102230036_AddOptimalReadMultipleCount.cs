using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class AddOptimalReadMultipleCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AddColumn<int>("OptimalMultipleSectorsRead", "Devices", nullable: false, defaultValue: 0);

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropColumn("OptimalMultipleSectorsRead", "Devices");
    }
}