using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aaru.Database.Migrations
{
    public partial class AddNesHeaders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NesHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Sha256 = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    NametableMirroring = table.Column<bool>(type: "INTEGER", nullable: false),
                    BatteryPresent = table.Column<bool>(type: "INTEGER", nullable: false),
                    FourScreenMode = table.Column<bool>(type: "INTEGER", nullable: false),
                    Mapper = table.Column<ushort>(type: "INTEGER", nullable: false),
                    ConsoleType = table.Column<byte>(type: "INTEGER", nullable: false),
                    Submapper = table.Column<byte>(type: "INTEGER", nullable: false),
                    TimingMode = table.Column<byte>(type: "INTEGER", nullable: false),
                    VsPpuType = table.Column<byte>(type: "INTEGER", nullable: false),
                    VsHardwareType = table.Column<byte>(type: "INTEGER", nullable: false),
                    ExtendedConsoleType = table.Column<byte>(type: "INTEGER", nullable: false),
                    DefaultExpansionDevice = table.Column<byte>(type: "INTEGER", nullable: false),
                    AddedWhen = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedWhen = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NesHeaders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NesHeaders_ModifiedWhen",
                table: "NesHeaders",
                column: "ModifiedWhen");

            migrationBuilder.CreateIndex(
                name: "IX_NesHeaders_Sha256",
                table: "NesHeaders",
                column: "Sha256");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NesHeaders");
        }
    }
}
