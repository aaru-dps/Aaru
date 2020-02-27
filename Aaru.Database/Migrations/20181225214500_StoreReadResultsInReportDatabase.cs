using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class StoreReadResultsInReportDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>("AdipData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("AtipData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("BluBcaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("BluDdsData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("BluDiData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("BluPacData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("BluSaiData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("C2PointersData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("CmiData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("CorrectedSubchannelData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("CorrectedSubchannelWithC2Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("DcbData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("DmiData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("DvdAacsData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("DvdBcaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("DvdDdsData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("DvdLayerData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("DvdSaiData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("EmbossedPfiData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("FullTocData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("HLDTSTReadRawDVDData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("HdCmiData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("LeadInData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("LeadOutData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("NecReadCddaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("PQSubchannelData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("PQSubchannelWithC2Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("PfiData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("PioneerReadCddaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("PioneerReadCddaMsfData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("PlextorReadCddaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("PlextorReadRawDVDData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("PmaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("PriData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("RWSubchannelData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("RWSubchannelWithC2Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Read10Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Read12Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Read16Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Read6Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadCdData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadCdFullData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadCdMsfData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadCdMsfFullData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadDmaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadDmaLba48Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadDmaLbaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadDmaRetryData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadDmaRetryLbaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadLba48Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadLbaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadLong10Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadLong16Data", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadLongData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadLongLbaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadLongRetryData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadLongRetryLbaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadRetryLbaData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadSectorsData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("ReadSectorsRetryData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("TocData", "TestedMedia", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Track1PregapData", "TestedMedia", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("AdipData", "TestedMedia");

            migrationBuilder.DropColumn("AtipData", "TestedMedia");

            migrationBuilder.DropColumn("BluBcaData", "TestedMedia");

            migrationBuilder.DropColumn("BluDdsData", "TestedMedia");

            migrationBuilder.DropColumn("BluDiData", "TestedMedia");

            migrationBuilder.DropColumn("BluPacData", "TestedMedia");

            migrationBuilder.DropColumn("BluSaiData", "TestedMedia");

            migrationBuilder.DropColumn("C2PointersData", "TestedMedia");

            migrationBuilder.DropColumn("CmiData", "TestedMedia");

            migrationBuilder.DropColumn("CorrectedSubchannelData", "TestedMedia");

            migrationBuilder.DropColumn("CorrectedSubchannelWithC2Data", "TestedMedia");

            migrationBuilder.DropColumn("DcbData", "TestedMedia");

            migrationBuilder.DropColumn("DmiData", "TestedMedia");

            migrationBuilder.DropColumn("DvdAacsData", "TestedMedia");

            migrationBuilder.DropColumn("DvdBcaData", "TestedMedia");

            migrationBuilder.DropColumn("DvdDdsData", "TestedMedia");

            migrationBuilder.DropColumn("DvdLayerData", "TestedMedia");

            migrationBuilder.DropColumn("DvdSaiData", "TestedMedia");

            migrationBuilder.DropColumn("EmbossedPfiData", "TestedMedia");

            migrationBuilder.DropColumn("FullTocData", "TestedMedia");

            migrationBuilder.DropColumn("HLDTSTReadRawDVDData", "TestedMedia");

            migrationBuilder.DropColumn("HdCmiData", "TestedMedia");

            migrationBuilder.DropColumn("LeadInData", "TestedMedia");

            migrationBuilder.DropColumn("LeadOutData", "TestedMedia");

            migrationBuilder.DropColumn("NecReadCddaData", "TestedMedia");

            migrationBuilder.DropColumn("PQSubchannelData", "TestedMedia");

            migrationBuilder.DropColumn("PQSubchannelWithC2Data", "TestedMedia");

            migrationBuilder.DropColumn("PfiData", "TestedMedia");

            migrationBuilder.DropColumn("PioneerReadCddaData", "TestedMedia");

            migrationBuilder.DropColumn("PioneerReadCddaMsfData", "TestedMedia");

            migrationBuilder.DropColumn("PlextorReadCddaData", "TestedMedia");

            migrationBuilder.DropColumn("PlextorReadRawDVDData", "TestedMedia");

            migrationBuilder.DropColumn("PmaData", "TestedMedia");

            migrationBuilder.DropColumn("PriData", "TestedMedia");

            migrationBuilder.DropColumn("RWSubchannelData", "TestedMedia");

            migrationBuilder.DropColumn("RWSubchannelWithC2Data", "TestedMedia");

            migrationBuilder.DropColumn("Read10Data", "TestedMedia");

            migrationBuilder.DropColumn("Read12Data", "TestedMedia");

            migrationBuilder.DropColumn("Read16Data", "TestedMedia");

            migrationBuilder.DropColumn("Read6Data", "TestedMedia");

            migrationBuilder.DropColumn("ReadCdData", "TestedMedia");

            migrationBuilder.DropColumn("ReadCdFullData", "TestedMedia");

            migrationBuilder.DropColumn("ReadCdMsfData", "TestedMedia");

            migrationBuilder.DropColumn("ReadCdMsfFullData", "TestedMedia");

            migrationBuilder.DropColumn("ReadDmaData", "TestedMedia");

            migrationBuilder.DropColumn("ReadDmaLba48Data", "TestedMedia");

            migrationBuilder.DropColumn("ReadDmaLbaData", "TestedMedia");

            migrationBuilder.DropColumn("ReadDmaRetryData", "TestedMedia");

            migrationBuilder.DropColumn("ReadDmaRetryLbaData", "TestedMedia");

            migrationBuilder.DropColumn("ReadLba48Data", "TestedMedia");

            migrationBuilder.DropColumn("ReadLbaData", "TestedMedia");

            migrationBuilder.DropColumn("ReadLong10Data", "TestedMedia");

            migrationBuilder.DropColumn("ReadLong16Data", "TestedMedia");

            migrationBuilder.DropColumn("ReadLongData", "TestedMedia");

            migrationBuilder.DropColumn("ReadLongLbaData", "TestedMedia");

            migrationBuilder.DropColumn("ReadLongRetryData", "TestedMedia");

            migrationBuilder.DropColumn("ReadLongRetryLbaData", "TestedMedia");

            migrationBuilder.DropColumn("ReadRetryLbaData", "TestedMedia");

            migrationBuilder.DropColumn("ReadSectorsData", "TestedMedia");

            migrationBuilder.DropColumn("ReadSectorsRetryData", "TestedMedia");

            migrationBuilder.DropColumn("TocData", "TestedMedia");

            migrationBuilder.DropColumn("Track1PregapData", "TestedMedia");
        }
    }
}