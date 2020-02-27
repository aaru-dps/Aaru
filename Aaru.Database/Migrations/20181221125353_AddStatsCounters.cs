using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class AddStatsCounters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>("Count", "Versions", nullable: false, defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>("Count", "Partitions", nullable: false, defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>("Count", "OperatingSystems", nullable: false, defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>("Count", "Medias", nullable: false, defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>("Count", "MediaFormats", nullable: false, defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>("Count", "Filters", nullable: false, defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>("Count", "Filesystems", nullable: false, defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>("Count", "Commands", nullable: false, defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Count", "Versions");

            migrationBuilder.DropColumn("Count", "Partitions");

            migrationBuilder.DropColumn("Count", "OperatingSystems");

            migrationBuilder.DropColumn("Count", "Medias");

            migrationBuilder.DropColumn("Count", "MediaFormats");

            migrationBuilder.DropColumn("Count", "Filters");

            migrationBuilder.DropColumn("Count", "Filesystems");

            migrationBuilder.DropColumn("Count", "Commands");
        }
    }
}