using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class AddMultisessionLeadTest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>("CanReadingIntersessionLeadIn", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<bool>("CanReadingIntersessionLeadOut", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("IntersessionLeadInData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("IntersessionLeadOutData", "TestedMedia", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("CanReadingIntersessionLeadIn", "TestedMedia");

            migrationBuilder.DropColumn("CanReadingIntersessionLeadOut", "TestedMedia");

            migrationBuilder.DropColumn("IntersessionLeadInData", "TestedMedia");

            migrationBuilder.DropColumn("IntersessionLeadOutData", "TestedMedia");
        }
    }
}