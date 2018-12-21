using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscImageChef.Database.Migrations
{
    public partial class NameValueStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Commands",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Name         = table.Column<string>(nullable: true),
                                             Synchronized = table.Column<bool>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_Commands", x => x.Id); });

            migrationBuilder.CreateTable("Filesystems",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Name         = table.Column<string>(nullable: true),
                                             Synchronized = table.Column<bool>(nullable: false)
                                         },
                                         constraints: table => { table.PrimaryKey("PK_Filesystems", x => x.Id); });

            migrationBuilder.CreateTable("Filters",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Name         = table.Column<string>(nullable: true),
                                             Synchronized = table.Column<bool>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_Filters", x => x.Id); });

            migrationBuilder.CreateTable("MediaFormats",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Name         = table.Column<string>(nullable: true),
                                             Synchronized = table.Column<bool>(nullable: false)
                                         },
                                         constraints: table => { table.PrimaryKey("PK_MediaFormats", x => x.Id); });

            migrationBuilder.CreateTable("Partitions",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Name         = table.Column<string>(nullable: true),
                                             Synchronized = table.Column<bool>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_Partitions", x => x.Id); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Commands");

            migrationBuilder.DropTable("Filesystems");

            migrationBuilder.DropTable("Filters");

            migrationBuilder.DropTable("MediaFormats");

            migrationBuilder.DropTable("Partitions");
        }
    }
}