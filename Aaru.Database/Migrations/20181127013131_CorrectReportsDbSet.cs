using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class CorrectReportsDbSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Reports");

            migrationBuilder.CreateTable("Reports",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<int>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             USBId            = table.Column<int>(nullable: true),
                                             FireWireId       = table.Column<int>(nullable: true),
                                             PCMCIAId         = table.Column<int>(nullable: true),
                                             CompactFlash     = table.Column<bool>(nullable: false),
                                             ATAId            = table.Column<int>(nullable: true),
                                             ATAPIId          = table.Column<int>(nullable: true),
                                             SCSIId           = table.Column<int>(nullable: true),
                                             MultiMediaCardId = table.Column<int>(nullable: true),
                                             SecureDigitalId  = table.Column<int>(nullable: true),
                                             Manufacturer     = table.Column<string>(nullable: true),
                                             Model            = table.Column<string>(nullable: true),
                                             Revision         = table.Column<string>(nullable: true),
                                             Type             = table.Column<int>(nullable: false)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_Devices", x => x.Id);
                                             table.ForeignKey("FK_Reports_Ata_ATAId", x => x.ATAId, "Ata", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Reports_Ata_ATAPIId", x => x.ATAPIId, "Ata", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Reports_FireWire_FireWireId", x => x.FireWireId,
                                                              "FireWire", "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Reports_MmcSd_MultiMediaCardId",
                                                              x => x.MultiMediaCardId, "MmcSd", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Reports_Pcmcia_PCMCIAId", x => x.PCMCIAId, "Pcmcia",
                                                              "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Reports_Scsi_SCSIId", x => x.SCSIId, "Scsi", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Reports_MmcSd_SecureDigitalId",
                                                              x => x.SecureDigitalId, "MmcSd", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Reports_Usb_USBId", x => x.USBId, "Usb", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.AddColumn<DateTime>("Created", "Reports", nullable: false,
                                                 defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0,
                                                                            DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>("Uploaded", "Reports", nullable: false, defaultValue: false);

            migrationBuilder.CreateTable("Devices",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<int>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             USBId            = table.Column<int>(nullable: true),
                                             FireWireId       = table.Column<int>(nullable: true),
                                             PCMCIAId         = table.Column<int>(nullable: true),
                                             CompactFlash     = table.Column<bool>(nullable: false),
                                             ATAId            = table.Column<int>(nullable: true),
                                             ATAPIId          = table.Column<int>(nullable: true),
                                             SCSIId           = table.Column<int>(nullable: true),
                                             MultiMediaCardId = table.Column<int>(nullable: true),
                                             SecureDigitalId  = table.Column<int>(nullable: true),
                                             Manufacturer     = table.Column<string>(nullable: true),
                                             Model            = table.Column<string>(nullable: true),
                                             Revision         = table.Column<string>(nullable: true),
                                             Type             = table.Column<int>(nullable: false),
                                             LastSynchronized = table.Column<DateTime>(nullable: false)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_Devices", x => x.Id);
                                             table.ForeignKey("FK_Devices_Ata_ATAId", x => x.ATAId, "Ata", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Devices_Ata_ATAPIId", x => x.ATAPIId, "Ata", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Devices_FireWire_FireWireId", x => x.FireWireId,
                                                              "FireWire", "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Devices_MmcSd_MultiMediaCardId",
                                                              x => x.MultiMediaCardId, "MmcSd", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Devices_Pcmcia_PCMCIAId", x => x.PCMCIAId, "Pcmcia",
                                                              "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Devices_Scsi_SCSIId", x => x.SCSIId, "Scsi", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Devices_MmcSd_SecureDigitalId",
                                                              x => x.SecureDigitalId, "MmcSd", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Devices_Usb_USBId", x => x.USBId, "Usb", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateIndex("IX_Devices_ATAId", "Devices", "ATAId");

            migrationBuilder.CreateIndex("IX_Devices_ATAPIId", "Devices", "ATAPIId");

            migrationBuilder.CreateIndex("IX_Devices_FireWireId", "Devices", "FireWireId");

            migrationBuilder.CreateIndex("IX_Devices_MultiMediaCardId", "Devices", "MultiMediaCardId");

            migrationBuilder.CreateIndex("IX_Devices_PCMCIAId", "Devices", "PCMCIAId");

            migrationBuilder.CreateIndex("IX_Devices_SCSIId", "Devices", "SCSIId");

            migrationBuilder.CreateIndex("IX_Devices_SecureDigitalId", "Devices", "SecureDigitalId");

            migrationBuilder.CreateIndex("IX_Devices_USBId", "Devices", "USBId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Devices");

            migrationBuilder.DropColumn("Created", "Reports");

            migrationBuilder.DropColumn("Uploaded", "Reports");

            migrationBuilder.AddColumn<string>("Discriminator", "Reports", nullable: false, defaultValue: "");

            migrationBuilder.AddColumn<DateTime>("LastSynchronized", "Reports", nullable: true);
        }
    }
}