using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class StoreReadResultsInReportDatabase : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestedMedias", "Read6Data",                     c => c.Binary());
            AddColumn("dbo.TestedMedias", "Read10Data",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "Read12Data",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "Read16Data",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadLong10Data",                c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadLong16Data",                c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadSectorsData",               c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadSectorsRetryData",          c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadDmaData",                   c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadDmaRetryData",              c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadLbaData",                   c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadRetryLbaData",              c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadDmaLbaData",                c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadDmaRetryLbaData",           c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadLba48Data",                 c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadDmaLba48Data",              c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadLongData",                  c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadLongRetryData",             c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadLongLbaData",               c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadLongRetryLbaData",          c => c.Binary());
            AddColumn("dbo.TestedMedias", "TocData",                       c => c.Binary());
            AddColumn("dbo.TestedMedias", "FullTocData",                   c => c.Binary());
            AddColumn("dbo.TestedMedias", "AtipData",                      c => c.Binary());
            AddColumn("dbo.TestedMedias", "PmaData",                       c => c.Binary());
            AddColumn("dbo.TestedMedias", "PfiData",                       c => c.Binary());
            AddColumn("dbo.TestedMedias", "DmiData",                       c => c.Binary());
            AddColumn("dbo.TestedMedias", "CmiData",                       c => c.Binary());
            AddColumn("dbo.TestedMedias", "DvdBcaData",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "DvdAacsData",                   c => c.Binary());
            AddColumn("dbo.TestedMedias", "DvdDdsData",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "DvdSaiData",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "BluBcaData",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "BluDdsData",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "BluSaiData",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "PriData",                       c => c.Binary());
            AddColumn("dbo.TestedMedias", "EmbossedPfiData",               c => c.Binary());
            AddColumn("dbo.TestedMedias", "AdipData",                      c => c.Binary());
            AddColumn("dbo.TestedMedias", "DcbData",                       c => c.Binary());
            AddColumn("dbo.TestedMedias", "HdCmiData",                     c => c.Binary());
            AddColumn("dbo.TestedMedias", "DvdLayerData",                  c => c.Binary());
            AddColumn("dbo.TestedMedias", "BluDiData",                     c => c.Binary());
            AddColumn("dbo.TestedMedias", "BluPacData",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadCdData",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadCdMsfData",                 c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadCdFullData",                c => c.Binary());
            AddColumn("dbo.TestedMedias", "ReadCdMsfFullData",             c => c.Binary());
            AddColumn("dbo.TestedMedias", "Track1PregapData",              c => c.Binary());
            AddColumn("dbo.TestedMedias", "LeadInData",                    c => c.Binary());
            AddColumn("dbo.TestedMedias", "LeadOutData",                   c => c.Binary());
            AddColumn("dbo.TestedMedias", "C2PointersData",                c => c.Binary());
            AddColumn("dbo.TestedMedias", "PQSubchannelData",              c => c.Binary());
            AddColumn("dbo.TestedMedias", "RWSubchannelData",              c => c.Binary());
            AddColumn("dbo.TestedMedias", "CorrectedSubchannelData",       c => c.Binary());
            AddColumn("dbo.TestedMedias", "PQSubchannelWithC2Data",        c => c.Binary());
            AddColumn("dbo.TestedMedias", "RWSubchannelWithC2Data",        c => c.Binary());
            AddColumn("dbo.TestedMedias", "CorrectedSubchannelWithC2Data", c => c.Binary());
        }

        public override void Down()
        {
            DropColumn("dbo.TestedMedias", "CorrectedSubchannelWithC2Data");
            DropColumn("dbo.TestedMedias", "RWSubchannelWithC2Data");
            DropColumn("dbo.TestedMedias", "PQSubchannelWithC2Data");
            DropColumn("dbo.TestedMedias", "CorrectedSubchannelData");
            DropColumn("dbo.TestedMedias", "RWSubchannelData");
            DropColumn("dbo.TestedMedias", "PQSubchannelData");
            DropColumn("dbo.TestedMedias", "C2PointersData");
            DropColumn("dbo.TestedMedias", "LeadOutData");
            DropColumn("dbo.TestedMedias", "LeadInData");
            DropColumn("dbo.TestedMedias", "Track1PregapData");
            DropColumn("dbo.TestedMedias", "ReadCdMsfFullData");
            DropColumn("dbo.TestedMedias", "ReadCdFullData");
            DropColumn("dbo.TestedMedias", "ReadCdMsfData");
            DropColumn("dbo.TestedMedias", "ReadCdData");
            DropColumn("dbo.TestedMedias", "BluPacData");
            DropColumn("dbo.TestedMedias", "BluDiData");
            DropColumn("dbo.TestedMedias", "DvdLayerData");
            DropColumn("dbo.TestedMedias", "HdCmiData");
            DropColumn("dbo.TestedMedias", "DcbData");
            DropColumn("dbo.TestedMedias", "AdipData");
            DropColumn("dbo.TestedMedias", "EmbossedPfiData");
            DropColumn("dbo.TestedMedias", "PriData");
            DropColumn("dbo.TestedMedias", "BluSaiData");
            DropColumn("dbo.TestedMedias", "BluDdsData");
            DropColumn("dbo.TestedMedias", "BluBcaData");
            DropColumn("dbo.TestedMedias", "DvdSaiData");
            DropColumn("dbo.TestedMedias", "DvdDdsData");
            DropColumn("dbo.TestedMedias", "DvdAacsData");
            DropColumn("dbo.TestedMedias", "DvdBcaData");
            DropColumn("dbo.TestedMedias", "CmiData");
            DropColumn("dbo.TestedMedias", "DmiData");
            DropColumn("dbo.TestedMedias", "PfiData");
            DropColumn("dbo.TestedMedias", "PmaData");
            DropColumn("dbo.TestedMedias", "AtipData");
            DropColumn("dbo.TestedMedias", "FullTocData");
            DropColumn("dbo.TestedMedias", "TocData");
            DropColumn("dbo.TestedMedias", "ReadLongRetryLbaData");
            DropColumn("dbo.TestedMedias", "ReadLongLbaData");
            DropColumn("dbo.TestedMedias", "ReadLongRetryData");
            DropColumn("dbo.TestedMedias", "ReadLongData");
            DropColumn("dbo.TestedMedias", "ReadDmaLba48Data");
            DropColumn("dbo.TestedMedias", "ReadLba48Data");
            DropColumn("dbo.TestedMedias", "ReadDmaRetryLbaData");
            DropColumn("dbo.TestedMedias", "ReadDmaLbaData");
            DropColumn("dbo.TestedMedias", "ReadRetryLbaData");
            DropColumn("dbo.TestedMedias", "ReadLbaData");
            DropColumn("dbo.TestedMedias", "ReadDmaRetryData");
            DropColumn("dbo.TestedMedias", "ReadDmaData");
            DropColumn("dbo.TestedMedias", "ReadSectorsRetryData");
            DropColumn("dbo.TestedMedias", "ReadSectorsData");
            DropColumn("dbo.TestedMedias", "ReadLong16Data");
            DropColumn("dbo.TestedMedias", "ReadLong10Data");
            DropColumn("dbo.TestedMedias", "Read16Data");
            DropColumn("dbo.TestedMedias", "Read12Data");
            DropColumn("dbo.TestedMedias", "Read10Data");
            DropColumn("dbo.TestedMedias", "Read6Data");
        }
    }
}