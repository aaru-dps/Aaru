using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class AddAudioFieldsToGdromReadCapabilities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>("Lba100000AudioData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba100000AudioDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba100000AudioPqData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba100000AudioPqDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba100000AudioPqReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba100000AudioPqReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba100000AudioPqSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<bool>("Lba100000AudioReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba100000AudioReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba100000AudioRwData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba100000AudioRwDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba100000AudioRwReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba100000AudioRwReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba100000AudioRwSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba100000AudioSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba400000AudioAudioData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba400000AudioDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba400000AudioPqData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba400000AudioPqDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba400000AudioPqReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba400000AudioPqReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba400000AudioPqSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<bool>("Lba400000AudioReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba400000AudioReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba400000AudioRwData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba400000AudioRwDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba400000AudioRwReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba400000AudioRwReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba400000AudioRwSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba400000AudioSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba44990AudioData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba44990AudioDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba44990AudioPqData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba44990AudioPqDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba44990AudioPqReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba44990AudioPqReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba44990AudioPqSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<bool>("Lba44990AudioReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba44990AudioReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba44990AudioRwData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba44990AudioRwDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba44990AudioRwReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba44990AudioRwReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba44990AudioRwSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba44990AudioSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba450000AudioData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba450000AudioDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba450000AudioPqData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba450000AudioPqDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba450000AudioPqReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba450000AudioPqReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba450000AudioPqSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<bool>("Lba450000AudioReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba450000AudioReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba450000AudioRwData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba450000AudioRwDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba450000AudioRwReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba450000AudioRwReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba450000AudioRwSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba450000AudioSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba45000AudioData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba45000AudioDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba45000AudioPqData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba45000AudioPqDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba45000AudioPqReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba45000AudioPqReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba45000AudioPqSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<bool>("Lba45000AudioReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba45000AudioReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba45000AudioRwData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba45000AudioRwDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba45000AudioRwReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba45000AudioRwReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba45000AudioRwSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba45000AudioSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba50000AudioData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba50000AudioDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba50000AudioPqData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba50000AudioPqDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba50000AudioPqReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba50000AudioPqReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba50000AudioPqSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<bool>("Lba50000AudioReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba50000AudioReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba50000AudioRwData", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<string>("Lba50000AudioRwDecodedSense", "GdRomSwapDiscCapabilities",
                                               nullable: true);

            migrationBuilder.AddColumn<bool>("Lba50000AudioRwReadable", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<int>("Lba50000AudioRwReadableCluster", "GdRomSwapDiscCapabilities",
                                            nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>("Lba50000AudioRwSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<byte[]>("Lba50000AudioSense", "GdRomSwapDiscCapabilities", nullable: true);

            migrationBuilder.AddColumn<bool>("TestCrashed", "GdRomSwapDiscCapabilities", nullable: false,
                                             defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Lba100000AudioData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioPqData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioPqDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioPqReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioPqReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioPqSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioRwData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioRwDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioRwReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioRwReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioRwSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba100000AudioSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioAudioData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioPqData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioPqDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioPqReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioPqReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioPqSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioRwData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioRwDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioRwReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioRwReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioRwSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba400000AudioSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioPqData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioPqDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioPqReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioPqReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioPqSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioRwData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioRwDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioRwReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioRwReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioRwSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba44990AudioSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioPqData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioPqDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioPqReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioPqReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioPqSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioRwData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioRwDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioRwReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioRwReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioRwSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba450000AudioSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioPqData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioPqDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioPqReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioPqReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioPqSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioRwData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioRwDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioRwReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioRwReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioRwSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba45000AudioSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioPqData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioPqDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioPqReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioPqReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioPqSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioRwData", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioRwDecodedSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioRwReadable", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioRwReadableCluster", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioRwSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("Lba50000AudioSense", "GdRomSwapDiscCapabilities");

            migrationBuilder.DropColumn("TestCrashed", "GdRomSwapDiscCapabilities");
        }
    }
}