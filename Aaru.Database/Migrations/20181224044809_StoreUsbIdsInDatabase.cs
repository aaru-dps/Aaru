using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class StoreUsbIdsInDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("UsbVendors", table => new
            {
                Id        = table.Column<ushort>(), Vendor         = table.Column<string>(nullable: true),
                AddedWhen = table.Column<DateTime>(), ModifiedWhen = table.Column<DateTime>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_UsbVendors", x => x.Id);
            });

            migrationBuilder.CreateTable("UsbProducts", table => new
            {
                Id           = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                ProductId    = table.Column<ushort>(),
                Product      = table.Column<string>(nullable: true),
                AddedWhen    = table.Column<DateTime>(),
                ModifiedWhen = table.Column<DateTime>(),
                VendorId     = table.Column<ushort>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_UsbProducts", x => x.Id);

                table.ForeignKey("FK_UsbProducts_UsbVendors_VendorId", x => x.VendorId, "UsbVendors", "Id",
                                 onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_UsbProducts_VendorId", "UsbProducts", "VendorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("UsbProducts");

            migrationBuilder.DropTable("UsbVendors");
        }
    }
}