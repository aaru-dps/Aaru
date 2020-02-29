using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class IdForDensityCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("DensityCode", newName: "DensityCode_old");

            migrationBuilder.CreateTable("DensityCode", table => new
            {
                Code                = table.Column<int>(nullable: false, defaultValue: 0),
                SscSupportedMediaId = table.Column<int>(nullable: true),
                Id                  = table.Column<int>().Annotation("Sqlite:Autoincrement", true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_DensityCode", x => x.Id);

                table.ForeignKey("FK_DensityCode_SscSupportedMedia_SscSupportedMediaId", x => x.SscSupportedMediaId,
                                 "SscSupportedMedia", "Id", onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.
                Sql("INSERT INTO DensityCode (Code, SscSupportedMediaId) SELECT Code, SscSupportedMediaId FROM DensityCode_old");

            migrationBuilder.DropTable("DensityCode_old");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("DensityCode", newName: "DensityCode_old");

            migrationBuilder.CreateTable("DensityCode", table => new
            {
                Code                = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                SscSupportedMediaId = table.Column<int>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_DensityCode", x => x.Code);

                table.ForeignKey("FK_DensityCode_SscSupportedMedia_SscSupportedMediaId", x => x.SscSupportedMediaId,
                                 "SscSupportedMedia", "Id", onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.
                Sql("INSERT INTO DensityCode (Code, SscSupportedMediaId) SELECT Code, SscSupportedMediaId FROM DensityCode_old");

            migrationBuilder.DropTable("DensityCode_old");
        }
    }
}