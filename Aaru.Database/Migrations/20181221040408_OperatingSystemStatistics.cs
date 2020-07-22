using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class OperatingSystemStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.CreateTable("OperatingSystems", table => new
            {
                Id           = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                Name         = table.Column<string>(nullable: true),
                Version      = table.Column<string>(nullable: true),
                Synchronized = table.Column<bool>()
            }, constraints: table => table.PrimaryKey("PK_OperatingSystems", x => x.Id));

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable("OperatingSystems");
    }
}