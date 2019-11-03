using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscImageChef.Database.Migrations
{
    public partial class MakeFieldsUnsigned : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TODO: SQLite does not support dropping columns or foreign keys so just left them be
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Read above
        }
    }
}