using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class AddFieldsForF1hCommand06hSubcommand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>("CanReadF1_06", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<bool>("CanReadF1_06LeadOut", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadF1_06Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadF1_06LeadOutData", "TestedMedia", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("CanReadF1_06", "TestedMedia");

            migrationBuilder.DropColumn("CanReadF1_06LeadOut", "TestedMedia");

            migrationBuilder.DropColumn("ReadF1_06Data", "TestedMedia");

            migrationBuilder.DropColumn("ReadF1_06LeadOutData", "TestedMedia");
        }
    }
}