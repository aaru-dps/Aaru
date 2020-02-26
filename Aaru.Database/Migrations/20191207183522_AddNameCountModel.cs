using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscImageChef.Database.Migrations
{
    public partial class AddNameCountModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Versions",
                newName: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Versions",
                newName: "Value");
        }
    }
}
