using System.Collections.Generic;
using System.IO;

// ReSharper disable StringLiteralTypo

namespace Aaru.Tests.Issues
{
    /* https://github.com/aaru-dps/Aaru/issues/358
     * 
     * roysmeding commented on Apr 27, 2020
     * 
     * When extracting files from a CD-i disk image, the sector subheader data that is required to be able to
     * interpret most real-time files is not extracted.
     */

    // 20200621 CLAUNIA: Fixed in 83a28237fab9e21b23bd43eb91b5b29f1bf9f220
    public class _358 : FsExtractHashIssueTest
    {
        protected override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Fixed", "issue358");
        protected override string TestFile => "cdi.aif";
        protected override Dictionary<string, string> ParsedOptions => new Dictionary<string, string>();
        protected override bool Debug => false;
        protected override bool Xattrs => true;
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
                            VolumeName  = "Compton's Multimedia Encyclopedi",
                            Directories = new List<string>(),
                            Files = new Dictionary<string, FileData>
                            {
                                {
                                    "path_tbl", new FileData
                                    {
                                        MD5 = "659ab7b1da8eb6ef2f87d2ae30406a4c",
                                        XattrsWithMd5 = new Dictionary<string, string>
                                        {
                                            {
                                                "org.iso.mode2.subheader", "6505facc70c11bf7080559019383e783"
                                            },
                                            {
                                                "org.iso.mode2.subheader.copy", "6505facc70c11bf7080559019383e783"
                                            }
                                        }
                                    }
                                },
                                {
                                    "Copyright", new FileData
                                    {
                                        MD5 = "15b50a1e7c9c816db754dc3bb184b527",
                                        XattrsWithMd5 = new Dictionary<string, string>
                                        {
                                            {
                                                "org.iso.mode2.subheader", "6505facc70c11bf7080559019383e783"
                                            },
                                            {
                                                "org.iso.mode2.subheader.copy", "6505facc70c11bf7080559019383e783"
                                            }
                                        }
                                    }
                                },
                                {
                                    "Abstract", new FileData
                                    {
                                        MD5 = "8e6703e1bbaae143a72280268d0cd7ac",
                                        XattrsWithMd5 = new Dictionary<string, string>
                                        {
                                            {
                                                "org.iso.mode2.subheader", "6505facc70c11bf7080559019383e783"
                                            },
                                            {
                                                "org.iso.mode2.subheader.copy", "6505facc70c11bf7080559019383e783"
                                            }
                                        }
                                    }
                                },
                                {
                                    "Bibliography", new FileData
                                    {
                                        MD5 = "f3b30f17c0a71394c27a5a66aa351c51",
                                        XattrsWithMd5 = new Dictionary<string, string>
                                        {
                                            {
                                                "org.iso.mode2.subheader", "6505facc70c11bf7080559019383e783"
                                            },
                                            {
                                                "org.iso.mode2.subheader.copy", "6505facc70c11bf7080559019383e783"
                                            }
                                        }
                                    }
                                },
                                {
                                    "cdi_cme1", new FileData
                                    {
                                        MD5 = "a085026ecc45c3b18e18af478711e36b",
                                        XattrsWithMd5 = new Dictionary<string, string>
                                        {
                                            {
                                                "org.iso.mode2.subheader", "78935a91628edd27e763828e07263a35"
                                            },
                                            {
                                                "org.iso.mode2.subheader.copy", "78935a91628edd27e763828e07263a35"
                                            }
                                        }
                                    }
                                },
                                {
                                    "cdi_cme.stb", new FileData
                                    {
                                        MD5 = "a935529bd99cade337f375b3cb2a143c",
                                        XattrsWithMd5 = new Dictionary<string, string>
                                        {
                                            {
                                                "org.iso.mode2.subheader", "d236e910b9004a92bb3c082e9d60101c"
                                            },
                                            {
                                                "org.iso.mode2.subheader.copy", "d236e910b9004a92bb3c082e9d60101c"
                                            }
                                        }
                                    }
                                },
                                {
                                    "cdi_bumper", new FileData
                                    {
                                        MD5 = "490a71f36e109ff7cf5226366450614e",
                                        XattrsWithMd5 = new Dictionary<string, string>
                                        {
                                            {
                                                "org.iso.mode2.subheader", "f839ff4781a96ccd32227f9f6f8f7744"
                                            },
                                            {
                                                "org.iso.mode2.subheader.copy", "f839ff4781a96ccd32227f9f6f8f7744"
                                            }
                                        }
                                    }
                                },
                                {
                                    "cdi_bumpdata", new FileData
                                    {
                                        MD5 = "fa3f795ffe82e418976b29b4928f6ff2",
                                        XattrsWithMd5 = new Dictionary<string, string>
                                        {
                                            {
                                                "org.iso.mode2.subheader", "6505facc70c11bf7080559019383e783"
                                            },
                                            {
                                                "org.iso.mode2.subheader.copy", "6505facc70c11bf7080559019383e783"
                                            }
                                        }
                                    }
                                },
                                {
                                    "bumper.rtf", new FileData
                                    {
                                        MD5 = "273dc435323ee73f63c1bbce0b354e48",
                                        XattrsWithMd5 = new Dictionary<string, string>
                                        {
                                            {
                                                "org.iso.mode2.subheader", "17cd61fdd638466ff4e15c535c930bc8"
                                            },
                                            {
                                                "org.iso.mode2.subheader.copy", "17cd61fdd638466ff4e15c535c930bc8"
                                            }
                                        }
                                    }
                                },
                                {
                                    "COMPTON", new FileData
                                    {
                                        MD5 = "cfedf3479f2f6c02a989fb847b03dbaa",
                                        XattrsWithMd5 = new Dictionary<string, string>
                                        {
                                            {
                                                "org.iso.mode2.subheader", "10495f9e84546060b8a370da206fc379"
                                            },
                                            {
                                                "org.iso.mode2.subheader.copy", "10495f9e84546060b8a370da206fc379"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}