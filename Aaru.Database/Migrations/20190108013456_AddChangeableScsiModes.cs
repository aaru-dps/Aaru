using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscImageChef.Database.Migrations
{
    public partial class AddChangeableScsiModes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>("ModeSense10ChangeableData", "Scsi", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ModeSense10CurrentData", "Scsi", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ModeSense6ChangeableData", "Scsi", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ModeSense6CurrentData", "Scsi", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("ModeSense10ChangeableData", "Scsi");

            migrationBuilder.DropColumn("ModeSense10CurrentData", "Scsi");

            migrationBuilder.DropColumn("ModeSense6ChangeableData", "Scsi");

            migrationBuilder.DropColumn("ModeSense6CurrentData", "Scsi");
        }
    }
}