using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class AddNameCountModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.RenameColumn("Value", "Versions", "Name");

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.RenameColumn("Name", "Versions", "Value");
    }
}