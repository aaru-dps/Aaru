using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscImageChef.Database.Migrations
{
    public partial class FixUsbIdsAndIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex("IX_UsbProducts_ProductId",    "UsbProducts", "ProductId");
            migrationBuilder.CreateIndex("IX_UsbProducts_ModifiedWhen", "UsbProducts", "ModifiedWhen");
            migrationBuilder.CreateIndex("IX_UsbVendors_ModifiedWhen",  "UsbVendors",  "ModifiedWhen");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_UsbProducts_ProductId",    "UsbProducts");
            migrationBuilder.DropIndex("IX_UsbProducts_ModifiedWhen", "UsbProducts");
            migrationBuilder.DropIndex("IX_UsbVendors_ModifiedWhen",  "UsbVendors");
        }
    }
}