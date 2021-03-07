using System.Collections.Generic;
using System.IO;

// ReSharper disable StringLiteralTypo

namespace Aaru.Tests.Issues
{
    /* https://github.com/aaru-dps/Aaru/issues/263
     * 
     * SilasLaspada commented on Jan 2, 2020
     * 
     * Trying to extract files from an ISO9660 cue image results in output that have the correct names and folders,
     * but are corrupted past the first few sectors. The image can be found at
     * https://archive.org/details/redump-id-54014
     */

    // 20200309 CLAUNIA: Fixed in 3b2bb0ebf0c6c615c5622aebff494ed34b51055d
    public class _266 : FsExtractHashIssueTest
    {
        protected override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Fixed", "issue266");
        protected override string TestFile => "Namco (USA) (2005 Assets).cue";
        protected override Dictionary<string, string> ParsedOptions => new Dictionary<string, string>();
        protected override bool Debug => false;
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
                            VolumeName = "May 02 2005",
                            Directories = new List<string>
                            {
                                "Namco E3 Assets Disc",
                                "Namco E3 Assets Disc/Arc The Lad",
                                "Namco E3 Assets Disc/Arc The Lad/Box Front",
                                "Namco E3 Assets Disc/Arc The Lad/Characters",
                                "Namco E3 Assets Disc/Arc The Lad/Logo",
                                "Namco E3 Assets Disc/Arc The Lad/Screens",
                                "Namco E3 Assets Disc/Arc The Lad/Sell sheet",
                                "Namco E3 Assets Disc/Atomic Betty",
                                "Namco E3 Assets Disc/Atomic Betty/Characters",
                                "Namco E3 Assets Disc/Atomic Betty/Logo",
                                "Namco E3 Assets Disc/Atomic Betty/Screens",
                                "Namco E3 Assets Disc/Atomic Betty/Sell sheet",
                                "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree",
                                "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Logo",
                                "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens",
                                "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Sell sheet",
                                "Namco E3 Assets Disc/Bounty Hounds",
                                "Namco E3 Assets Disc/Bounty Hounds/Logo",
                                "Namco E3 Assets Disc/Bounty Hounds/Screens",
                                "Namco E3 Assets Disc/Bounty Hounds/Sell sheet",
                                "Namco E3 Assets Disc/Curious George",
                                "Namco E3 Assets Disc/Curious George/Sell Sheet",
                                "Namco E3 Assets Disc/Dead to Rights Reckoning",
                                "Namco E3 Assets Disc/Dead to Rights Reckoning/Box Art",
                                "Namco E3 Assets Disc/Dead to Rights Reckoning/Logo",
                                "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens",
                                "Namco E3 Assets Disc/Dead to Rights Reckoning/Sell sheet",
                                "Namco E3 Assets Disc/Frame City Killer",
                                "Namco E3 Assets Disc/Frame City Killer/Characters",
                                "Namco E3 Assets Disc/Frame City Killer/Concept Art",
                                "Namco E3 Assets Disc/Frame City Killer/Logo",
                                "Namco E3 Assets Disc/Frame City Killer/Screens",
                                "Namco E3 Assets Disc/Frame City Killer/Sell sheet",
                                "Namco E3 Assets Disc/Gumby Vs The Astrobots",
                                "Namco E3 Assets Disc/Gumby Vs The Astrobots/Logo",
                                "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens",
                                "Namco E3 Assets Disc/Gumby Vs The Astrobots/Sell sheet",
                                "Namco E3 Assets Disc/Hello Kitty",
                                "Namco E3 Assets Disc/Hello Kitty/Logo",
                                "Namco E3 Assets Disc/Hello Kitty/Screens",
                                "Namco E3 Assets Disc/Hello Kitty/Sell sheet",
                                "Namco E3 Assets Disc/Mage Knight",
                                "Namco E3 Assets Disc/Mage Knight/Concept Art",
                                "Namco E3 Assets Disc/Mage Knight/Logo",
                                "Namco E3 Assets Disc/Mage Knight/Screens",
                                "Namco E3 Assets Disc/Mage Knight/Sell sheet",
                                "Namco E3 Assets Disc/MotoGP4",
                                "Namco E3 Assets Disc/MotoGP4/Logo",
                                "Namco E3 Assets Disc/MotoGP4/Screens",
                                "Namco E3 Assets Disc/MotoGP4/Sell sheet",
                                "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection",
                                "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Box Front",
                                "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Logo",
                                "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens",
                                "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit",
                                "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Sell sheet",
                                "Namco E3 Assets Disc/Namco Museum Battle Collection",
                                "Namco E3 Assets Disc/Namco Museum Battle Collection/Logos",
                                "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens",
                                "Namco E3 Assets Disc/Namco Museum Battle Collection/Sell sheet",
                                "Namco E3 Assets Disc/Pac N Roll",
                                "Namco E3 Assets Disc/Pac N Roll/Logo",
                                "Namco E3 Assets Disc/Pac N Roll/Screens",
                                "Namco E3 Assets Disc/Pac N Roll/Sell sheet",
                                "Namco E3 Assets Disc/Pac-Man World 3",
                                "Namco E3 Assets Disc/Pac-Man World 3/Backgrounds",
                                "Namco E3 Assets Disc/Pac-Man World 3/Characters",
                                "Namco E3 Assets Disc/Pac-Man World 3/Concept Art",
                                "Namco E3 Assets Disc/Pac-Man World 3/Logo",
                                "Namco E3 Assets Disc/Pac-Man World 3/Pac-Man 25th Anniversary logo",
                                "Namco E3 Assets Disc/Pac-Man World 3/Screens",
                                "Namco E3 Assets Disc/Pac-Man World 3/Screens/GC",
                                "Namco E3 Assets Disc/Pac-Man World 3/Screens/PS2",
                                "Namco E3 Assets Disc/Pac-Man World 3/Screens/PSP",
                                "Namco E3 Assets Disc/Pac-Man World 3/Screens/Xbox",
                                "Namco E3 Assets Disc/Pac-Man World 3/Sell sheet",
                                "Namco E3 Assets Disc/Payout Poker and Casino",
                                "Namco E3 Assets Disc/Payout Poker and Casino/Logo",
                                "Namco E3 Assets Disc/Payout Poker and Casino/Screens",
                                "Namco E3 Assets Disc/Payout Poker and Casino/Sell sheet",
                                "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires",
                                "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Logo",
                                "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens",
                                "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Sell sheet",
                                "Namco E3 Assets Disc/Rebelstar Tactical Command",
                                "Namco E3 Assets Disc/Rebelstar Tactical Command/Characters",
                                "Namco E3 Assets Disc/Rebelstar Tactical Command/Logo",
                                "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens",
                                "Namco E3 Assets Disc/Rebelstar Tactical Command/Sell sheet",
                                "Namco E3 Assets Disc/SOULCALIBUR III",
                                "Namco E3 Assets Disc/SOULCALIBUR III/Characters",
                                "Namco E3 Assets Disc/SOULCALIBUR III/Logo",
                                "Namco E3 Assets Disc/SOULCALIBUR III/Screens",
                                "Namco E3 Assets Disc/SOULCALIBUR III/Sell sheet",
                                "Namco E3 Assets Disc/Sigma Star Saga",
                                "Namco E3 Assets Disc/Sigma Star Saga/Box Front",
                                "Namco E3 Assets Disc/Sigma Star Saga/Logo",
                                "Namco E3 Assets Disc/Sigma Star Saga/Screens",
                                "Namco E3 Assets Disc/Sigma Star Saga/Sell sheet",
                                "Namco E3 Assets Disc/Sniper Elite",
                                "Namco E3 Assets Disc/Sniper Elite/Concept Art",
                                "Namco E3 Assets Disc/Sniper Elite/Logo",
                                "Namco E3 Assets Disc/Sniper Elite/Screens",
                                "Namco E3 Assets Disc/Sniper Elite/Screens/PC",
                                "Namco E3 Assets Disc/Sniper Elite/Screens/PS2",
                                "Namco E3 Assets Disc/Sniper Elite/Screens/PS2/PS2",
                                "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox",
                                "Namco E3 Assets Disc/Sniper Elite/Sell sheet",
                                "Namco E3 Assets Disc/Stargate SG-1 The Alliance",
                                "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Logo",
                                "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens",
                                "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC",
                                "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Sell sheet",
                                "Namco E3 Assets Disc/Tale of Legendia",
                                "Namco E3 Assets Disc/Tale of Legendia/Logo",
                                "Namco E3 Assets Disc/Tale of Legendia/Sell Sheet",
                                "Namco E3 Assets Disc/Urban Reign",
                                "Namco E3 Assets Disc/Urban Reign/Logo",
                                "Namco E3 Assets Disc/Urban Reign/Screens",
                                "Namco E3 Assets Disc/Urban Reign/Sell sheet",
                                "Namco E3 Assets Disc/We Love Katamari",
                                "Namco E3 Assets Disc/We Love Katamari/Characters",
                                "Namco E3 Assets Disc/We Love Katamari/Screens",
                                "Namco E3 Assets Disc/We Love Katamari/Sell sheet"
                            },
                            Files = new Dictionary<string, FileData>
                            {
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Box Front/ARC concept rd6.jpg", new FileData
                                    {
                                        MD5 = "b3d911ab1051a45a7137654a68a5c441"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Characters/Arc.psd", new FileData
                                    {
                                        MD5 = "8a64009459bd9e867e9ecd76a29702af"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Characters/Edda.psd", new FileData
                                    {
                                        MD5 = "4a34bdceb7a27b50415b97923470635b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Characters/Hemo.psd", new FileData
                                    {
                                        MD5 = "de993bc5fe849571afecb4afe45603a2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Characters/Kirika.psd", new FileData
                                    {
                                        MD5 = "b49b717ff3a10f602fc7d0c47891f4f5"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Characters/Thumbs.db", new FileData
                                    {
                                        MD5 = "89161af24fccb15374d1ab32616d6d97"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Logo/Arc The Lad Logo_bigger.psd", new FileData
                                    {
                                        MD5 = "a49eccce2bd793afdaca6d6baf5639b9"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Screens/battle (16).bmp", new FileData
                                    {
                                        MD5 = "02d844c1b057bd41b4eef4c801ae2a7f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Screens/battle (23).bmp", new FileData
                                    {
                                        MD5 = "d07e8242495798b9007bd8145636fa7e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Screens/battle (66).bmp", new FileData
                                    {
                                        MD5 = "a3826bf7bd214faaa2b6dfc47073eb8f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Screens/battle (81).jpg", new FileData
                                    {
                                        MD5 = "c8155689e10969412687313ce7d67f91"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Screens/king-size enemy02 (4).bmp", new FileData
                                    {
                                        MD5 = "741388b6ee8e0b4f7c6282e56516c444"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Screens/king-size enemy03 (1).bmp", new FileData
                                    {
                                        MD5 = "d1a83d98d7a9145cb27f6cbc9fd6e5d8"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Screens/king-size enemy04.bmp", new FileData
                                    {
                                        MD5 = "9c2aee160e3a091bd8f9cf51c5e3dbde"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Screens/king-size enemy05 (2).bmp", new FileData
                                    {
                                        MD5 = "2c76030138329eb793b6952c3083c934"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Arc The Lad/Sell sheet/Arc the Lad SS.pdf", new FileData
                                    {
                                        MD5 = "3a80cbd3870df88e387fef43d78cfff3"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Characters/AB_2-3.psd", new FileData
                                    {
                                        MD5 = "2f3159a19a059074257f5a889832b402"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Characters/AB_4-1 4-2.psd", new FileData
                                    {
                                        MD5 = "e6c699a7884547c6a408c85411657aa9"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Characters/AB_5-3.psd", new FileData
                                    {
                                        MD5 = "9815f4004993b78b4cfad61773644834"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Characters/AB_7-1 7-2.psd", new FileData
                                    {
                                        MD5 = "38278e2807f6f6808fec2ebb693f5306"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Characters/AB_8-2.psd", new FileData
                                    {
                                        MD5 = "62839a336afbc0147ffe48a44001bd3d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Characters/AB-REF-FlyingBetty_REV.ai",
                                    new FileData
                                    {
                                        MD5 = "18774c66a58d5a0c92dbbc764e1a94b3"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Characters/CG_01.eps", new FileData
                                    {
                                        MD5 = "f471da563d7a0c8990ba03301d51d96a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Characters/CharacterSizeChart.ai", new FileData
                                    {
                                        MD5 = "b520df688809d97b877a144ee7f2991a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Characters/Thumbs.db", new FileData
                                    {
                                        MD5 = "89161af24fccb15374d1ab32616d6d97"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Logo/logo_bandw.jpg", new FileData
                                    {
                                        MD5 = "9cf1942209c3e1dd868dea74ba02bce5"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Logo/logo_bandw.psd", new FileData
                                    {
                                        MD5 = "0a1a43cd8e88a6249114b6a4d1a3cfea"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Logo/logo_color.jpg", new FileData
                                    {
                                        MD5 = "62d3e1523b56c181fc2e77aa454498f6"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Logo/logo_color.psd", new FileData
                                    {
                                        MD5 = "25c8ec67a4e977b60e3a4d86ee290ba9"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/Mini-Game ss_11.bmp", new FileData
                                    {
                                        MD5 = "f5924fe17669e30e227bc84266f1a2cd"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/Mini-Game ss_12.bmp", new FileData
                                    {
                                        MD5 = "94bae925f5e9eebc4a1a18f70f3e4418"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/ss_1.bmp", new FileData
                                    {
                                        MD5 = "379dbeeb8b22265b2208d93f34398a94"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/ss_15.bmp", new FileData
                                    {
                                        MD5 = "2422cadcb2a54c244772ceb97be8fedb"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/ss_2.bmp", new FileData
                                    {
                                        MD5 = "763d983f2357e8cc49a9cf75c0c0105a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/ss_3.bmp", new FileData
                                    {
                                        MD5 = "a4c6d5eec08f5388128c6db031abb705"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/ss_4.bmp", new FileData
                                    {
                                        MD5 = "da9ccbfce24f6af06b9ed99966f737a6"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/ss_5.bmp", new FileData
                                    {
                                        MD5 = "d46424d125f70c5ed3e33f3b0011656b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/ss_6.bmp", new FileData
                                    {
                                        MD5 = "0afe635906746d628e06b3f7a93d70b8"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/ss_7.bmp", new FileData
                                    {
                                        MD5 = "675ea8662d1126e36d6f37f7274f2178"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/ss_8.bmp", new FileData
                                    {
                                        MD5 = "2983db6ebaaae715167869589cdef27a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "4a5844d6a1fa3439b0db8697936920af"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Atomic Betty/Sell sheet/Atomic Betty SS.pdf", new FileData
                                    {
                                        MD5 = "c3b1141e9e050724d5ea8d3b6e6b71b8"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Logo/BB_SOT Logo.psd",
                                    new FileData
                                    {
                                        MD5 = "d1654bd076ac1c519042887dbaadf675"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_1.png",
                                    new FileData
                                    {
                                        MD5 = "18ebc4bad3abbeea2c19321ad63dcc1c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_11.png",
                                    new FileData
                                    {
                                        MD5 = "1d4ff7fb00b6bb9585ffe4d222acd038"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_12.png",
                                    new FileData
                                    {
                                        MD5 = "88221d6f3cbb8f57e6ebaf6bcb6f3b65"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_13.png",
                                    new FileData
                                    {
                                        MD5 = "152a7aa455826f4ba6d52c74f5a24ebf"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_14.png",
                                    new FileData
                                    {
                                        MD5 = "4425067fbc7ef57c5c0c23f6d4369fbe"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_15.png",
                                    new FileData
                                    {
                                        MD5 = "bcaa8bd1a85a1fa2561a7339b3754a08"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_2.png",
                                    new FileData
                                    {
                                        MD5 = "76358f197f13c2574e558f755f585d88"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_3.png",
                                    new FileData
                                    {
                                        MD5 = "8dd03bcac1d75d514574742dd7477747"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_4.png",
                                    new FileData
                                    {
                                        MD5 = "19df5261a3f5e7220d9429bd8f82f026"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_5.png",
                                    new FileData
                                    {
                                        MD5 = "6f717d740b4234bcc20099f67d91dc86"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_6.png",
                                    new FileData
                                    {
                                        MD5 = "c7a1f14ac2292387d47e3517b710704a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_7.png",
                                    new FileData
                                    {
                                        MD5 = "fcfd063203695e77f2c65a19b9607569"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_8.png",
                                    new FileData
                                    {
                                        MD5 = "7e9881ca9f6bd90339bac7296fe1420c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/042505_9.png",
                                    new FileData
                                    {
                                        MD5 = "ac36e92e6e72b801154e0007af3a0401"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Screens/Thumbs.db",
                                    new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Berenstain Bears and the Spooky Old Tree/Sell sheet/BB_SOT SS.pdf",
                                    new FileData
                                    {
                                        MD5 = "3e5034d4564f3458dfd840a27dc9f68b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Logo/bh logo.jpg", new FileData
                                    {
                                        MD5 = "e1deeeb2740c7988443044d01f065934"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Screens/001.bmp", new FileData
                                    {
                                        MD5 = "b8022ebe8005086cf1ca0af931394248"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Screens/008.bmp", new FileData
                                    {
                                        MD5 = "c78b6ffc9f11277267a425424fadf3b0"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Screens/011.bmp", new FileData
                                    {
                                        MD5 = "27729476b865f5d1939d173d52a7c3ab"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Screens/015.bmp", new FileData
                                    {
                                        MD5 = "5288bc8665d8c70dc8a52b6ba851fd20"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Screens/016.bmp", new FileData
                                    {
                                        MD5 = "b3c1d7caacf6f943f1d7fedce349a35f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Screens/018.bmp", new FileData
                                    {
                                        MD5 = "a820d2a17f734b4e785d9a9032b28bf2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Screens/021.bmp", new FileData
                                    {
                                        MD5 = "d89874d151b04dd2400fadf526f54c5a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Screens/026.bmp", new FileData
                                    {
                                        MD5 = "9d32f9d19c524da83f79d4eabc3eaccc"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Screens/029.bmp", new FileData
                                    {
                                        MD5 = "d2a3ce37c35ac0e8671ec0b378d1ca44"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Screens/032.bmp", new FileData
                                    {
                                        MD5 = "d223181823f68a1c165b584261197a79"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Bounty Hounds/Sell sheet/Bounty Hounds SS.pdf", new FileData
                                    {
                                        MD5 = "c5dd5711ce24139f9bd6c4eace5489b0"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Curious George/Sell Sheet/Curious George SS.pdf", new FileData
                                    {
                                        MD5 = "e578772c2d61e69e11e67ae2d5368f12"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Box Art/DTR_PSP_BOX-FRONT.jpg",
                                    new FileData
                                    {
                                        MD5 = "fb8aab14c224e6d314d4bf2a6c5860d9"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Box Art/Thumbs.db", new FileData
                                    {
                                        MD5 = "996f15ab2f890ff979e839cea41b358d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Logo/DTR_PSP_logo_1.psd",
                                    new FileData
                                    {
                                        MD5 = "f2fb646961af004fa404bcdb53991781"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Logo/DTR_PSP_logo_1.tif",
                                    new FileData
                                    {
                                        MD5 = "d1bc8ddad7d6d0b9701b934a141cc974"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens/DTR 2005-04-22 Beer Barn-19.bmp",
                                    new FileData
                                    {
                                        MD5 = "fbf671f247e8ee18f2f51517723fcc44"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens/DTR 2005-04-22 Old Church-10.bmp",
                                    new FileData
                                    {
                                        MD5 = "05cece2b6a11e48019232da5f6db1cb2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens/DTR 2005-04-22 Pink Starfish-00.bmp",
                                    new FileData
                                    {
                                        MD5 = "a2fb383c5aabcf6b08def8ba1550433d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens/DTR 2005-04-22 Pink Starfish-08.bmp",
                                    new FileData
                                    {
                                        MD5 = "1c626b55d5888d72294c337defe39b0a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens/DTR 2005-04-22 Train Yards-10.bmp",
                                    new FileData
                                    {
                                        MD5 = "c878f9373f10f2b13ad213b5de703ff8"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens/DTR 2005-04-22 Train Yards-11.bmp",
                                    new FileData
                                    {
                                        MD5 = "77d4a932488887b39d14ba5f86323457"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens/DTR 2005-04-22 Under Rink-01.bmp",
                                    new FileData
                                    {
                                        MD5 = "8b5ce98b3eb4af98457c7e1758012e38"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens/DTR 2005-04-22 Under Rink-02.bmp",
                                    new FileData
                                    {
                                        MD5 = "0fd6aca248b8f2eac69c8dcfc245c533"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens/DTR 2005-04-22 Under Rink-08.bmp",
                                    new FileData
                                    {
                                        MD5 = "e6589ef45841b3e42a44c17b7e0c7b42"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens/DTR 2005-04-22 Villa-08.bmp",
                                    new FileData
                                    {
                                        MD5 = "24e959cf2faf1fd9026332a591c7358e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Dead to Rights Reckoning/Sell sheet/DTRR SS.pdf", new FileData
                                    {
                                        MD5 = "fa27bbc6f7ccfe0f2e593101cd9afee0"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Characters/khan.psd", new FileData
                                    {
                                        MD5 = "248df32163a4047bf1463b6d0866aaa7"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Characters/Thumbs.db", new FileData
                                    {
                                        MD5 = "89161af24fccb15374d1ab32616d6d97"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Concept Art/crow_face_front01.jpg",
                                    new FileData
                                    {
                                        MD5 = "c74bce6675a2e06cf23a1376475eab16"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Concept Art/crow_face_front02.jpg",
                                    new FileData
                                    {
                                        MD5 = "1afa3c29d3d7d6fdb052b27ebc26911b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Concept Art/Thumbs.db", new FileData
                                    {
                                        MD5 = "996f15ab2f890ff979e839cea41b358d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Concept Art/visual_asid_junkie.psd",
                                    new FileData
                                    {
                                        MD5 = "cc3a9073e8d522eba2fdeb22b78c431f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Logo/FrameCityKillerlogo_nhi.ai",
                                    new FileData
                                    {
                                        MD5 = "d62c1767e6a0ea711c57437436cffb8f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/car08-E3.bmp", new FileData
                                    {
                                        MD5 = "b33442e1388ee3b72b56a8a83f62876f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/casino_f3_2-E3.bmp", new FileData
                                    {
                                        MD5 = "5b7bf82bc0c59f87e4bf833a189bee87"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/city_morning04-E3.bmp", new FileData
                                    {
                                        MD5 = "b7e41ad99732c7479a6c92e308a5b662"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/city_night01.bmp", new FileData
                                    {
                                        MD5 = "82f1612bb52a4e9e4425cf1f8de3bcb5"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/city_night08-E3.bmp", new FileData
                                    {
                                        MD5 = "3c98e4b4aa54c91d14d4322684e786d4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/crow03-E3.bmp", new FileData
                                    {
                                        MD5 = "06130b782aa70b109158fbef69357f9b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/crow06-E3.bmp", new FileData
                                    {
                                        MD5 = "f0e71777c5f740f452aaa203229dbd8b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/inf_scan10-E3.bmp", new FileData
                                    {
                                        MD5 = "36e5c8c098fbc66ea2bb29cb45eef51a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/kick18-E3.bmp", new FileData
                                    {
                                        MD5 = "edde8b2edfc5d001ca32162bbdde7722"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/police03-E3.bmp", new FileData
                                    {
                                        MD5 = "55f337e54beb73ceb5b3f5604c192c8f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/port02-E3.bmp", new FileData
                                    {
                                        MD5 = "8f25eed7385714ab99a914bb868e4f66"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/punch_03-E3.bmp", new FileData
                                    {
                                        MD5 = "1c736ebd2ee14814df99be19ae9be892"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/shoot02-E3.bmp", new FileData
                                    {
                                        MD5 = "5a20c35a54fb6aa455940d5e5b6323da"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/snipe21-E3.bmp", new FileData
                                    {
                                        MD5 = "3e00b9aa3f32cfcb77ae946ce1a48975"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "4f97ec71a52d37fc8376b3347e3e767e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Frame City Killer/Sell sheet/FCK SS.pdf", new FileData
                                    {
                                        MD5 = "5e6a8dbbcdc4cd3722595c272308e769"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Logo/Gumy Logo suppliedNoBkgrd.psd",
                                    new FileData
                                    {
                                        MD5 = "8af54cd8de92da5b0538ba0f810f48b6"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_01.png", new FileData
                                    {
                                        MD5 = "6927a289db5607c52a64505f2b6b745b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_02.png", new FileData
                                    {
                                        MD5 = "055f0dfe9dcc761d0a35965f26e62b91"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_03.png", new FileData
                                    {
                                        MD5 = "4ad82b6c792fb2231642cf755c5bf069"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_04.png", new FileData
                                    {
                                        MD5 = "2192ad5dd3da1a3a5b1b8a294710fef2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_05.png", new FileData
                                    {
                                        MD5 = "ec614cc9e5b33d490a1c0e767bcc0a80"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_06.png", new FileData
                                    {
                                        MD5 = "942b895a578b32d6944aa037fd042c92"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_07.png", new FileData
                                    {
                                        MD5 = "a42b8db657b3ea4c6af2031c3eeb04d6"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_10.png", new FileData
                                    {
                                        MD5 = "b422f23a2e774a07c5899023c4c56c6c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_11.png", new FileData
                                    {
                                        MD5 = "90a18689e1874f581b3a0ee7ba45cbc4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_12.png", new FileData
                                    {
                                        MD5 = "80907a0ed01648e6e43a1178b1e11e6c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_14.png", new FileData
                                    {
                                        MD5 = "834d84bcaba060fc3930bee7988b2935"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_15.png", new FileData
                                    {
                                        MD5 = "7b9dd7972b693910f8a30de83c5fe95c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_18.png", new FileData
                                    {
                                        MD5 = "a6511936ad6b56123c82be1a7f9a7887"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_19.png", new FileData
                                    {
                                        MD5 = "7886461f182e5b8f36c33575dae622ea"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_21.png", new FileData
                                    {
                                        MD5 = "97b83bc102128832a8e77d17d8b2ee4b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_22.png", new FileData
                                    {
                                        MD5 = "491d580282bae27f9cbdfa1f94d0bf35"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_23.png", new FileData
                                    {
                                        MD5 = "a3334186639afe42afff1403ee217959"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_25.png", new FileData
                                    {
                                        MD5 = "d41176e71d07939b9fb79f1a81f298ee"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_26.png", new FileData
                                    {
                                        MD5 = "b767a7465809428946f3fe38a90527cf"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Gumby_27.png", new FileData
                                    {
                                        MD5 = "33c7eb2fcd56ad7cb98253c68005276e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Gumby Vs The Astrobots/Sell sheet/Gumby SS.pdf", new FileData
                                    {
                                        MD5 = "8f097d558592e32df906ce230afacaca"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Hello Kitty/Logo/hello_kitty_roller_rescue_logo_big.psd",
                                    new FileData
                                    {
                                        MD5 = "545b32be4fb0d152c1cf90c887e634fb"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Hello Kitty/Screens/image106.bmp", new FileData
                                    {
                                        MD5 = "cfd1e13d3df7cdb33f0f9cb6c46755d1"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Hello Kitty/Screens/image122.bmp", new FileData
                                    {
                                        MD5 = "6e36f6275b800f76ba5475d029431033"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Hello Kitty/Screens/image144.bmp", new FileData
                                    {
                                        MD5 = "82aee0f803741e68f5276690ff8d586e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Hello Kitty/Screens/image84.bmp", new FileData
                                    {
                                        MD5 = "32f2847d9dd06b49bf14847016b2437a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Hello Kitty/Screens/image87.bmp", new FileData
                                    {
                                        MD5 = "e99e058f27df5006c47f2745454f6c2b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Hello Kitty/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Hello Kitty/Screens/xb_bang-image31.bmp", new FileData
                                    {
                                        MD5 = "a779a59c9d8abfc24c3bad7703b6cc53"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Hello Kitty/Screens/xb_bang-image32.bmp", new FileData
                                    {
                                        MD5 = "1aca7228323adf586feae56fb475626a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Hello Kitty/Screens/xb_bang-image62.bmp", new FileData
                                    {
                                        MD5 = "88127fd16dcb0048eef5a49d8465f767"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Hello Kitty/Sell sheet/Kello Kitty SS.pdf", new FileData
                                    {
                                        MD5 = "e10a5f1c9f7497da3b6a6fdcc120dec0"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Concept Art/MK_POSTER_cmyk copy.JPG", new FileData
                                    {
                                        MD5 = "82cf230cb195522a496f01493529d9ae"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Concept Art/Thumbs.db", new FileData
                                    {
                                        MD5 = "996f15ab2f890ff979e839cea41b358d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Logo/MK Logo.psd", new FileData
                                    {
                                        MD5 = "77fe6eb99cd5cadd42cdb20d3901b557"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Screens/snap_016.bmp", new FileData
                                    {
                                        MD5 = "ca80cf6aa22bc7723eca80949cd316e3"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Screens/snap_018.bmp", new FileData
                                    {
                                        MD5 = "c791c59f7b6e49304996a3465c634c33"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Screens/snap_044.bmp", new FileData
                                    {
                                        MD5 = "0b6ce6e42ffcd4216f707da10bd89774"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Screens/snap015.bmp", new FileData
                                    {
                                        MD5 = "9ad9ae18ee6c83c47cd2b174cbd0827a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Screens/snap022.bmp", new FileData
                                    {
                                        MD5 = "f33f64b215c19808fdb7110c6cd0d3e3"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Screens/snap034.bmp", new FileData
                                    {
                                        MD5 = "df107519ec744e2c70c572cb5ede1cf4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Screens/snap037.bmp", new FileData
                                    {
                                        MD5 = "4804dd7b410daa5f94006b814cca78ec"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Mage Knight/Sell sheet/Mage Knight SS.pdf", new FileData
                                    {
                                        MD5 = "a55776096091896e6e7303747d174200"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Logo/MotoGP4_logo_last.ai", new FileData
                                    {
                                        MD5 = "f9a1de349626100bb867a980e948e097"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Screens/Brno_capt0029.tif", new FileData
                                    {
                                        MD5 = "8a4c5252e0e64ae9d0e1240e7757dc71"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Screens/Lemans_capt0032.tif", new FileData
                                    {
                                        MD5 = "789962955a88170600630e5d606245c6"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Screens/Motegi_capt0022.tif", new FileData
                                    {
                                        MD5 = "2392946184671739dc8767bf8a820a70"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Screens/Nelson_capt0010.tif", new FileData
                                    {
                                        MD5 = "22db54691b19fcb036ce21bc23beb974"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Screens/Paul_capt0011.tif", new FileData
                                    {
                                        MD5 = "00ba6d824580b71ed5c4c3325b6565ce"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Screens/Sachsenring_capt0000.tif", new FileData
                                    {
                                        MD5 = "1258a6fc7182d99a29a9fdd62bfbcce4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Screens/Sachsenring_capt0023.tif", new FileData
                                    {
                                        MD5 = "ca041bf032e74c9deb0eaaac6536ae95"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Screens/Sepang_capt0010.tif", new FileData
                                    {
                                        MD5 = "b6e5868e2d42e9fbde2965e45524ed8f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Screens/Suzuka_capt0015.tif", new FileData
                                    {
                                        MD5 = "a365cb5fd415928f85f4e3cbbb5f6de0"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Screens/Valencia_capt0014.tif", new FileData
                                    {
                                        MD5 = "67bfbbeb8ba6785ee17a7091e8b074c4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/MotoGP4/Sell sheet/MotoGP4 SS.pdf", new FileData
                                    {
                                        MD5 = "8dbb336aebfe0df8944e237d50bce7c6"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Box Front/NM50th_PS2_pack.jpg",
                                    new FileData
                                    {
                                        MD5 = "bc3b84f6919c788de0415d69ae9c881a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Logo/NM50th Logo.psd",
                                    new FileData
                                    {
                                        MD5 = "8174620d070b72bcbf4c54eec8ee367c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/Bosconian 1.png",
                                    new FileData
                                    {
                                        MD5 = "5b2ce13bebc549cc4b138b5bcf7998cb"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/DigDug 2.png",
                                    new FileData
                                    {
                                        MD5 = "93a53ba7028b7d10e0d92a4a5709b1f3"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/DragonSpirit 3.png",
                                    new FileData
                                    {
                                        MD5 = "981706f059e1846a02d513d0dfd527c3"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/Galaga 2.png",
                                    new FileData
                                    {
                                        MD5 = "0e67cb04819f1c84b839ef2dbe1ab6a8"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/Mappy 3.png",
                                    new FileData
                                    {
                                        MD5 = "564dfd55b0200bb584f869a13c67fa14"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/mspacman 1.png",
                                    new FileData
                                    {
                                        MD5 = "b7402e427fe12b601407a71fbe7313a9"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/mspacman 3.gif",
                                    new FileData
                                    {
                                        MD5 = "fdd23d8f42447e7863df9e15c7bf8da5"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/Pac-Man 2.png",
                                    new FileData
                                    {
                                        MD5 = "290ec387a464c47c20ec0bcbbd7043c0"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/Pole Position 4.png",
                                    new FileData
                                    {
                                        MD5 = "f6aa3c00ba1a4d132f542be24d9a9916"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/rollingthunder_5.gif",
                                    new FileData
                                    {
                                        MD5 = "8a2d7ffaaf3c92f8c512fcb84d7b4bb8"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/Sky_Kid 2.png",
                                    new FileData
                                    {
                                        MD5 = "ce648f4fb2e9c92af425daf9447c8a00"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/Thumbs.db",
                                    new FileData
                                    {
                                        MD5 = "0fe4a9719aab5c37eda7b04b7a716f03"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Namco 50th E3 Press Kit/wSuper_Pac-Man.png",
                                    new FileData
                                    {
                                        MD5 = "76e35b2a26196232312e3a2da64065f4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Screens/Thumbs.db",
                                    new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum 50th Anniversary Collection/Sell sheet/NM50th SS.pdf",
                                    new FileData
                                    {
                                        MD5 = "0bb8576e4f9ac12619b086292b69b0bb"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Logos/NM50th Logo.psd",
                                    new FileData
                                    {
                                        MD5 = "8174620d070b72bcbf4c54eec8ee367c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/DIG DUG arrange 2.BMP",
                                    new FileData
                                    {
                                        MD5 = "9d6de0ad41d571c22b6e16274970da46"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/DIG DUG arrange wireless.bmp",
                                    new FileData
                                    {
                                        MD5 = "969c3db1c7e28900bf271c9166303e3e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/DIG DUG arrange.bmp",
                                    new FileData
                                    {
                                        MD5 = "3aa139927912810884428498c35f23d0"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Dig Dug Horizontal.bmp",
                                    new FileData
                                    {
                                        MD5 = "ee93768c88965772624ee81f908279aa"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Galaga arrange wireless.bmp",
                                    new FileData
                                    {
                                        MD5 = "d75079775f5cac5ac7a3ee0648659caf"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Galaga arrange.bmp",
                                    new FileData
                                    {
                                        MD5 = "7c0666afbf9e07fa3e7d211124fcbc52"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Galaga.bmp",
                                    new FileData
                                    {
                                        MD5 = "3118b1e48705058331e49d97197c487c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Galaxian.bmp",
                                    new FileData
                                    {
                                        MD5 = "35990987e423122f6485a0d2d0c08224"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Ms Pac Man horizontal.bmp",
                                    new FileData
                                    {
                                        MD5 = "b3c7fa9c8d7dc7c25f9f4e1f2585d86c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/New Rally X.bmp",
                                    new FileData
                                    {
                                        MD5 = "e62c0adec3293c79b7b2f9fddfcbf6b9"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/New RALLY-X arrange wireless.bmp",
                                    new FileData
                                    {
                                        MD5 = "3e07fe6bc7f5acf4f89b2ff032f44a9a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/New RALLY-X arrange.bmp",
                                    new FileData
                                    {
                                        MD5 = "645449e9b5fdf6b65cca330e3e87cd19"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Pac Man Arrange 2.bmp",
                                    new FileData
                                    {
                                        MD5 = "1d7ac87e7209f0ca7a473c059bfceb81"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Pac Man Arrange Wireless.bmp",
                                    new FileData
                                    {
                                        MD5 = "7ceef5f72cc15dbc7b0c85c62116bdf3"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Pac Man Arrange.bmp",
                                    new FileData
                                    {
                                        MD5 = "bb25dab9db541ec0673959dfc8c49296"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Pac Man.bmp",
                                    new FileData
                                    {
                                        MD5 = "cf5100405f58e02d810f4db2abb1e086"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Rally X.bmp",
                                    new FileData
                                    {
                                        MD5 = "c8601f9dd804f7a51ad9e79c396f391d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Screens/Thumbs.db",
                                    new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Namco Museum Battle Collection/Sell sheet/NMBC SS.pdf",
                                    new FileData
                                    {
                                        MD5 = "7d31739a462151a4e95825989d0ed5c0"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Logo/Pac'N Roll Logo.jpg", new FileData
                                    {
                                        MD5 = "97b3aed019a5f72b7bdfe0b692127f8d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Screens/CastlePac.bmp", new FileData
                                    {
                                        MD5 = "147a6ed9adbc64dbc2f420c76224a699"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Screens/FatherHat-2.bmp", new FileData
                                    {
                                        MD5 = "c6eb20163e70d95eacbe98c42ce3da40"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Screens/Original-mode.bmp", new FileData
                                    {
                                        MD5 = "28196cbbe0c622906f9ac08fb466c172"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Screens/Pac-Knight-1.bmp", new FileData
                                    {
                                        MD5 = "4b202f04b51a9add06a811828b661a4a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Screens/W1-3.bmp", new FileData
                                    {
                                        MD5 = "9d17753a0146a22841fb247570616441"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Screens/W1-7.bmp", new FileData
                                    {
                                        MD5 = "320576d6786ec641973246d9e50627c8"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Screens/W4-1.bmp", new FileData
                                    {
                                        MD5 = "cf664719a4e3a0da21598f8372aa67b7"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Screens/W5-3.bmp", new FileData
                                    {
                                        MD5 = "916bc21331633f49ebb015c9bc7dcfcc"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Screens/W5-8.bmp", new FileData
                                    {
                                        MD5 = "2f383a2a536e614e726ca094c8e150af"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Screens/W6-1.bmp", new FileData
                                    {
                                        MD5 = "d6c8eaf08711fd43b473e1db1cde231d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac N Roll/Sell sheet/PacNRoll SS.pdf", new FileData
                                    {
                                        MD5 = "ea9456ecb8586ead6e451c1b6821a683"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Backgrounds/InGameLandscape18Mar05a.bmp",
                                    new FileData
                                    {
                                        MD5 = "7e236dbff5a168a66fc4146a2a6cd29c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Backgrounds/InGameLandscape18Mar05c.bmp",
                                    new FileData
                                    {
                                        MD5 = "e998937ba333cc275648be6bc0a62bec"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Backgrounds/InGameLandscape18Mar05d.bmp",
                                    new FileData
                                    {
                                        MD5 = "adca6d86a121e392b01ab240fc2c8f4b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Characters/Ghost - Inky.bmp", new FileData
                                    {
                                        MD5 = "663b720d31b56347b3b4dfdf76c74e9e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Characters/NM50th Logo.psd", new FileData
                                    {
                                        MD5 = "8174620d070b72bcbf4c54eec8ee367c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Characters/Pac_punch16Mar05.tga", new FileData
                                    {
                                        MD5 = "33ccc664889a1c8b975e2f3811d80577"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Characters/Pac-image(PM only).jpg",
                                    new FileData
                                    {
                                        MD5 = "fa92333b5f73384b12247ac065207b62"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Characters/Pac-thumb-up16Mar05.tga",
                                    new FileData
                                    {
                                        MD5 = "243f40d3f76a284eb31223b13c079df2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Characters/Pac&pac-dot16Mar05.tga",
                                    new FileData
                                    {
                                        MD5 = "956dda6601c4e65c705f44eaf12bc0ae"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Characters/Thumbs.db", new FileData
                                    {
                                        MD5 = "89161af24fccb15374d1ab32616d6d97"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Concept Art/Copy of ErwinConcept 21Dec04.jpg",
                                    new FileData
                                    {
                                        MD5 = "13350298abaf8c5852fd21b49c136877"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Concept Art/ErwinConcept 21Dec04.jpg",
                                    new FileData
                                    {
                                        MD5 = "13350298abaf8c5852fd21b49c136877"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Concept Art/Thumbs.db", new FileData
                                    {
                                        MD5 = "996f15ab2f890ff979e839cea41b358d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Logo/PMW3 Logo.jpg", new FileData
                                    {
                                        MD5 = "96a4cd886faf6136657f1ff1c929e91a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Logo/Thumbs.db", new FileData
                                    {
                                        MD5 = "2090e87053eadcf030c3b38b4fd18142"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Pac-Man 25th Anniversary logo/PM 25th Seal v2.jpg",
                                    new FileData
                                    {
                                        MD5 = "0d092397e23f28ab7eafb8bf20c04b44"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/GC/scr115013(1of1).bmp", new FileData
                                    {
                                        MD5 = "7cfba722acd78b655bac9178cac2c9ef"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/GC/scr16926(1of1).bmp", new FileData
                                    {
                                        MD5 = "491f3b5c3c0327730aaa9f955b5bd465"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/GC/scr27830(1of1).bmp", new FileData
                                    {
                                        MD5 = "f13a0343b812d43fd25750009cf52495"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/GC/scr50502(1of1).bmp", new FileData
                                    {
                                        MD5 = "e37bf80f2ca0223009de53ec446b1dca"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/GC/scr58437(1of1).bmp", new FileData
                                    {
                                        MD5 = "17130320bb8ca71e62e0ee09042e95bd"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/GC/scr7035(1of1).bmp", new FileData
                                    {
                                        MD5 = "ddb356f4e1e0577b1a6da62c59a41272"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/GC/scr78422(1of1).bmp", new FileData
                                    {
                                        MD5 = "d5f41a6f6fcebeccd2c6f38228e8726f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/PS2/scr104100.jpg", new FileData
                                    {
                                        MD5 = "b2c2fa769f9966e0c8d517fe46100f34"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/PS2/scr11273.jpg", new FileData
                                    {
                                        MD5 = "d03f6dae534c2eeff6c2e8fce5783733"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/PS2/scr119317.jpg", new FileData
                                    {
                                        MD5 = "21c7caa7a17a5da6c3d509c42fe3053b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/PS2/scr120246.jpg", new FileData
                                    {
                                        MD5 = "762fecfa0cdd2b6d74d0a95c736651cd"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/PS2/scr134509.jpg", new FileData
                                    {
                                        MD5 = "157fe3ecc7875030937b6fda1a9f6195"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/PS2/scr141404.jpg", new FileData
                                    {
                                        MD5 = "4f8bcc5e9c027596a17c3aa55849b146"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/PS2/scr80829.jpg", new FileData
                                    {
                                        MD5 = "d77bb4fa44af80d81e1e93a23d549dc4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/PSP/scr22605(1of1).bmp", new FileData
                                    {
                                        MD5 = "becd2e1bcb073ac60ecec0daf895e00e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/PSP/scr26894(1of1).bmp", new FileData
                                    {
                                        MD5 = "860b50df0c915320b309f342d8657034"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/PSP/scr3346(1of1).bmp", new FileData
                                    {
                                        MD5 = "08d275a4e739a817f325835151cc19f0"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/PSP/scr6103(1of1).bmp", new FileData
                                    {
                                        MD5 = "282c8a144db55a7ac3fd1272756c4623"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/Xbox/scr27150(1of1).bmp", new FileData
                                    {
                                        MD5 = "e8b51222e64b7c753049f1361844c1a1"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/Xbox/scr31531(1of1).bmp", new FileData
                                    {
                                        MD5 = "52065f46167762668a21108bce1a5022"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/Xbox/scr388285(1of1).bmp",
                                    new FileData
                                    {
                                        MD5 = "77105603a525cf8b793b9ff12a1fcc6f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Screens/Xbox/scr7683(1of1).bmp", new FileData
                                    {
                                        MD5 = "03e9bd1c27d20fc4a9c1b218715a6798"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Pac-Man World 3/Sell sheet/PMW3 SS.pdf", new FileData
                                    {
                                        MD5 = "91569c69ee0b8268369da0de7b2ecc02"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Logo/PP&C Logo v1.jpg", new FileData
                                    {
                                        MD5 = "0bb5713b30b877155a5327dea49fdb1e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Screens/Casino_Screenshot_00.bmp",
                                    new FileData
                                    {
                                        MD5 = "a6b46c29e4fedda0063a6f4e9bac2b79"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Screens/Casino_Screenshot_05.bmp",
                                    new FileData
                                    {
                                        MD5 = "f8596f38272d7594731d92f80806a961"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Screens/Casino_Screenshot_06.bmp",
                                    new FileData
                                    {
                                        MD5 = "46fc16c77a7707daeffc86888b96ab8a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Screens/Casino_Screenshot_07.bmp",
                                    new FileData
                                    {
                                        MD5 = "39f11c1b64ae46898592bb70dfe52e06"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Screens/Casino_Screenshot_09.bmp",
                                    new FileData
                                    {
                                        MD5 = "6d01a66430342028de15aaf21066ac79"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Screens/Casino_Screenshot_10.bmp",
                                    new FileData
                                    {
                                        MD5 = "285f8052252512591188a9723362d6de"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Screens/Casino_Screenshot_12.bmp",
                                    new FileData
                                    {
                                        MD5 = "fb0de95ea52eea296a121e2a895ea837"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Screens/Casino_Screenshot_17.bmp",
                                    new FileData
                                    {
                                        MD5 = "7e6245a1ff8bff970f10f0c8446dd0a2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Screens/Casino_Screenshot_28.bmp",
                                    new FileData
                                    {
                                        MD5 = "269d82c5a552cdb463b0484ed43ce22c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Screens/Casino_Screenshot_51.bmp",
                                    new FileData
                                    {
                                        MD5 = "8a94577a8abbb3fec84b6f86fc4e0cf7"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Payout Poker and Casino/Sell sheet/Payout SS.pdf",
                                    new FileData
                                    {
                                        MD5 = "f7853aff3127c82ce23d1406e20ebe4c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Logo/Shogun Logo New Final.jpg",
                                    new FileData
                                    {
                                        MD5 = "5b674d1ea3b2f5cb801f132014b15146"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_01.jpg",
                                    new FileData
                                    {
                                        MD5 = "78079025476753f8a06389399ba32b67"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_02.jpg",
                                    new FileData
                                    {
                                        MD5 = "a7b3ce67b59756b01269a4b09d153e2f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_03.jpg",
                                    new FileData
                                    {
                                        MD5 = "ef6d94da42175fa15faabb5319d43271"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_04.jpg",
                                    new FileData
                                    {
                                        MD5 = "22e97ebb272bdc0d89bf20b379b3ab74"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_05.jpg",
                                    new FileData
                                    {
                                        MD5 = "617eb234f5db4d1c7df2f302b0e707c3"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_06.jpg",
                                    new FileData
                                    {
                                        MD5 = "beb3ff6b93bcdedb7d5b59c2ab0b24da"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_07.jpg",
                                    new FileData
                                    {
                                        MD5 = "7487327f348c91d6d5c70159deaa1a05"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_08.jpg",
                                    new FileData
                                    {
                                        MD5 = "8e10cdead5de0e1d36072f0a72b6618c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_09.jpg",
                                    new FileData
                                    {
                                        MD5 = "959df221a2d73c7ae44c999ef05894ed"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_10.jpg",
                                    new FileData
                                    {
                                        MD5 = "502460fe3d92eec6ce8e63e81f3b001f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_11.jpg",
                                    new FileData
                                    {
                                        MD5 = "d740b936e1eda48b1bb3a5aabbc46a17"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_12.jpg",
                                    new FileData
                                    {
                                        MD5 = "745b04c4b35cf2f0e8b1761b8b4a030b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_13.jpg",
                                    new FileData
                                    {
                                        MD5 = "e13b19418ca2711ee18faae35b3c3a7c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_14.jpg",
                                    new FileData
                                    {
                                        MD5 = "08dc8721549501c09e7a365e2b272f82"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_15.jpg",
                                    new FileData
                                    {
                                        MD5 = "3259503dbf999e5d399c3f04620926f8"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/SW_042905_16.jpg",
                                    new FileData
                                    {
                                        MD5 = "9a22d83c4693666fd88d7403f556c53b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Screens/Thumbs.db",
                                    new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Real Time Conflict - Shogun Empires/Sell sheet/Shogun SS.pdf",
                                    new FileData
                                    {
                                        MD5 = "d8426f75975145ce5bf109418563afdf"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Characters/Rebelstar_Keyart.psd",
                                    new FileData
                                    {
                                        MD5 = "99b9ac2b90e327e277ae28454aa28912"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Characters/Thumbs.db", new FileData
                                    {
                                        MD5 = "89161af24fccb15374d1ab32616d6d97"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Logo/RebelStarLogo2.psd",
                                    new FileData
                                    {
                                        MD5 = "68d828d4392ba289daa48993e2364701"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens/Rebelstar_042705_01.png",
                                    new FileData
                                    {
                                        MD5 = "8a1f49fc21444454842caf69c57d426b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens/Rebelstar_042705_03.png",
                                    new FileData
                                    {
                                        MD5 = "8fbcd8cdac7cefe6faecfc6de90d04d3"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens/Rebelstar_042705_04.png",
                                    new FileData
                                    {
                                        MD5 = "2d08940475dddad1d35176d5e399d803"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens/Rebelstar_042705_15.png",
                                    new FileData
                                    {
                                        MD5 = "bfd5a3d7eab3ee24f1d9480c0612a159"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens/Rebelstar_042705_16.png",
                                    new FileData
                                    {
                                        MD5 = "fcdfc4e4ccb5a850d025516337542afd"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens/Rebelstar_042705_35.png",
                                    new FileData
                                    {
                                        MD5 = "00f13167635f548ca8bd049a243c7fbc"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens/Rebelstar_042705_38.png",
                                    new FileData
                                    {
                                        MD5 = "249e4207a94f817b6a1071e067264746"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens/Rebelstar_042705_50.png",
                                    new FileData
                                    {
                                        MD5 = "2088ba4eacca29c0db9e5e0e9c8886e2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens/Rebelstar_042705_67.png",
                                    new FileData
                                    {
                                        MD5 = "32ca9cfdb786152b976564e035cdc4bc"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens/Rebelstar_042705_70.png",
                                    new FileData
                                    {
                                        MD5 = "dc137cef6fed3269150f4fbf192b0d27"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Rebelstar Tactical Command/Sell sheet/RS SS.pdf", new FileData
                                    {
                                        MD5 = "7051316d6d8ac624d4e0dd769908cc96"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Box Front/SSS_pack_front_for_promo_approval.gif",
                                    new FileData
                                    {
                                        MD5 = "cba6c6cbde38bd4bfd5308f785df8ab4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Box Front/Thumbs.db", new FileData
                                    {
                                        MD5 = "996f15ab2f890ff979e839cea41b358d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Logo/Final Logo on BG.psd", new FileData
                                    {
                                        MD5 = "fa95056562bd57191a720aa6b2de0103"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Logo/SSS - Final Logo on White.psd",
                                    new FileData
                                    {
                                        MD5 = "a007c128b4237f51dd67f864e2212744"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Screens/Desert1.png", new FileData
                                    {
                                        MD5 = "3a968a2af146b139cd50ac6f9073e623"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Screens/EarthBase2.png", new FileData
                                    {
                                        MD5 = "915b659a3acc03aec8bcf0f2ac39c3b2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Screens/EarthShooter3.png", new FileData
                                    {
                                        MD5 = "0f1a92d3a21b71bf2559cf199f09af53"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Screens/KrillBattleworm2.png", new FileData
                                    {
                                        MD5 = "ca2a33021c2d55d5f3a7cccdac1e93fb"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Screens/SSSROM032805_13.png", new FileData
                                    {
                                        MD5 = "50ecd13d69951ac2cdac9f62b4290298"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Screens/SSSROM032805_88.png", new FileData
                                    {
                                        MD5 = "cf94a5160fd4bf083a7a82c84820f46e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Screens/SSSROM042805_231.png", new FileData
                                    {
                                        MD5 = "91f7ae32d68bba0b4fa9243cadcc9fea"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Screens/SSSROM042805_242.png", new FileData
                                    {
                                        MD5 = "15979b10a78c1f9695ba1adeed17bf4b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Screens/Starbase1_2.png", new FileData
                                    {
                                        MD5 = "8ed20b45117852b6e5f23d791accd23e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sigma Star Saga/Sell sheet/SSS SS.pdf", new FileData
                                    {
                                        MD5 = "38453c5315baaa42f55261157bb9f0f7"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Concept Art/sniper-finish-alt cmyk-small.jpg",
                                    new FileData
                                    {
                                        MD5 = "a10af5c515a653f7acc171ebd66ca43f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Concept Art/Thumbs.db", new FileData
                                    {
                                        MD5 = "996f15ab2f890ff979e839cea41b358d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Logo/se_logo finish_flat.jpg", new FileData
                                    {
                                        MD5 = "8af021c6cd71785d10e7f75760e57d39"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PC/scope 1.bmp", new FileData
                                    {
                                        MD5 = "296daa47c5bdcf6bd990856a9cc6e6f4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PC/scope 2.bmp", new FileData
                                    {
                                        MD5 = "9aa904a38b60cb04c9a40788d7ebdb0f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PC/SniperElite 2005-03-08 12-23-18-72.bmp",
                                    new FileData
                                    {
                                        MD5 = "42d123e39db370fa5549c0e9c8f58ec2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PC/SniperElite 2005-03-08 12-30-16-86.bmp",
                                    new FileData
                                    {
                                        MD5 = "e3fa0476927d67c87b0ba46dddc2f839"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PC/SniperElite 2005-03-08 13-02-41-69.bmp",
                                    new FileData
                                    {
                                        MD5 = "6c379adb997c6f2c35002d634baababb"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PC/SniperElite 2005-03-08 18-02-31-07.bmp",
                                    new FileData
                                    {
                                        MD5 = "f91e296b6ac1748faa44b06ceea7213a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PC/SniperElite 2005-03-08 18-26-22-49.bmp",
                                    new FileData
                                    {
                                        MD5 = "86747f89be6b5f8c4bfdc43718d9d14c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PC/SniperElite 2005-03-08 18-38-41-90.bmp",
                                    new FileData
                                    {
                                        MD5 = "a66abd9748e69a96afe20d42fe3985a8"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PC/SniperElite 2005-03-16 14-48-27-68.bmp",
                                    new FileData
                                    {
                                        MD5 = "6c6b9d514cb4d2c0083b7d28a3170ba6"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PC/SniperElite 2005-03-16 14-58-16-69.bmp",
                                    new FileData
                                    {
                                        MD5 = "730c0628356863d52bfc3464f724245e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PC/Thumbs.db", new FileData
                                    {
                                        MD5 = "5caab4438f5e9e9321e4b8a733adec9a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PS2/PS2/Headshot 1 230305.bmp",
                                    new FileData
                                    {
                                        MD5 = "7229659d322449ff565f1f7c7ff4a972"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PS2/PS2/Headshot 2 230305.bmp",
                                    new FileData
                                    {
                                        MD5 = "ceb0f8e422599550567fe312435519bd"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PS2/PS2/Screen000.bmp", new FileData
                                    {
                                        MD5 = "871123dd1e02b3e0620de9632797b0f9"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PS2/PS2/Screen006.bmp", new FileData
                                    {
                                        MD5 = "a24985d6cc2a8c097be5ccc8c361cb3c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PS2/PS2/Screen011.bmp", new FileData
                                    {
                                        MD5 = "727166f37caffe8238ef984256372bcc"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PS2/PS2/Screen012.bmp", new FileData
                                    {
                                        MD5 = "2a7cb8f6d34f67b9839fe56044aeed13"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PS2/PS2/Screen024.bmp", new FileData
                                    {
                                        MD5 = "95a9c95815ef15d4120cd26b1dca6089"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PS2/PS2/Screen035.bmp", new FileData
                                    {
                                        MD5 = "8a8e96d16cb520579ab15bfccb53e9e4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PS2/PS2/Screen042.bmp", new FileData
                                    {
                                        MD5 = "2e88e2075633d8589c095f206ec13cf5"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/PS2/PS2/Thumbs.db", new FileData
                                    {
                                        MD5 = "3789a270744c18770b09a4d92bfa0607"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/1.bmp", new FileData
                                    {
                                        MD5 = "e663b202a7bf21c85c76607f8a00c162"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/10.bmp", new FileData
                                    {
                                        MD5 = "bdb77ed8e47ec3115f32259f376d8696"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/12.bmp", new FileData
                                    {
                                        MD5 = "5a127fb2a03ddcb58be14e557ea82ac2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/13.bmp", new FileData
                                    {
                                        MD5 = "f3599f982bfef023ff1f044bbdd2045e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/2.bmp", new FileData
                                    {
                                        MD5 = "ab2b50d372247733ae5dc73a11bed38d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/3.bmp", new FileData
                                    {
                                        MD5 = "4ca418b657666601ec6c1a3a7146b2c2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/4.bmp", new FileData
                                    {
                                        MD5 = "c0f30408104d504fad38a80543b559d4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/7.bmp", new FileData
                                    {
                                        MD5 = "9c6358b4c0edbfd74cb964323f615d9f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/8.bmp", new FileData
                                    {
                                        MD5 = "276baa40a815c4fc2f6fc5312cef900e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/9.bmp", new FileData
                                    {
                                        MD5 = "d43333f96c3dd0854f547105ed26ff88"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/birds.bmp", new FileData
                                    {
                                        MD5 = "c4f0cf91c1f6d7ec4d0d0ab35759d824"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/boom2.bmp", new FileData
                                    {
                                        MD5 = "30f99f12875e953c76d3f466b0f5b98e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/sniper xbox 4.bmp", new FileData
                                    {
                                        MD5 = "a30c3c0e0ba4f93823f1e7f6c038c6b3"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/sniper12.bmp", new FileData
                                    {
                                        MD5 = "520d68b8606beaa5a00000eadfff7b8f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Screens/Xbox/sniper13.bmp", new FileData
                                    {
                                        MD5 = "650fd8eac6c20b2b01e4673ce0d2bfdb"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Sniper Elite/Sell sheet/Sniper Elite SS.pdf", new FileData
                                    {
                                        MD5 = "3c00d8e44bde5661d917dc4557fc72e1"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Characters/SC33D_setsuka001rev.psd",
                                    new FileData
                                    {
                                        MD5 = "a2b846d6129615ded5baf512bbce1e4a"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Characters/SC33D_zasalamel001.psd",
                                    new FileData
                                    {
                                        MD5 = "ff2ce06b8e3347a02f001b4da55e9127"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Characters/Thumbs.db", new FileData
                                    {
                                        MD5 = "89161af24fccb15374d1ab32616d6d97"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Characters/Tira.psd", new FileData
                                    {
                                        MD5 = "6234b4dfa4b08795b2efd96bd9b8f3ff"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Logo/SC3_Logo1.jpg", new FileData
                                    {
                                        MD5 = "2bae52fa7287de45dab77aeca32b21cb"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/c4_0418084834sm.bmp", new FileData
                                    {
                                        MD5 = "6d81523c38c33b16a222b5e340343459"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/c6_0414055833sm.bmp", new FileData
                                    {
                                        MD5 = "a7e93435ff2af3b94fd0d5c7714f1f52"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/s1_0301104334_00.bmp", new FileData
                                    {
                                        MD5 = "c6574d340c382b987f6fae05302cef96"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/S1_0301114752_00.bmp", new FileData
                                    {
                                        MD5 = "6da155eff57b46c6da1f3162a9246ecd"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/s1_0301115648_00R.bmp", new FileData
                                    {
                                        MD5 = "58cd5607cdc4c38258f8f15762c1bb4f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/s1_0301120230_00R.bmp", new FileData
                                    {
                                        MD5 = "a7983f354357b6350770a1b8d93b8000"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/s1_0302083833_00R.bmp", new FileData
                                    {
                                        MD5 = "130832a63fc104bd001372ae2a2f5dc8"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/s1_0302090056_00R.bmp", new FileData
                                    {
                                        MD5 = "e4d7af519e47168f4e8e48cdabb1ce87"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/s1_0302092232_00.bmp", new FileData
                                    {
                                        MD5 = "9bf76c72c77613f00584855143137092"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/s1_0325080225_00.bmp", new FileData
                                    {
                                        MD5 = "565ce28f86f5567e3b44d2709b89eeb9"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/s1_0427175327_00.bmp", new FileData
                                    {
                                        MD5 = "feb2f54d395c43722cdc298f1a053a90"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/SOULCALIBUR III/Sell sheet/SC SS.pdf", new FileData
                                    {
                                        MD5 = "7078f9d8e7a89de3d51a5f05e4383cf1"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Logo/SG1_Alliance logo 19-4-5.JPG",
                                    new FileData
                                    {
                                        MD5 = "a0264df94d176524d28ae26940bfa175"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/Alliance_Jan2005_19.jpg",
                                    new FileData
                                    {
                                        MD5 = "bc97bcc14f1d9c32cbdaf3a4404e329d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/M12_train_tunnel_gate.JPG",
                                    new FileData
                                    {
                                        MD5 = "783db75bba80e671ec12b717302b4e3c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/M12_vehplant_06.jpg",
                                    new FileData
                                    {
                                        MD5 = "fd7b18a42f4fe17f677c79a8715e86dd"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/NTShot07.jpg",
                                    new FileData
                                    {
                                        MD5 = "bfff7912fb6d105e582cd348be08ac4b"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/SG1_Alliance_aim_wlogo.jpg",
                                    new FileData
                                    {
                                        MD5 = "58044971675fe378d73acc24159bd714"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/SG1_Alliance_cave_chaos_wlogo.jpg",
                                    new FileData
                                    {
                                        MD5 = "778b261415ac3fb5605326183f750424"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/SG1_Alliance_grassattack_wlogo.jpg",
                                    new FileData
                                    {
                                        MD5 = "c6bad5d46ed2bd44da144526d03d0ec2"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/SG1_Alliance_jaffa_firing_03_wlogo.jpg",
                                    new FileData
                                    {
                                        MD5 = "853534dff1cc8e2fdbe98b6dc0a0fa5e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/SG1_Alliance_M04_5.jpg",
                                    new FileData
                                    {
                                        MD5 = "f4b7828e57f69f15f7f3df945e96ab9e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/SG1_Alliance_NOV2004_10.jpg",
                                    new FileData
                                    {
                                        MD5 = "567180aec0ff2697d44fb7ab84b8dd5d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/SG1_Alliance_NOV2004_5.jpg",
                                    new FileData
                                    {
                                        MD5 = "ee58ddac72b261813c55984f960e8033"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/Shot00026.JPG",
                                    new FileData
                                    {
                                        MD5 = "3d65a228ed48a4f6dfaa1f1914edf96e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/Shot00053.JPG",
                                    new FileData
                                    {
                                        MD5 = "59e5945a7d607e9c3b1ca09e6e01c068"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/Shot00055.JPG",
                                    new FileData
                                    {
                                        MD5 = "efc298c865c2bc2b60789c1860f8b494"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/Shot00065.JPG",
                                    new FileData
                                    {
                                        MD5 = "58b98b035a70b9051f22160a4d5f81d5"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/PC/Shot00077.JPG",
                                    new FileData
                                    {
                                        MD5 = "cf3a6cf24340c9ead1f340fac5d71aaf"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Stargate SG-1 The Alliance/Sell sheet/Stargate SS.pdf",
                                    new FileData
                                    {
                                        MD5 = "3ac6169d76529bcace4311ba73ef707c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Tale of Legendia/Logo/TOL_logo_cmyk_small.psd", new FileData
                                    {
                                        MD5 = "cbf1f93313ace673dbecc5e1f2a9ba43"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Tale of Legendia/Sell Sheet/ToL2 SS.pdf", new FileData
                                    {
                                        MD5 = "36a0cd4329540dc5a564bc51a9e3197d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Urban Reign/Logo/UR_logo.jpg", new FileData
                                    {
                                        MD5 = "f50f7bb09fa1b0812d7e8ef6c61c7cb5"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Urban Reign/Screens/Urban_Reign_7Apr05_07.bmp", new FileData
                                    {
                                        MD5 = "9c2a3cacff2e6e973e6a25600f774489"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Urban Reign/Screens/Urban_Reign_7Apr05_08.bmp", new FileData
                                    {
                                        MD5 = "fe9ad28241e7a4c3016573cc7c1dbf29"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Urban Reign/Screens/Urban_Reign_7Apr05_09.bmp", new FileData
                                    {
                                        MD5 = "79676279db885856aadcc77620b06ab6"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Urban Reign/Screens/Urban_Reign_7Apr05_11.bmp", new FileData
                                    {
                                        MD5 = "cfefcd4ab6ce02221ca1d28e16a96ae0"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Urban Reign/Screens/Urban_Reign_7Apr05_15.bmp", new FileData
                                    {
                                        MD5 = "888569e443b8038512171f87c2ac153e"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/Urban Reign/Sell sheet/Urban Reign SS.pdf", new FileData
                                    {
                                        MD5 = "e377642d8f993e5ff6d9e892fd0c3a8c"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Characters/Prince.psd", new FileData
                                    {
                                        MD5 = "85200a93f9ae7b5f540376f2ac619255"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Characters/Thumbs.db", new FileData
                                    {
                                        MD5 = "89161af24fccb15374d1ab32616d6d97"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Characters/UMA_KING_02psd.psd", new FileData
                                    {
                                        MD5 = "179660eb29f75ae1de179842309e6f69"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/04.bmp", new FileData
                                    {
                                        MD5 = "37309d6de91533f706fc93d99bee2db5"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/2P-1.bmp", new FileData
                                    {
                                        MD5 = "972db6873f0ae181c7df904681171fa3"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/2P-2.bmp", new FileData
                                    {
                                        MD5 = "5c9230fd172bdae0645e003332a689fb"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/big1-1.bmp", new FileData
                                    {
                                        MD5 = "2fd3445b76a92607f1be7088d9e27b89"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/big4-1.bmp", new FileData
                                    {
                                        MD5 = "4bf290321ee959ec06015472bbdc499d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/big5-1.bmp", new FileData
                                    {
                                        MD5 = "3c7d5d286b0b2e8f41737e90cdfeb6ee"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/big5-3.bmp", new FileData
                                    {
                                        MD5 = "6606406da5eef754aadf930fa40bc2bc"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/lake02.bmp", new FileData
                                    {
                                        MD5 = "7a9a77a0bad5011358ed96941c879688"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/lake10.bmp", new FileData
                                    {
                                        MD5 = "47fca5772a98d47253e8dff9ad3db50d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/snow-2.bmp", new FileData
                                    {
                                        MD5 = "8acdf8175c6295c1e95c2e22dda40481"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/sweet03.bmp", new FileData
                                    {
                                        MD5 = "6661018f1eca1fb8af401e0524325aba"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/Thumbs.db", new FileData
                                    {
                                        MD5 = "dbee13a1543f59422d4fd1d3dcd01a4d"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/vs-1.bmp", new FileData
                                    {
                                        MD5 = "d1633acc85ae42f5f38fe30c3e3adb1f"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/vs-3.bmp", new FileData
                                    {
                                        MD5 = "3a58aab284dc43888da9f7c91c78a8f1"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/vs-4.bmp", new FileData
                                    {
                                        MD5 = "eb9d882d5c28efb5d7bf93e6fba1a9b4"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/vs-6.bmp", new FileData
                                    {
                                        MD5 = "c19c6a5b8b8f2becee806b776a6bee52"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Screens/zoo06.bmp", new FileData
                                    {
                                        MD5 = "80b0208781642d71f121c8c857e1d2d0"
                                    }
                                },
                                {
                                    "Namco E3 Assets Disc/We Love Katamari/Sell sheet/Katamari2 SS.pdf", new FileData
                                    {
                                        MD5 = "048b71e1ad2abfa02b7e9cbca494b2b0"
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