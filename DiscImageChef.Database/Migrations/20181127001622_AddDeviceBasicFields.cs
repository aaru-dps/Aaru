using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscImageChef.Database.Migrations
{
    public partial class AddDeviceBasicFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "Reports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Reports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Revision",
                table: "Reports",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Reports",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Revision",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Reports");
        }
    }
}
