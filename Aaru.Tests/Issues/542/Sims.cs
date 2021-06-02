using System.Collections.Generic;
using System.IO;

// ReSharper disable StringLiteralTypo

namespace Aaru.Tests.Issues._542
{
    /* https://github.com/aaru-dps/Aaru/issues/542
     *
     * SilasLaspada commented on Feb 10, 2021
     *
     * When extracting an image of a SafeDisc protected CD, most files aren't properly extracted.
     */

    public class Sims : FsExtractHashIssueTest
    {
        protected override string DataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Fixed", "issue542", "sims");
        protected override string                     TestFile         => "The Sims.aaruf";
        protected override Dictionary<string, string> ParsedOptions    => new Dictionary<string, string>();
        protected override bool                       Debug            => false;
        protected override bool                       Xattrs           => false;
        protected override string                     Encoding         => null;
        protected override bool                       ExpectPartitions => true;
        protected override string                     Namespace        => null;

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
                            VolumeName = "The Sims",
                            Directories = new List<string>
                            {
                                "Data",
                                "Demos",
                                "DirectX",
                                "IP",
                                "launcher",
                                "Music",
                                "Music/Modes",
                                "Music/Modes/Build",
                                "Music/Modes/Buy",
                                "Music/Modes/Load",
                                "Music/Modes/Nhood",
                                "Music/Modes/NhoodUS",
                                "Music/Stations",
                                "Music/Stations/Classica",
                                "Music/Stations/Country",
                                "Music/Stations/Latin",
                                "Music/Stations/Rock",
                                "NPSPatch",
                                "Readme",
                                "Setup",
                                "Setup/English",
                                "Setup/Thai",
                                "Support",
                                "Support/SKU1",
                                "Support/SKU1/Spanish",
                                "Support/SKU1/USEnglish",
                                "Support/SKU2",
                                "Support/SKU2/Dutch",
                                "Support/SKU2/Dutch/finished version",
                                "Support/SKU2/French",
                                "Support/SKU2/French/finished version",
                                "Support/SKU2/German",
                                "Support/SKU2/German/finished version",
                                "Support/SKU2/Italian",
                                "Support/SKU2/Italian/finished version",
                                "Support/SKU2/Swedish",
                                "Support/SKU2/Swedish/finished version",
                                "Support/SKU2/UKEnglish",
                                "Support/SKU2/UKEnglish/finished version",
                                "Support/SKU3",
                                "Support/SKU3/Portuguese",
                                "Support/SKU3/Portuguese/finished version",
                                "Support/SKU3/Spanish",
                                "Support/SKU3/Spanish/finished version",
                                "Support/SKU3/USEnglish",
                                "Support/SKU3/USEnglish/finished version",
                                "WMP"
                            },
                            Files = new Dictionary<string, FileData>
                            {
                                {
                                    "00000001.TMP", new FileData
                                    {
                                        MD5 = "7cfe3652305153ccbade2cc74c1f1c9c"
                                    }
                                },
                                {
                                    "00000404.016", new FileData
                                    {
                                        MD5 = "b5c7efd347944b76d5858f29aaa5ab37"
                                    }
                                },
                                {
                                    "00000404.256", new FileData
                                    {
                                        MD5 = "df44b05f87f695aaf3f9054fc66450af"
                                    }
                                },
                                {
                                    "00000407.016", new FileData
                                    {
                                        MD5 = "69ec63414cdcc0d86002c2e0fed15c43"
                                    }
                                },
                                {
                                    "00000407.256", new FileData
                                    {
                                        MD5 = "476ae017d76da68c1f07d25d6bd6b184"
                                    }
                                },
                                {
                                    "00000409.016", new FileData
                                    {
                                        MD5 = "b5c7efd347944b76d5858f29aaa5ab37"
                                    }
                                },
                                {
                                    "00000409.256", new FileData
                                    {
                                        MD5 = "df44b05f87f695aaf3f9054fc66450af"
                                    }
                                },
                                {
                                    "0000040c.016", new FileData
                                    {
                                        MD5 = "3ae3ce03e0d362a42c031e8d105eadef"
                                    }
                                },
                                {
                                    "0000040c.256", new FileData
                                    {
                                        MD5 = "d5f902e2748052321d5da4f24b2d0c27"
                                    }
                                },
                                {
                                    "00000410.016", new FileData
                                    {
                                        MD5 = "b5c7efd347944b76d5858f29aaa5ab37"
                                    }
                                },
                                {
                                    "00000410.256", new FileData
                                    {
                                        MD5 = "df44b05f87f695aaf3f9054fc66450af"
                                    }
                                },
                                {
                                    "00000411.016", new FileData
                                    {
                                        MD5 = "a925c566e627a787a18e2be55b7cefe8"
                                    }
                                },
                                {
                                    "00000411.256", new FileData
                                    {
                                        MD5 = "3ad41bdfa97320011c626d9ed183b7f9"
                                    }
                                },
                                {
                                    "00000412.016", new FileData
                                    {
                                        MD5 = "b5c7efd347944b76d5858f29aaa5ab37"
                                    }
                                },
                                {
                                    "00000412.256", new FileData
                                    {
                                        MD5 = "df44b05f87f695aaf3f9054fc66450af"
                                    }
                                },
                                {
                                    "00000413.016", new FileData
                                    {
                                        MD5 = "b5c7efd347944b76d5858f29aaa5ab37"
                                    }
                                },
                                {
                                    "00000413.256", new FileData
                                    {
                                        MD5 = "df44b05f87f695aaf3f9054fc66450af"
                                    }
                                },
                                {
                                    "00000415.016", new FileData
                                    {
                                        MD5 = "b5c7efd347944b76d5858f29aaa5ab37"
                                    }
                                },
                                {
                                    "00000415.256", new FileData
                                    {
                                        MD5 = "df44b05f87f695aaf3f9054fc66450af"
                                    }
                                },
                                {
                                    "00000416.016", new FileData
                                    {
                                        MD5 = "b5c7efd347944b76d5858f29aaa5ab37"
                                    }
                                },
                                {
                                    "00000416.256", new FileData
                                    {
                                        MD5 = "df44b05f87f695aaf3f9054fc66450af"
                                    }
                                },
                                {
                                    "0000041d.016", new FileData
                                    {
                                        MD5 = "cadf4d58fa27587fa4e8575e585191db"
                                    }
                                },
                                {
                                    "0000041d.256", new FileData
                                    {
                                        MD5 = "cee2133178c1089b85c45039a7d2db91"
                                    }
                                },
                                {
                                    "0000041e.016", new FileData
                                    {
                                        MD5 = "cadf4d58fa27587fa4e8575e585191db"
                                    }
                                },
                                {
                                    "0000041e.256", new FileData
                                    {
                                        MD5 = "cee2133178c1089b85c45039a7d2db91"
                                    }
                                },
                                {
                                    "00000804.016", new FileData
                                    {
                                        MD5 = "b5c7efd347944b76d5858f29aaa5ab37"
                                    }
                                },
                                {
                                    "00000804.256", new FileData
                                    {
                                        MD5 = "df44b05f87f695aaf3f9054fc66450af"
                                    }
                                },
                                {
                                    "00000809.016", new FileData
                                    {
                                        MD5 = "b5c7efd347944b76d5858f29aaa5ab37"
                                    }
                                },
                                {
                                    "00000809.256", new FileData
                                    {
                                        MD5 = "df44b05f87f695aaf3f9054fc66450af"
                                    }
                                },
                                {
                                    "00000c0a.016", new FileData
                                    {
                                        MD5 = "cadf4d58fa27587fa4e8575e585191db"
                                    }
                                },
                                {
                                    "00000c0a.256", new FileData
                                    {
                                        MD5 = "cee2133178c1089b85c45039a7d2db91"
                                    }
                                },
                                {
                                    "autorun.inf", new FileData
                                    {
                                        MD5 = "22b64314899dfa9be2b7a437c6e20acf"
                                    }
                                },
                                {
                                    "clcd16.dll", new FileData
                                    {
                                        MD5 = "4de2636a761f57126da707aef6c9c51d"
                                    }
                                },
                                {
                                    "clcd32.dll", new FileData
                                    {
                                        MD5 = "2abd8b3c9ae3845cde0d1c09796f8776"
                                    }
                                },
                                {
                                    "clokspl.exe", new FileData
                                    {
                                        MD5 = "31f2b78f2a5e23eba0276675d1f482df"
                                    }
                                },
                                {
                                    "Data/data1.cab", new FileData
                                    {
                                        MD5 = "886f4c8ee47cd7e8a85fff68f6f7fa0a"
                                    }
                                },
                                {
                                    "Demos/STP4SP.mpg", new FileData
                                    {
                                        MD5 = "bc6662fb02372260c8daf3117aeadd31"
                                    }
                                },
                                {
                                    "DirectX/cfgmgr32.dll", new FileData
                                    {
                                        MD5 = "1fb1bc76d7802828c32a9e23a6e2118e"
                                    }
                                },
                                {
                                    "DirectX/directx.cab", new FileData
                                    {
                                        MD5 = "e05884e26cc93d9cda238d400c4f6c71"
                                    }
                                },
                                {
                                    "DirectX/directx.inf", new FileData
                                    {
                                        MD5 = "fa466ed9d0b990c083ac9de181100522"
                                    }
                                },
                                {
                                    "DirectX/dsetup.dll", new FileData
                                    {
                                        MD5 = "4aca1abd4a9441218ece8ecd5f86161f"
                                    }
                                },
                                {
                                    "DirectX/dsetup32.dll", new FileData
                                    {
                                        MD5 = "941e9464da90f2ac182388e59c462dc3"
                                    }
                                },
                                {
                                    "DirectX/DXMEDIA.EXE", new FileData
                                    {
                                        MD5 = "418db4f1d7355a0e23c9dad136abcfb0"
                                    }
                                },
                                {
                                    "DirectX/dxsetup.exe", new FileData
                                    {
                                        MD5 = "eb9040f0a112a35bd370d33ba350a0bb"
                                    }
                                },
                                {
                                    "DirectX/setupapi.dll", new FileData
                                    {
                                        MD5 = "5991908f571101d7dda613dbb679aa5b"
                                    }
                                },
                                {
                                    "dplayerx.dll", new FileData
                                    {
                                        MD5 = "6fcb281159944d36eeeb3c67c30775f3"
                                    }
                                },
                                {
                                    "drvmgt.dll", new FileData
                                    {
                                        MD5 = "1475518b2bdd98dda6e0753d7cd1acfc"
                                    }
                                },
                                {
                                    "IP/IP.cfg", new FileData
                                    {
                                        MD5 = "026af87c9a689123c60aa83972abbe1c"
                                    }
                                },
                                {
                                    "IP/IP.exe", new FileData
                                    {
                                        MD5 = "bb62aba0a7ac087ad88f80b3ed6e0356"
                                    }
                                },
                                {
                                    "launcher/AutoplayBack.bmp", new FileData
                                    {
                                        MD5 = "2759c0786de66d429ffe6037281f7325"
                                    }
                                },
                                {
                                    "launcher/AutoplayBackDe.BMP", new FileData
                                    {
                                        MD5 = "4f0b58ded07d4c6bceb7f8572f26dfb8"
                                    }
                                },
                                {
                                    "launcher/AutoplayBackFr.bmp", new FileData
                                    {
                                        MD5 = "9d8f35e32aa14b9b25d5cf1a95173163"
                                    }
                                },
                                {
                                    "launcher/AutoplayBackJP.bmp", new FileData
                                    {
                                        MD5 = "708161caa31e4730aaeb79fd725f7d52"
                                    }
                                },
                                {
                                    "launcher/AutoPlayBackSP.bmp", new FileData
                                    {
                                        MD5 = "f4e9c333e28a44a90e5e1d645a4d4a1b"
                                    }
                                },
                                {
                                    "MAXIS.INI", new FileData
                                    {
                                        MD5 = "ddf21028f8b961457249193e7b9b50b6"
                                    }
                                },
                                {
                                    "MSEULA_Eng.txt", new FileData
                                    {
                                        MD5 = "2a58bf518cc1684dc738292d11a1fc1e"
                                    }
                                },
                                {
                                    "MSEULA_Fre.txt", new FileData
                                    {
                                        MD5 = "58067092b828cf432969e74dfaea2f24"
                                    }
                                },
                                {
                                    "MSEULA_Ger.txt", new FileData
                                    {
                                        MD5 = "2222a1563b8fa88abb56a5c954e18444"
                                    }
                                },
                                {
                                    "MSEULA_Ita.txt", new FileData
                                    {
                                        MD5 = "0c4cea837aea74ae4f1d18074758aaf2"
                                    }
                                },
                                {
                                    "MSEULA_Spa.txt", new FileData
                                    {
                                        MD5 = "85df336988fde7d122112ff0716a1cec"
                                    }
                                },
                                {
                                    "Music/Modes/Build/build1.mp3", new FileData
                                    {
                                        MD5 = "09c1c3f6b77f710427d2ec2b16fbee1d"
                                    }
                                },
                                {
                                    "Music/Modes/Build/build2.mp3", new FileData
                                    {
                                        MD5 = "2d907526b9f106ffcea4d98e289f4609"
                                    }
                                },
                                {
                                    "Music/Modes/Build/build3.mp3", new FileData
                                    {
                                        MD5 = "11d899714199101f8dbf91685ceac313"
                                    }
                                },
                                {
                                    "Music/Modes/Build/build4.mp3", new FileData
                                    {
                                        MD5 = "864f6720fea08faa58806c6a3fe0cecb"
                                    }
                                },
                                {
                                    "Music/Modes/Build/build5.mp3", new FileData
                                    {
                                        MD5 = "7fd44da87859f8f5d49dca6270663870"
                                    }
                                },
                                {
                                    "Music/Modes/Build/build6.mp3", new FileData
                                    {
                                        MD5 = "2047db652f0d3ef9568eef7ab5bfa475"
                                    }
                                },
                                {
                                    "Music/Modes/Buy/buy1.mp3", new FileData
                                    {
                                        MD5 = "589856fa6676f3755d937911a29a38a3"
                                    }
                                },
                                {
                                    "Music/Modes/Buy/buy2.mp3", new FileData
                                    {
                                        MD5 = "26766ec6b91f4e35ea7eced638623153"
                                    }
                                },
                                {
                                    "Music/Modes/Buy/buy3.mp3", new FileData
                                    {
                                        MD5 = "4e2c74b9952aee3fc1c0575428124d80"
                                    }
                                },
                                {
                                    "Music/Modes/Buy/buy4.mp3", new FileData
                                    {
                                        MD5 = "0c1212d1f62092213426d86dca882f4b"
                                    }
                                },
                                {
                                    "Music/Modes/Load/loadloop.wav", new FileData
                                    {
                                        MD5 = "c5555fff4eaa25419b297aa69cfb2903"
                                    }
                                },
                                {
                                    "Music/Modes/Nhood/latin2.mp3", new FileData
                                    {
                                        MD5 = "da637bd1bd4c24ca6ce4adb29c9cfe5d"
                                    }
                                },
                                {
                                    "Music/Modes/Nhood/latin4.mp3", new FileData
                                    {
                                        MD5 = "b7e7422cc5d5fc2d316250c1640977b6"
                                    }
                                },
                                {
                                    "Music/Modes/Nhood/latin6.mp3", new FileData
                                    {
                                        MD5 = "d68554c3a9dcedde858b74bcbb2f597b"
                                    }
                                },
                                {
                                    "Music/Modes/Nhood/Latin7.mp3", new FileData
                                    {
                                        MD5 = "9d501af0cc419bc32028c25da72e2816"
                                    }
                                },
                                {
                                    "Music/Modes/NhoodUS/nhood1.mp3", new FileData
                                    {
                                        MD5 = "9eb7c93106e151123845af8375d5248c"
                                    }
                                },
                                {
                                    "Music/Modes/NhoodUS/nhood2.mp3", new FileData
                                    {
                                        MD5 = "4f73d200113051e5aa3c9b3a372e3a0d"
                                    }
                                },
                                {
                                    "Music/Modes/NhoodUS/nhood3.mp3", new FileData
                                    {
                                        MD5 = "831b6e21e782c5cc37e5aae6107a2150"
                                    }
                                },
                                {
                                    "Music/sims.wve", new FileData
                                    {
                                        MD5 = "77747e9a38b0e92a2694ebf3a3424dfe"
                                    }
                                },
                                {
                                    "Music/Stations/Classica/BbMaj.mp3", new FileData
                                    {
                                        MD5 = "4f837fff98a53a7e44b83a2a2d784a29"
                                    }
                                },
                                {
                                    "Music/Stations/Classica/Cmaj.mp3", new FileData
                                    {
                                        MD5 = "ccdf2d93c2f388f1ac7e1794ff1f17c9"
                                    }
                                },
                                {
                                    "Music/Stations/Classica/EbMaj.mp3", new FileData
                                    {
                                        MD5 = "bbd48fb9df9a6083c22c0394e46d9e2b"
                                    }
                                },
                                {
                                    "Music/Stations/Classica/Fmaj.mp3", new FileData
                                    {
                                        MD5 = "118374111369b0382ed2bf84546083d4"
                                    }
                                },
                                {
                                    "Music/Stations/Classica/Gmaj.mp3", new FileData
                                    {
                                        MD5 = "3ea30f0839b9b7bb4053b2db24e5db40"
                                    }
                                },
                                {
                                    "Music/Stations/Country/beaumont.mp3", new FileData
                                    {
                                        MD5 = "377cc1863c356b29e8a8addca73d4201"
                                    }
                                },
                                {
                                    "Music/Stations/Country/devilsdr.mp3", new FileData
                                    {
                                        MD5 = "93fad9a374fbf5e428292277dfbd940a"
                                    }
                                },
                                {
                                    "Music/Stations/Country/downroad.mp3", new FileData
                                    {
                                        MD5 = "c9a5b58ece608b6e9720d26c443ab81c"
                                    }
                                },
                                {
                                    "Music/Stations/Country/downyon.mp3", new FileData
                                    {
                                        MD5 = "149f3dd6a32351af9eb667a92a1ac4d7"
                                    }
                                },
                                {
                                    "Music/Stations/Country/sallygoo.mp3", new FileData
                                    {
                                        MD5 = "66aba807281a751611689531a9ba4741"
                                    }
                                },
                                {
                                    "Music/Stations/Country/splatter.mp3", new FileData
                                    {
                                        MD5 = "0d94e1172bd51af7f312af787a15578c"
                                    }
                                },
                                {
                                    "Music/Stations/Country/turkey.mp3", new FileData
                                    {
                                        MD5 = "7291acc8bad369b8a8c88856a63df0d6"
                                    }
                                },
                                {
                                    "Music/Stations/Latin/latin1.mp3", new FileData
                                    {
                                        MD5 = "0dd8a470d0337fc7d151839a4253d0fb"
                                    }
                                },
                                {
                                    "Music/Stations/Latin/latin2.mp3", new FileData
                                    {
                                        MD5 = "da637bd1bd4c24ca6ce4adb29c9cfe5d"
                                    }
                                },
                                {
                                    "Music/Stations/Latin/latin3.mp3", new FileData
                                    {
                                        MD5 = "6ba58882f2a9014ebc57eda8e740e741"
                                    }
                                },
                                {
                                    "Music/Stations/Latin/latin4.mp3", new FileData
                                    {
                                        MD5 = "b7e7422cc5d5fc2d316250c1640977b6"
                                    }
                                },
                                {
                                    "Music/Stations/Latin/latin5.mp3", new FileData
                                    {
                                        MD5 = "dad7383eacc552409eb11b3f7da3f453"
                                    }
                                },
                                {
                                    "Music/Stations/Latin/Latin7.mp3", new FileData
                                    {
                                        MD5 = "9d501af0cc419bc32028c25da72e2816"
                                    }
                                },
                                {
                                    "Music/Stations/Rock/rock1.mp3", new FileData
                                    {
                                        MD5 = "2932cf44027bf8c33ef25cbeb0323878"
                                    }
                                },
                                {
                                    "Music/Stations/Rock/rock2.mp3", new FileData
                                    {
                                        MD5 = "c3ecd2cba6a8b953eb06768ad0381347"
                                    }
                                },
                                {
                                    "Music/Stations/Rock/rock3.mp3", new FileData
                                    {
                                        MD5 = "876ebf5e17e16a10cf432e3b45c89f4f"
                                    }
                                },
                                {
                                    "Music/Stations/Rock/rock4.mp3", new FileData
                                    {
                                        MD5 = "b6493bfe4b300c4c72cc0936fc38310b"
                                    }
                                },
                                {
                                    "Music/Stations/Rock/rock5.mp3", new FileData
                                    {
                                        MD5 = "06802aa608d474d937d01270b33833d1"
                                    }
                                },
                                {
                                    "NPSPatch/_inst32i.ex_", new FileData
                                    {
                                        MD5 = "6229a86a1d291c311da49a7d69a49a1f"
                                    }
                                },
                                {
                                    "NPSPatch/_ISDel.exe", new FileData
                                    {
                                        MD5 = "51161bf79f25ff278912005078ad93d5"
                                    }
                                },
                                {
                                    "NPSPatch/_Setup.dll", new FileData
                                    {
                                        MD5 = "ecacc9ab09d7e8898799fe5c4ebbbdd2"
                                    }
                                },
                                {
                                    "NPSPatch/_sys1.cab", new FileData
                                    {
                                        MD5 = "8833055a72055a58deb8d06bd1c6d17b"
                                    }
                                },
                                {
                                    "NPSPatch/_sys1.hdr", new FileData
                                    {
                                        MD5 = "1b7dad3866a296e68a3cf45b359f3e86"
                                    }
                                },
                                {
                                    "NPSPatch/_user1.cab", new FileData
                                    {
                                        MD5 = "3b81476723cfaed8acce27cef590f313"
                                    }
                                },
                                {
                                    "NPSPatch/_user1.hdr", new FileData
                                    {
                                        MD5 = "c34afd46f8d2ca51ffb5f70054fd24a5"
                                    }
                                },
                                {
                                    "NPSPatch/DATA.TAG", new FileData
                                    {
                                        MD5 = "25d1a5abae3fcb433895ca06d524ba5c"
                                    }
                                },
                                {
                                    "NPSPatch/data1.cab", new FileData
                                    {
                                        MD5 = "0eef96d99264e3c3effb85f86471ea7b"
                                    }
                                },
                                {
                                    "NPSPatch/data1.hdr", new FileData
                                    {
                                        MD5 = "13f4874c34b67a3dfbdaff1ebb4da084"
                                    }
                                },
                                {
                                    "NPSPatch/lang.dat", new FileData
                                    {
                                        MD5 = "70627bd56fe92a5c97027cbbd88bacd0"
                                    }
                                },
                                {
                                    "NPSPatch/layout.bin", new FileData
                                    {
                                        MD5 = "d9f05148a0e6b609819ea967492a13a1"
                                    }
                                },
                                {
                                    "NPSPatch/os.dat", new FileData
                                    {
                                        MD5 = "478f65a0b922b6ba0a6ce99e1d15c336"
                                    }
                                },
                                {
                                    "NPSPatch/Setup.exe", new FileData
                                    {
                                        MD5 = "71e6dd8a9de4a9baf89fca951768059a"
                                    }
                                },
                                {
                                    "NPSPatch/SETUP.INI", new FileData
                                    {
                                        MD5 = "e81cb2124225ea84a7f50fb0ff57e9ea"
                                    }
                                },
                                {
                                    "NPSPatch/setup.ins", new FileData
                                    {
                                        MD5 = "09ac71c03c86b29fb57cdda116d1884d"
                                    }
                                },
                                {
                                    "NPSPatch/setup.lid", new FileData
                                    {
                                        MD5 = "1b79748e93a541cc1590505b6c72828a"
                                    }
                                },
                                {
                                    "NPSPatch/vssver.scc", new FileData
                                    {
                                        MD5 = "1653007bed51896c7b64ef8349eafb01"
                                    }
                                },
                                {
                                    "Readme/Läsmig.txt", new FileData
                                    {
                                        MD5 = "6db27c4e439af96c36b78388dd1ae533"
                                    }
                                },
                                {
                                    "Readme/Leeme.txt", new FileData
                                    {
                                        MD5 = "14d0ad5c5e1b0a97acd7dfcad2624b89"
                                    }
                                },
                                {
                                    "Readme/Leesmij.txt", new FileData
                                    {
                                        MD5 = "c2bb689dc72ac61663d7da0a2dec4357"
                                    }
                                },
                                {
                                    "Readme/Leggimi.txt", new FileData
                                    {
                                        MD5 = "f145a82b186c92b58d3382841ae2164c"
                                    }
                                },
                                {
                                    "Readme/Leia-me.txt", new FileData
                                    {
                                        MD5 = "d548799c5be094636668b71733cc7d10"
                                    }
                                },
                                {
                                    "Readme/Liesmich.txt", new FileData
                                    {
                                        MD5 = "d43029d81dce21721efa2e0db57dde5c"
                                    }
                                },
                                {
                                    "Readme/Lisezmoi.txt", new FileData
                                    {
                                        MD5 = "ff68d6dd6783f2cd5849c746e4905e28"
                                    }
                                },
                                {
                                    "Readme/Readme.txt", new FileData
                                    {
                                        MD5 = "f869755a208e98175828ec2052de2ced"
                                    }
                                },
                                {
                                    "Readme/ReadmeJ.txt", new FileData
                                    {
                                        MD5 = "7f3e22c51364bbf9868bbfeaecdb39e1"
                                    }
                                },
                                {
                                    "Readme/ReadmeKo.txt", new FileData
                                    {
                                        MD5 = "457df2f376895024311e1d17e622318c"
                                    }
                                },
                                {
                                    "Readme/ReadmeP.txt", new FileData
                                    {
                                        MD5 = "94b5f10a89e6dcee49060082bb66d30f"
                                    }
                                },
                                {
                                    "Readme/ReadmeSC.txt", new FileData
                                    {
                                        MD5 = "a465fe9cbaa0b6b6bfb3641bdd683f92"
                                    }
                                },
                                {
                                    "Readme/ReadmeTC.txt", new FileData
                                    {
                                        MD5 = "15fee71940c9afbae32acc57262f91ef"
                                    }
                                },
                                {
                                    "Readme/ReadmeTh.txt", new FileData
                                    {
                                        MD5 = "5e7f393e66c7579c57673be355648bb8"
                                    }
                                },
                                {
                                    "secdrv.sys", new FileData
                                    {
                                        MD5 = "d40e5b623d1a7e9bf09bdbf376d16432"
                                    }
                                },
                                {
                                    "Setup.exe", new FileData
                                    {
                                        MD5 = "1fc002f92a39988f25eaf94120f7eb7f"
                                    }
                                },
                                {
                                    "Setup/English/_INST32I.EX_", new FileData
                                    {
                                        MD5 = "8db6fa678163e440c9c0328a44a752f1"
                                    }
                                },
                                {
                                    "Setup/English/_ISDEL.EXE", new FileData
                                    {
                                        MD5 = "cd209af116614554405a336aad013c14"
                                    }
                                },
                                {
                                    "Setup/English/_s327.exe", new FileData
                                    {
                                        MD5 = "38369bacc2bf3c731ccf2c9ed7fac71c"
                                    }
                                },
                                {
                                    "Setup/English/_SETUP.DLL", new FileData
                                    {
                                        MD5 = "024856bad2071599202cff50fa9aae71"
                                    }
                                },
                                {
                                    "Setup/English/_sys1.cab", new FileData
                                    {
                                        MD5 = "a54937ee0981a2fd807ec8dd42df335a"
                                    }
                                },
                                {
                                    "Setup/English/_user1.cab", new FileData
                                    {
                                        MD5 = "567bbb9823e8497c8a9e7e4e2730eb43"
                                    }
                                },
                                {
                                    "Setup/English/DATA.TAG", new FileData
                                    {
                                        MD5 = "955c2eb926ebb9183d42844c8c4f941b"
                                    }
                                },
                                {
                                    "Setup/English/lang.dat", new FileData
                                    {
                                        MD5 = "d0754bcefd6ee3ebc144beaf9e193332"
                                    }
                                },
                                {
                                    "Setup/English/layout.bin", new FileData
                                    {
                                        MD5 = "106b254fbab5af928901774f1c8b0630"
                                    }
                                },
                                {
                                    "Setup/English/os.dat", new FileData
                                    {
                                        MD5 = "af1d8d9435cb10fe2f4b4215eaf6bec4"
                                    }
                                },
                                {
                                    "Setup/English/setup.bmp", new FileData
                                    {
                                        MD5 = "c2ee7cb47471e608f3ac5c7efd1384c0"
                                    }
                                },
                                {
                                    "Setup/English/SETUP.INI", new FileData
                                    {
                                        MD5 = "b95d519c7ffde9aa3799a7fd65fa6e8a"
                                    }
                                },
                                {
                                    "Setup/English/setup.ins", new FileData
                                    {
                                        MD5 = "a8cc540f91f60f53cb7ddd9a39c0441a"
                                    }
                                },
                                {
                                    "Setup/English/setup.lid", new FileData
                                    {
                                        MD5 = "1b79748e93a541cc1590505b6c72828a"
                                    }
                                },
                                {
                                    "Setup/Thai/_INST32I.EX_", new FileData
                                    {
                                        MD5 = "8db6fa678163e440c9c0328a44a752f1"
                                    }
                                },
                                {
                                    "Setup/Thai/_ISDEL.EXE", new FileData
                                    {
                                        MD5 = "cd209af116614554405a336aad013c14"
                                    }
                                },
                                {
                                    "Setup/Thai/_s327.exe", new FileData
                                    {
                                        MD5 = "38369bacc2bf3c731ccf2c9ed7fac71c"
                                    }
                                },
                                {
                                    "Setup/Thai/_SETUP.DLL", new FileData
                                    {
                                        MD5 = "0a858fbf151806d66f4cffb9c840d67c"
                                    }
                                },
                                {
                                    "Setup/Thai/_sys1.cab", new FileData
                                    {
                                        MD5 = "19eb67c87e87541b52033293e4d6be61"
                                    }
                                },
                                {
                                    "Setup/Thai/_user1.cab", new FileData
                                    {
                                        MD5 = "0ac10c3f909a59608957492dd992e6c1"
                                    }
                                },
                                {
                                    "Setup/Thai/DATA.TAG", new FileData
                                    {
                                        MD5 = "955c2eb926ebb9183d42844c8c4f941b"
                                    }
                                },
                                {
                                    "Setup/Thai/lang.dat", new FileData
                                    {
                                        MD5 = "d0754bcefd6ee3ebc144beaf9e193332"
                                    }
                                },
                                {
                                    "Setup/Thai/layout.bin", new FileData
                                    {
                                        MD5 = "106b254fbab5af928901774f1c8b0630"
                                    }
                                },
                                {
                                    "Setup/Thai/os.dat", new FileData
                                    {
                                        MD5 = "af1d8d9435cb10fe2f4b4215eaf6bec4"
                                    }
                                },
                                {
                                    "Setup/Thai/setup.bmp", new FileData
                                    {
                                        MD5 = "c2ee7cb47471e608f3ac5c7efd1384c0"
                                    }
                                },
                                {
                                    "Setup/Thai/SETUP.INI", new FileData
                                    {
                                        MD5 = "b95d519c7ffde9aa3799a7fd65fa6e8a"
                                    }
                                },
                                {
                                    "Setup/Thai/setup.ins", new FileData
                                    {
                                        MD5 = "a8cc540f91f60f53cb7ddd9a39c0441a"
                                    }
                                },
                                {
                                    "Setup/Thai/setup.lid", new FileData
                                    {
                                        MD5 = "c0d2ef909b439cdaba6787936ab77b7c"
                                    }
                                },
                                {
                                    "SIMS.ICD", new FileData
                                    {
                                        MD5 = "c012747eb077b6bc988392dccad18d52"
                                    }
                                },
                                {
                                    "simscd.ico", new FileData
                                    {
                                        MD5 = "a6c14e7ec732d8a1ab76b8f0059efd98"
                                    }
                                },
                                {
                                    "Support/SKU1/Spanish/eahelp.hlp", new FileData
                                    {
                                        MD5 = "5eaa6884e04c32d281f8d731a74e0437"
                                    }
                                },
                                {
                                    "Support/SKU1/Spanish/IP.cfg", new FileData
                                    {
                                        MD5 = "026af87c9a689123c60aa83972abbe1c"
                                    }
                                },
                                {
                                    "Support/SKU1/Spanish/IP.exe", new FileData
                                    {
                                        MD5 = "bb62aba0a7ac087ad88f80b3ed6e0356"
                                    }
                                },
                                {
                                    "Support/SKU1/Spanish/readme.txt", new FileData
                                    {
                                        MD5 = "14d0ad5c5e1b0a97acd7dfcad2624b89"
                                    }
                                },
                                {
                                    "Support/SKU1/USEnglish/eahelp.hlp", new FileData
                                    {
                                        MD5 = "5eaa6884e04c32d281f8d731a74e0437"
                                    }
                                },
                                {
                                    "Support/SKU1/USEnglish/IP.cfg", new FileData
                                    {
                                        MD5 = "026af87c9a689123c60aa83972abbe1c"
                                    }
                                },
                                {
                                    "Support/SKU1/USEnglish/IP.exe", new FileData
                                    {
                                        MD5 = "bb62aba0a7ac087ad88f80b3ed6e0356"
                                    }
                                },
                                {
                                    "Support/SKU1/USEnglish/readme.txt", new FileData
                                    {
                                        MD5 = "f869755a208e98175828ec2052de2ced"
                                    }
                                },
                                {
                                    "Support/SKU2/Dutch/eaukhelp.hlp", new FileData
                                    {
                                        MD5 = "d5a50ba9c24f43d30f7349aeb38ec3eb"
                                    }
                                },
                                {
                                    "Support/SKU2/Dutch/finished version/IP.exe", new FileData
                                    {
                                        MD5 = "bb62aba0a7ac087ad88f80b3ed6e0356"
                                    }
                                },
                                {
                                    "Support/SKU2/Dutch/finished version/readme.txt", new FileData
                                    {
                                        MD5 = "c2bb689dc72ac61663d7da0a2dec4357"
                                    }
                                },
                                {
                                    "Support/SKU2/Dutch/IP.cfg", new FileData
                                    {
                                        MD5 = "026af87c9a689123c60aa83972abbe1c"
                                    }
                                },
                                {
                                    "Support/SKU2/French/eaukhelp.hlp", new FileData
                                    {
                                        MD5 = "d5a50ba9c24f43d30f7349aeb38ec3eb"
                                    }
                                },
                                {
                                    "Support/SKU2/French/finished version/IP.exe", new FileData
                                    {
                                        MD5 = "bb62aba0a7ac087ad88f80b3ed6e0356"
                                    }
                                },
                                {
                                    "Support/SKU2/French/finished version/readme.txt", new FileData
                                    {
                                        MD5 = "ff68d6dd6783f2cd5849c746e4905e28"
                                    }
                                },
                                {
                                    "Support/SKU2/French/IP.cfg", new FileData
                                    {
                                        MD5 = "026af87c9a689123c60aa83972abbe1c"
                                    }
                                },
                                {
                                    "Support/SKU2/German/eaukhelp.hlp", new FileData
                                    {
                                        MD5 = "d5a50ba9c24f43d30f7349aeb38ec3eb"
                                    }
                                },
                                {
                                    "Support/SKU2/German/finished version/IP.exe", new FileData
                                    {
                                        MD5 = "bb62aba0a7ac087ad88f80b3ed6e0356"
                                    }
                                },
                                {
                                    "Support/SKU2/German/finished version/readme.txt", new FileData
                                    {
                                        MD5 = "d43029d81dce21721efa2e0db57dde5c"
                                    }
                                },
                                {
                                    "Support/SKU2/German/IP.cfg", new FileData
                                    {
                                        MD5 = "026af87c9a689123c60aa83972abbe1c"
                                    }
                                },
                                {
                                    "Support/SKU2/Italian/eaukhelp.hlp", new FileData
                                    {
                                        MD5 = "d5a50ba9c24f43d30f7349aeb38ec3eb"
                                    }
                                },
                                {
                                    "Support/SKU2/Italian/finished version/IP.exe", new FileData
                                    {
                                        MD5 = "bb62aba0a7ac087ad88f80b3ed6e0356"
                                    }
                                },
                                {
                                    "Support/SKU2/Italian/finished version/readme.txt", new FileData
                                    {
                                        MD5 = "f145a82b186c92b58d3382841ae2164c"
                                    }
                                },
                                {
                                    "Support/SKU2/Italian/IP.cfg", new FileData
                                    {
                                        MD5 = "026af87c9a689123c60aa83972abbe1c"
                                    }
                                },
                                {
                                    "Support/SKU2/Swedish/eaukhelp.hlp", new FileData
                                    {
                                        MD5 = "d5a50ba9c24f43d30f7349aeb38ec3eb"
                                    }
                                },
                                {
                                    "Support/SKU2/Swedish/finished version/IP.exe", new FileData
                                    {
                                        MD5 = "bb62aba0a7ac087ad88f80b3ed6e0356"
                                    }
                                },
                                {
                                    "Support/SKU2/Swedish/finished version/readme.txt", new FileData
                                    {
                                        MD5 = "6db27c4e439af96c36b78388dd1ae533"
                                    }
                                },
                                {
                                    "Support/SKU2/Swedish/IP.cfg", new FileData
                                    {
                                        MD5 = "026af87c9a689123c60aa83972abbe1c"
                                    }
                                },
                                {
                                    "Support/SKU2/UKEnglish/eaukhelp.hlp", new FileData
                                    {
                                        MD5 = "d5a50ba9c24f43d30f7349aeb38ec3eb"
                                    }
                                },
                                {
                                    "Support/SKU2/UKEnglish/finished version/readme.txt", new FileData
                                    {
                                        MD5 = "f869755a208e98175828ec2052de2ced"
                                    }
                                },
                                {
                                    "Support/SKU3/Portuguese/eaukhelp.hlp", new FileData
                                    {
                                        MD5 = "d5a50ba9c24f43d30f7349aeb38ec3eb"
                                    }
                                },
                                {
                                    "Support/SKU3/Portuguese/finished version/IP.exe", new FileData
                                    {
                                        MD5 = "bb62aba0a7ac087ad88f80b3ed6e0356"
                                    }
                                },
                                {
                                    "Support/SKU3/Portuguese/finished version/readme.txt", new FileData
                                    {
                                        MD5 = "d548799c5be094636668b71733cc7d10"
                                    }
                                },
                                {
                                    "Support/SKU3/Portuguese/IP.cfg", new FileData
                                    {
                                        MD5 = "026af87c9a689123c60aa83972abbe1c"
                                    }
                                },
                                {
                                    "Support/SKU3/Spanish/eaukhelp.hlp", new FileData
                                    {
                                        MD5 = "d5a50ba9c24f43d30f7349aeb38ec3eb"
                                    }
                                },
                                {
                                    "Support/SKU3/Spanish/finished version/IP.exe", new FileData
                                    {
                                        MD5 = "bb62aba0a7ac087ad88f80b3ed6e0356"
                                    }
                                },
                                {
                                    "Support/SKU3/Spanish/finished version/readme.txt", new FileData
                                    {
                                        MD5 = "14d0ad5c5e1b0a97acd7dfcad2624b89"
                                    }
                                },
                                {
                                    "Support/SKU3/Spanish/IP.cfg", new FileData
                                    {
                                        MD5 = "026af87c9a689123c60aa83972abbe1c"
                                    }
                                },
                                {
                                    "Support/SKU3/USEnglish/eaukhelp.hlp", new FileData
                                    {
                                        MD5 = "d5a50ba9c24f43d30f7349aeb38ec3eb"
                                    }
                                },
                                {
                                    "Support/SKU3/USEnglish/finished version/IP.exe", new FileData
                                    {
                                        MD5 = "bb62aba0a7ac087ad88f80b3ed6e0356"
                                    }
                                },
                                {
                                    "Support/SKU3/USEnglish/finished version/readme.txt", new FileData
                                    {
                                        MD5 = "f869755a208e98175828ec2052de2ced"
                                    }
                                },
                                {
                                    "Support/SKU3/USEnglish/IP.cfg", new FileData
                                    {
                                        MD5 = "026af87c9a689123c60aa83972abbe1c"
                                    }
                                },
                                {
                                    "Web Page Template README.html", new FileData
                                    {
                                        MD5 = "6fa6cd037855c2dc22ffca89c85ac3f7"
                                    }
                                },
                                {
                                    "Web Page Template ReadmeTh.html", new FileData
                                    {
                                        MD5 = "ddf48657ff2f867b9aeab463761dc0b9"
                                    }
                                },
                                {
                                    "WMP/mpfull.exe", new FileData
                                    {
                                        MD5 = "22c33ef3b99995a8638f6674491d5d43"
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