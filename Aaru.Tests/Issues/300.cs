using System.Collections.Generic;
using System.IO;

// ReSharper disable StringLiteralTypo

namespace Aaru.Tests.Issues
{
    /* https://github.com/aaru-dps/Aaru/issues/300
     * 
     * SilasLaspada commented on Mar 9, 2020
     * 
     * Trying to extract files from an image results in
     * "Partition 1:
     * Identifying filesystem on partition
     * Error reading file: Object reference not set to an instance of an object."
     *
     * Aaru does extract the files successfully as far as I can tell despite the error.
     * Log and image files: https://drive.google.com/open?id=17Hzuo4rj9UbLA8Zh3-tclkWM3a_L43e7
     */

    // 20200309 CLAUNIA: Fixed in 48f067d79ff30cfd10e084085ff479bbb0939512
    public class _300 : FsExtractHashIssueTest
    {
        protected override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Fixed", "issue300");
        protected override string TestFile => "sony.dicf";
        protected override Dictionary<string, string> ParsedOptions => new Dictionary<string, string>();
        protected override bool Debug => true;
        protected override bool Xattrs => false;
        protected override string Encoding => null;
        protected override bool ExpectPartitions => true;
        protected override string Namespace => null;

        protected override FsExtractHashData ExpectedData => new FsExtractHashData
        {
            Partitions = new[]
            {
                new PartitionVolumes
                {
                    Volumes = new[]
                    {
                        new VolumeData
                        {
                            VolumeName  = "Sony USB Driver",
                            Directories = new List<string>(),
                            FilesWithMd5 = new Dictionary<string, string>
                            {
                                {
                                    "DATA1.HDR", "3d1ffe01d012dcc0fc208ff0c0dd5cfe"
                                },
                                {
                                    "$PATH_TABLE.MSB", "cc662c00c2ee224212141f82414f06c2"
                                },
                                {
                                    "PRIV.DLL", "a2a17469db36053d5000781eb00e8da2"
                                },
                                {
                                    "LAYOUT.BIN", "5a7c5bc8faad06c9b0e7b674bef678de"
                                },
                                {
                                    "USBSETUP.DAT", "efeae3b28ce736eeab4347912ffa7293"
                                },
                                {
                                    "$PATH_TABLE.LSB", "b396ff192a15bd3a0bc6075eef122ebd"
                                },
                                {
                                    "SETUP.INX", "91311772c03c18d3e0bce5d1ca8c4375"
                                },
                                {
                                    "IKERNEL.EX_", "63736e15d6061f12d0ea9cc9586ee931"
                                },
                                {
                                    "SETUP.INI", "22a4471186e5bf51c94c75fc6edb052d"
                                },
                                {
                                    "SONYSYS.DAT", "5abfed688c49490a63f57ed9dd2a2ead"
                                },
                                {
                                    "$", "557e3a3615a72aba02dce151b36ebc7a"
                                },
                                {
                                    "$PVD", "a96f2e6434b669f2b4582965b5661f4a"
                                },
                                {
                                    "DATA2.CAB", "debccd001263b8863e9677984e05fcc5"
                                },
                                {
                                    "DOSETUP.DAT", "48aa8c4215e554239d8f702beb1769da"
                                },
                                {
                                    "SETUP.EXE", "d0f6e0fb47eafc597fc588bb18711211"
                                },
                                {
                                    "DATA1.CAB", "4ce6639d1f34dee43b037787c35c1561"
                                }
                            }
                        }
                    }
                },
                new PartitionVolumes()
            }
        };
    }
}