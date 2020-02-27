using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class FixUnsignedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>("ProductIDSql", "Usb", nullable: false, defaultValue: (short)0);

            migrationBuilder.AddColumn<short>("VendorIDSql", "Usb", nullable: false, defaultValue: (short)0);

            migrationBuilder.AddColumn<int>("BlockSizeSql", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<long>("BlocksSql", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<long>("LBA48SectorsSql", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<int>("LBASectorsSql", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<short>("LogicalAlignmentSql", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<int>("LongBlockSizeSql", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<short>("NominalRotationRateSql", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<int>("PhysicalBlockSizeSql", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<short>("UnformattedBPSSql", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<short>("UnformattedBPTSql", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<int>("BitsPerMmSql", "SupportedDensity", nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<int>("CapacitySql", "SupportedDensity", nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<short>("TracksSql", "SupportedDensity", nullable: false, defaultValue: (short)0);

            migrationBuilder.AddColumn<short>("WidthSql", "SupportedDensity", nullable: false, defaultValue: (short)0);

            migrationBuilder.AddColumn<short>("LengthSql", "SscSupportedMedia", nullable: false,
                                              defaultValue: (short)0);

            migrationBuilder.AddColumn<short>("WidthSql", "SscSupportedMedia", nullable: false, defaultValue: (short)0);

            migrationBuilder.AddColumn<int>("MaxBlockLengthSql", "Ssc", nullable: true);

            migrationBuilder.AddColumn<int>("MinBlockLengthSql", "Ssc", nullable: true);

            migrationBuilder.AddColumn<short>("CardCodeSql", "Pcmcia", nullable: true);

            migrationBuilder.AddColumn<short>("ManufacturerCodeSql", "Pcmcia", nullable: true);

            migrationBuilder.AddColumn<short>("BlocksPerReadableUnitSql", "MmcFeatures", nullable: true);

            migrationBuilder.AddColumn<int>("LogicalBlockSizeSql", "MmcFeatures", nullable: true);

            migrationBuilder.AddColumn<int>("PhysicalInterfaceStandardNumberSql", "MmcFeatures", nullable: true);

            migrationBuilder.AddColumn<short>("VolumeLevelsSql", "MmcFeatures", nullable: true);

            migrationBuilder.AddColumn<int>("ProductIDSql", "FireWire", nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<int>("VendorIDSql", "FireWire", nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<short>("CylindersSql", "Chs", nullable: false, defaultValue: (short)0);

            migrationBuilder.AddColumn<short>("HeadsSql", "Chs", nullable: false, defaultValue: (short)0);

            migrationBuilder.AddColumn<short>("SectorsSql", "Chs", nullable: false, defaultValue: (short)0);

            migrationBuilder.AddColumn<int>("BlockLengthSql", "BlockDescriptor", nullable: true);

            migrationBuilder.AddColumn<long>("BlocksSql", "BlockDescriptor", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("ProductIDSql", "Usb");

            migrationBuilder.DropColumn("VendorIDSql", "Usb");

            migrationBuilder.DropColumn("BlockSizeSql", "TestedMedia");

            migrationBuilder.DropColumn("BlocksSql", "TestedMedia");

            migrationBuilder.DropColumn("LBA48SectorsSql", "TestedMedia");

            migrationBuilder.DropColumn("LBASectorsSql", "TestedMedia");

            migrationBuilder.DropColumn("LogicalAlignmentSql", "TestedMedia");

            migrationBuilder.DropColumn("LongBlockSizeSql", "TestedMedia");

            migrationBuilder.DropColumn("NominalRotationRateSql", "TestedMedia");

            migrationBuilder.DropColumn("PhysicalBlockSizeSql", "TestedMedia");

            migrationBuilder.DropColumn("UnformattedBPSSql", "TestedMedia");

            migrationBuilder.DropColumn("UnformattedBPTSql", "TestedMedia");

            migrationBuilder.DropColumn("BitsPerMmSql", "SupportedDensity");

            migrationBuilder.DropColumn("CapacitySql", "SupportedDensity");

            migrationBuilder.DropColumn("TracksSql", "SupportedDensity");

            migrationBuilder.DropColumn("WidthSql", "SupportedDensity");

            migrationBuilder.DropColumn("LengthSql", "SscSupportedMedia");

            migrationBuilder.DropColumn("WidthSql", "SscSupportedMedia");

            migrationBuilder.DropColumn("MaxBlockLengthSql", "Ssc");

            migrationBuilder.DropColumn("MinBlockLengthSql", "Ssc");

            migrationBuilder.DropColumn("CardCodeSql", "Pcmcia");

            migrationBuilder.DropColumn("ManufacturerCodeSql", "Pcmcia");

            migrationBuilder.DropColumn("BlocksPerReadableUnitSql", "MmcFeatures");

            migrationBuilder.DropColumn("LogicalBlockSizeSql", "MmcFeatures");

            migrationBuilder.DropColumn("PhysicalInterfaceStandardNumberSql", "MmcFeatures");

            migrationBuilder.DropColumn("VolumeLevelsSql", "MmcFeatures");

            migrationBuilder.DropColumn("ProductIDSql", "FireWire");

            migrationBuilder.DropColumn("VendorIDSql", "FireWire");

            migrationBuilder.DropColumn("CylindersSql", "Chs");

            migrationBuilder.DropColumn("HeadsSql", "Chs");

            migrationBuilder.DropColumn("SectorsSql", "Chs");

            migrationBuilder.DropColumn("BlockLengthSql", "BlockDescriptor");

            migrationBuilder.DropColumn("BlocksSql", "BlockDescriptor");
        }
    }
}