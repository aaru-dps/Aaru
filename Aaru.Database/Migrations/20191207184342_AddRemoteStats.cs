using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class AddRemoteStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("RemoteApplications", table => new
            {
                Id           = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                Name         = table.Column<string>(nullable: true),
                Version      = table.Column<string>(nullable: true),
                Synchronized = table.Column<bool>(),
                Count        = table.Column<ulong>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_RemoteApplications", x => x.Id);
            });

            migrationBuilder.CreateTable("RemoteArchitectures", table => new
            {
                Id           = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                Name         = table.Column<string>(nullable: true),
                Synchronized = table.Column<bool>(),
                Count        = table.Column<ulong>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_RemoteArchitectures", x => x.Id);
            });

            migrationBuilder.CreateTable("RemoteOperatingSystems", table => new
            {
                Id           = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                Name         = table.Column<string>(nullable: true),
                Version      = table.Column<string>(nullable: true),
                Synchronized = table.Column<bool>(),
                Count        = table.Column<ulong>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_RemoteOperatingSystems", x => x.Id);
            });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("RemoteApplications");

            migrationBuilder.DropTable("RemoteArchitectures");

            migrationBuilder.DropTable("RemoteOperatingSystems");
        }
    }
}