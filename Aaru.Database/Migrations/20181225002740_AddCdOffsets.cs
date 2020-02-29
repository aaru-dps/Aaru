using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class AddCdOffsets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable("CdOffsets",
                                                                                                      table => new
                                                                                                      {
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
                                                                                                          Offset =
                                                                                                              table.
                                                                                                                  Column
                                                                                                                  <short
                                                                                                                  >(),
                                                                                                          Submissions =
                                                                                                              table.
                                                                                                                  Column
                                                                                                                  <int
                                                                                                                  >(),
                                                                                                          Agreement =
                                                                                                              table.
                                                                                                                  Column
                                                                                                                  <float
                                                                                                                  >(),
                                                                                                          Id = table.
                                                                                                               Column<
                                                                                                                   int
                                                                                                               >().
                                                                                                               Annotation("Sqlite:Autoincrement",
                                                                                                                          true),
                                                                                                          AddedWhen =
                                                                                                              table.
                                                                                                                  Column
                                                                                                                  <DateTime
                                                                                                                  >(),
                                                                                                          ModifiedWhen =
                                                                                                              table.
                                                                                                                  Column
                                                                                                                  <DateTime
                                                                                                                  >()
                                                                                                      }, constraints:
                                                                                                      table =>
                                                                                                      {
                                                                                                          table.
                                                                                                              PrimaryKey("PK_CdOffsets",
                                                                                                                         x =>
                                                                                                                             x.
                                                                                                                                 Id);
                                                                                                      });

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("CdOffsets");
    }
}