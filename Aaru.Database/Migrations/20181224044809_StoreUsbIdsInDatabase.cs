using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class StoreUsbIdsInDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("UsbVendors",
                                         table => new
                                         {
                                             Id           = table.Column<ushort>(nullable: false),
                                             Vendor       = table.Column<string>(nullable: true),
                                             AddedWhen    = table.Column<DateTime>(nullable: false),
                                             ModifiedWhen = table.Column<DateTime>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_UsbVendors", x => x.Id); });

            migrationBuilder.CreateTable("UsbProducts",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             ProductId    = table.Column<ushort>(nullable: false),
                                             Product      = table.Column<string>(nullable: true),
                                             AddedWhen    = table.Column<DateTime>(nullable: false),
                                             ModifiedWhen = table.Column<DateTime>(nullable: false),
                                             VendorId     = table.Column<ushort>(nullable: false)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_UsbProducts", x => x.Id);
                                             table.ForeignKey("FK_UsbProducts_UsbVendors_VendorId", x => x.VendorId,
                                                              "UsbVendors", "Id", onDelete: ReferentialAction.Cascade);
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