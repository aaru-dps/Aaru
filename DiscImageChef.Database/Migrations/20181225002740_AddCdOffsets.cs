using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscImageChef.Database.Migrations
{
    public partial class AddCdOffsets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("CdOffsets",
                                         table => new
                                         {
                                             Manufacturer = table.Column<string>(nullable: true),
                                             Model        = table.Column<string>(nullable: true),
                                             Offset       = table.Column<short>(nullable: false),
                                             Submissions  = table.Column<int>(nullable: false),
                                             Agreement    = table.Column<float>(nullable: false),
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             AddedWhen    = table.Column<DateTime>(nullable: false),
                                             ModifiedWhen = table.Column<DateTime>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_CdOffsets", x => x.Id); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("CdOffsets");
        }
    }
}