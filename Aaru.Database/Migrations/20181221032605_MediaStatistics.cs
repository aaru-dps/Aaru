using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscImageChef.Database.Migrations
{
    public partial class MediaStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Medias",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Type         = table.Column<string>(nullable: true),
                                             Real         = table.Column<bool>(nullable: false),
                                             Synchronized = table.Column<bool>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_Medias", x => x.Id); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Medias");
        }
    }
}