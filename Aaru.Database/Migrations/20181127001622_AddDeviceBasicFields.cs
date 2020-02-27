using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class AddDeviceBasicFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>("Manufacturer", "Reports", nullable: true);

            migrationBuilder.AddColumn<string>("Model", "Reports", nullable: true);

            migrationBuilder.AddColumn<string>("Revision", "Reports", nullable: true);

            migrationBuilder.AddColumn<int>("Type", "Reports", nullable: false, defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Manufacturer", "Reports");

            migrationBuilder.DropColumn("Model", "Reports");

            migrationBuilder.DropColumn("Revision", "Reports");

            migrationBuilder.DropColumn("Type", "Reports");
        }
    }
}