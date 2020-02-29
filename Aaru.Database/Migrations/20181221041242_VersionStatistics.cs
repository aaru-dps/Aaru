using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class VersionStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable("Versions",
                                                                                                      table => new
                                                                                                      {
                                                                                                          Id = table.
                                                                                                               Column<
                                                                                                                   int
                                                                                                               >().
                                                                                                               Annotation("Sqlite:Autoincrement",
                                                                                                                          true),
                                                                                                          Value = table.
                                                                                                              Column<
                                                                                                                  string
                                                                                                              >(nullable
                                                                                                                : true),
                                                                                                          Synchronized =
                                                                                                              table.
                                                                                                                  Column
                                                                                                                  <bool
                                                                                                                  >()
                                                                                                      }, constraints:
                                                                                                      table =>
                                                                                                      {
                                                                                                          table.
                                                                                                              PrimaryKey("PK_Versions",
                                                                                                                         x =>
                                                                                                                             x.
                                                                                                                                 Id);
                                                                                                      });

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Versions");
    }
}