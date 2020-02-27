using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    // TODO: Find how to permanently create indexes
    public partial class StoreMmcGetConfigurationResponse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_UsbVendors_ModifiedWhen", "UsbVendors");

            migrationBuilder.DropIndex("IX_UsbProducts_ModifiedWhen", "UsbProducts");

            migrationBuilder.DropIndex("IX_UsbProducts_ProductId", "UsbProducts");

            migrationBuilder.AddColumn<byte[]>("BinaryData", "MmcFeatures", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("BinaryData", "MmcFeatures");

            migrationBuilder.CreateIndex("IX_UsbVendors_ModifiedWhen", "UsbVendors", "ModifiedWhen");

            migrationBuilder.CreateIndex("IX_UsbProducts_ModifiedWhen", "UsbProducts", "ModifiedWhen");

            migrationBuilder.CreateIndex("IX_UsbProducts_ProductId", "UsbProducts", "ProductId");
        }
    }
}