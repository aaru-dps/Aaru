using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class SeenDevicesStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable("SeenDevices",
                                                                                                      table => new
                                                                                                      {
                                                                                                          Id = table.
                                                                                                               Column<
                                                                                                                   int
                                                                                                               >().
                                                                                                               Annotation("Sqlite:Autoincrement",
                                                                                                                          true),
                                                                                                          Manufacturer =
                                                                                                              table.
                                                                                                                  Column
                                                                                                                  <string
                                                                                                                  >(nullable
                                                                                                                    : true),
                                                                                                          Model = table.
                                                                                                              Column<
                                                                                                                  string
                                                                                                              >(nullable
                                                                                                                : true),
                                                                                                          Revision =
                                                                                                              table.
                                                                                                                  Column
                                                                                                                  <string
                                                                                                                  >(nullable
                                                                                                                    : true),
                                                                                                          Bus = table.
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
                                                                                                              PrimaryKey("PK_SeenDevices",
                                                                                                                         x =>
                                                                                                                             x.
                                                                                                                                 Id);
                                                                                                      });

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("SeenDevices");
    }
}