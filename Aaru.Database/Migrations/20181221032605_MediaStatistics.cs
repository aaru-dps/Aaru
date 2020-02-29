using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class MediaStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable("Medias", table =>
                                                                                                          new
                                                                                                          {
                                                                                                              Id =
                                                                                                                  table.
                                                                                                                      Column
                                                                                                                      <int
                                                                                                                      >().
                                                                                                                      Annotation("Sqlite:Autoincrement",
                                                                                                                                 true),
                                                                                                              Type =
                                                                                                                  table.
                                                                                                                      Column
                                                                                                                      <string
                                                                                                                      >(nullable
                                                                                                                        : true),
                                                                                                              Real =
                                                                                                                  table.
                                                                                                                      Column
                                                                                                                      <bool
                                                                                                                      >(),
                                                                                                              Synchronized
                                                                                                                  = table.
                                                                                                                      Column
                                                                                                                      <bool
                                                                                                                      >()
                                                                                                          }, constraints
                                                                                                      : table =>
                                                                                                      {
                                                                                                          table.
                                                                                                              PrimaryKey("PK_Medias",
                                                                                                                         x =>
                                                                                                                             x.
                                                                                                                                 Id);
                                                                                                      });

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Medias");
    }
}