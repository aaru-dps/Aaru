using System.Collections.Generic;
using System.IO;

// ReSharper disable StringLiteralTypo

namespace Aaru.Tests.Issues._590
{
    /* https://github.com/aaru-dps/Aaru/issues/590
     *
     * SilasLaspada commented on June 1, 2021
     *
     * Other images seemingly affected by the same bug
     */

    public class asus_driver_gpu_tweak_v1231_2014 : FsExtractHashIssueTest
    {
        protected override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Fixed", "issue590",
                                                             "asus-driver-gpu-tweak-v1231-2014");
        protected override string                     TestFile         => "V1231.aaruf";
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
                            VolumeName = "V1231",
                            Directories = new List<string>
                            {
                                "Driver",
                                "Driver/Win7_Win8.1",
                                "Driver/Win7_Win8.1/Bin",
                                "Driver/Win7_Win8.1/Bin64",
                                "Driver/Win7_Win8.1/Config",
                                "Driver/Win7_Win8.1/Images",
                                "Driver/Win7_Win8.1/Packages",
                                "Driver/Win7_Win8.1/Packages/Apps",
                                "Driver/Win7_Win8.1/Packages/Apps/AVT",
                                "Driver/Win7_Win8.1/Packages/Apps/AVT64",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Branding",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/cs",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/da",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/de",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/el",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/en-us",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/es",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/fi",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/fr",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/hu",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/it",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/ja",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/ko",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/nl",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/no",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/pl",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/pt-BR",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/ru",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/sv",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/th",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/tr",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/zh-CHS",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/zh-CHT",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Localisation",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Localisation/All",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/MOM-InstallProxy-Net4",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Profiles",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Profiles/Desktop",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Utility-Net4",
                                "Driver/Win7_Win8.1/Packages/Apps/CCC2/Utility64-Net4",
                                "Driver/Win7_Win8.1/Packages/Apps/CIM",
                                "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32",
                                "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64",
                                "Driver/Win7_Win8.1/Packages/Apps/DotNet45",
                                "Driver/Win7_Win8.1/Packages/Apps/DotNet45/dotnet45",
                                "Driver/Win7_Win8.1/Packages/Apps/Raptr",
                                "Driver/Win7_Win8.1/Packages/Apps/Raptr/RaptrInstaller",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1028",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1031",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1033",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1036",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1040",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1041",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1042",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1049",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/2052",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/3082",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1028",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1031",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1033",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1036",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1040",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1041",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1042",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1049",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/2052",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/3082",
                                "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics",
                                "Driver/Win7_Win8.1/Packages/Apps/VC12RTx64",
                                "Driver/Win7_Win8.1/Packages/Apps/VC12RTx64/vcredist_x64",
                                "Driver/Win7_Win8.1/Packages/Apps/VC12RTx86",
                                "Driver/Win7_Win8.1/Packages/Apps/VC12RTx86/vcredist_x86",
                                "Driver/Win7_Win8.1/Packages/Drivers",
                                "Driver/Win7_Win8.1/Packages/Drivers/Display",
                                "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF",
                                "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487",
                                "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF",
                                "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487",
                                "Driver/Win7_Win8.1/Packages/Drivers/WDM",
                                "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI",
                                "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W7",
                                "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W764A",
                                "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB",
                                "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB64A",
                                "Driver/Win7_Win8.1/Packages/Drivers/amdkmafd",
                                "Driver/Win7_Win8.1/Packages/Drivers/amdkmafd/WB",
                                "Driver/Win7_Win8.1/Packages/Drivers/amdkmafd/WB64A",
                                "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd",
                                "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/W7",
                                "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/W764A",
                                "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/WB",
                                "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/WB64A",
                                "Manual",
                                "Utility",
                                "Utility/APRP",
                                "Utility/GPUTweak",
                                "Utility/GoogleChrome",
                                "Utility/GoogleToolbar",
                                "Utility/Streaming"
                            },
                            Files = new Dictionary<string, FileData>
                            {
                                {
                                    "ASUSLogo.ico", new FileData
                                    {
                                        MD5 = "231cf4b7d5ff29073aeeae0fd7a9db02"
                                    }
                                },
                                {
                                    "AutoRun.inf", new FileData
                                    {
                                        MD5 = "70391812bb5864078f119f7d172a007c"
                                    }
                                },
                                {
                                    "CheckID.exe", new FileData
                                    {
                                        MD5 = "cc78085252f65de3610cbf0988b0f1ae"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/atidcmxx.sys", new FileData
                                    {
                                        MD5 = "2e924c8d792d7a8c5b86605f5dcef0cc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/ATILog.dll", new FileData
                                    {
                                        MD5 = "0eb5dbb405571de9556d9fe593832964"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/ATIManifestDLMExt.dll", new FileData
                                    {
                                        MD5 = "757d6c5671c02132bae2b26228682c17"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/ATISetup.exe", new FileData
                                    {
                                        MD5 = "2e3cdbb9242a0ff2eb89e0bd19bd1754"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/CompressionDLMExt.dll", new FileData
                                    {
                                        MD5 = "ccbcdab438ec444dd74255bd648af6a7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/ControlCenterActions.dll", new FileData
                                    {
                                        MD5 = "3587d45ca0aa4394133d8e18dcaa08f1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/CRCVerDLMExt.dll", new FileData
                                    {
                                        MD5 = "06d94595df59a385d1e571beae1f3491"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/DetectionManager.dll", new FileData
                                    {
                                        MD5 = "75139183455144dcfababe421755b633"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/difxapi.dll", new FileData
                                    {
                                        MD5 = "1bd976dd77b31fe0f25708ad5c1351ae"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/DLMCom.dll", new FileData
                                    {
                                        MD5 = "75c762812a481fd134c02ec42c950b6b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/EncryptionDLMExt.dll", new FileData
                                    {
                                        MD5 = "5442766444aa3b744dd3777fa21572e2"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/InstallManager.dll", new FileData
                                    {
                                        MD5 = "ba9fa5ca180a4f4545ff0be72582cbe9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/InstallManagerApp.exe", new FileData
                                    {
                                        MD5 = "512ba2b2b8b8b8ba2673841242e5716b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/LanguageMgr.dll", new FileData
                                    {
                                        MD5 = "5b305b3706e6a449134bf7f3b9d221c1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/mfc110u.dll", new FileData
                                    {
                                        MD5 = "2d79817dd5aea2a2a4449e72f20491e0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/msvcp110.dll", new FileData
                                    {
                                        MD5 = "3e29914113ec4b968ba5eb1f6d194a0a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/msvcr110.dll", new FileData
                                    {
                                        MD5 = "4ba25d2cbe1587a841dcfb8c8c4a6ea6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/PackageManager.dll", new FileData
                                    {
                                        MD5 = "7fdd3f2fbd0781953f4383ad22133c28"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/Setup.exe", new FileData
                                    {
                                        MD5 = "bb0b25438176280fd446d43ea89e0c94"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/xerces-c_2_6.dll", new FileData
                                    {
                                        MD5 = "acb9594096be5c4b6f64d38a17d14508"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin/zlibwapi.dll", new FileData
                                    {
                                        MD5 = "4efaa53c545f4ffb1ee0ed1709c15ea7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/atdcm64a.sys", new FileData
                                    {
                                        MD5 = "4d438a8954c66dd3e66162dbe40961a7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/ATILog.dll", new FileData
                                    {
                                        MD5 = "9b6c900350a3d9b446e3c6dab5104d91"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/ATIManifestDLMExt.dll", new FileData
                                    {
                                        MD5 = "b9e309e5e29e29dfeadbd6d484d28026"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/ATISetup.exe", new FileData
                                    {
                                        MD5 = "6a85a5b422302a7edbc706251a6adecd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/CompressionDLMExt.dll", new FileData
                                    {
                                        MD5 = "baae2ce96adbb8f507bbd5309d1c280e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/ControlCenterActions.dll", new FileData
                                    {
                                        MD5 = "e81e2b80c480ba75f59c7ee5a4b4426e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/CRCVerDLMExt.dll", new FileData
                                    {
                                        MD5 = "bd16cc887d84fcbb383507cb47d21128"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/DetectionManager.dll", new FileData
                                    {
                                        MD5 = "bb2551e9fc25a33e9e663c5ad21aff61"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/difxapi.dll", new FileData
                                    {
                                        MD5 = "f5558c67a3adb662d43d40a1cbde4160"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/DLMCom.dll", new FileData
                                    {
                                        MD5 = "9402f322b283452a46ae4fc6ddfff0e1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/EncryptionDLMExt.dll", new FileData
                                    {
                                        MD5 = "648b334b837890cc3941a4e4c44585a8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/InstallManager.dll", new FileData
                                    {
                                        MD5 = "aff7bbc1b43efb9787e6a415904c8d01"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/InstallManagerApp.exe", new FileData
                                    {
                                        MD5 = "f73392ee14762bfc4ddb2bb97a6e0466"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/LanguageMgr.dll", new FileData
                                    {
                                        MD5 = "609cadbac7dbeb492af9d4db6881f67f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/mfc110u.dll", new FileData
                                    {
                                        MD5 = "3d8b311a16f40c08b2487cfaa2fcd621"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/msvcp110.dll", new FileData
                                    {
                                        MD5 = "7caa1b97a3311eb5a695e3c9028616e7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/msvcr110.dll", new FileData
                                    {
                                        MD5 = "7c3b449f661d99a9b1033a14033d2987"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/PackageManager.dll", new FileData
                                    {
                                        MD5 = "38328d3b0f8661abfae60b90d8b733fc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/Setup.exe", new FileData
                                    {
                                        MD5 = "a192dc3797288ff17d8698bbd8cc50c2"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/xerces-c_2_6.dll", new FileData
                                    {
                                        MD5 = "b12d201ff4ac15a134d82923c0b9b302"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Bin64/zlibwapi.dll", new FileData
                                    {
                                        MD5 = "dd91e4c7d445c31682ebdd22e732d93d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/atiicdxx.msi", new FileData
                                    {
                                        MD5 = "a2dcffc49ffca063a4f26084cedcc8e6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/chipset.MSI", new FileData
                                    {
                                        MD5 = "b5a239e0f1a6f776595cb4e1dd55c0bc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/DLMServer.cfg", new FileData
                                    {
                                        MD5 = "2af436ecb9696189482700c67ef42812"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaCHS.txt", new FileData
                                    {
                                        MD5 = "87035f60af2aed5828c20a7e1d400f59"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaCHT.txt", new FileData
                                    {
                                        MD5 = "74070f71d10237c108378d0ad2e2253b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaCSY.txt", new FileData
                                    {
                                        MD5 = "98ae6455cf7f1070ba0371ffa4011a4d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaDAN.txt", new FileData
                                    {
                                        MD5 = "f612eb54e9a49c62835848bfb05d9e30"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaDEU.txt", new FileData
                                    {
                                        MD5 = "5ff57120806182975829f6b46bd93ad5"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaENU.txt", new FileData
                                    {
                                        MD5 = "8d79cabd842c01445a9768e3e8f66b78"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaESP.txt", new FileData
                                    {
                                        MD5 = "ec789d8f655ceabd697188afa10d25bd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaFIN.txt", new FileData
                                    {
                                        MD5 = "6d3507316d3f330d3a2f09ac226ef650"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaFRA.txt", new FileData
                                    {
                                        MD5 = "619093b9fa95d356558a04847a6d1e10"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaGRK.txt", new FileData
                                    {
                                        MD5 = "0e2d6aa08f6fd199cf48e8de84c8b8e9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaHNG.txt", new FileData
                                    {
                                        MD5 = "e0543b378f7cc5674b1b01542a23aa37"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaITA.txt", new FileData
                                    {
                                        MD5 = "65a699f6700b8387b6ffc278d1b52f49"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaJPN.txt", new FileData
                                    {
                                        MD5 = "9db16cf3dfcf65fece81816a03dfbe7e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaKOR.txt", new FileData
                                    {
                                        MD5 = "39d05fc13861d7195c5a705eda30ec94"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaNLD.txt", new FileData
                                    {
                                        MD5 = "68e067e3c6fa5d9c6380db792e5fa6c7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaNOR.txt", new FileData
                                    {
                                        MD5 = "9c454b25ef4bb052f02c7e0d9510f540"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaPLK.txt", new FileData
                                    {
                                        MD5 = "b2532f21de60049fd465ba86b8cf3a64"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaPTB.txt", new FileData
                                    {
                                        MD5 = "646c0b0d4d988f4ceee91f678f5e0e8b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaRSA.txt", new FileData
                                    {
                                        MD5 = "e137d51134b665d803a6e8cf02b9d318"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaSVE.txt", new FileData
                                    {
                                        MD5 = "5653ce9de6bbfd96c82b40d3d505540e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaTHA.txt", new FileData
                                    {
                                        MD5 = "b9bb2abef81db05b39e663ad6f729a7e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/eulaTRK.txt", new FileData
                                    {
                                        MD5 = "4b03e3cbe8222d4f8d39464135cd2066"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/InstallManager.cfg", new FileData
                                    {
                                        MD5 = "8fa671e9a477f7da8854d01b822eb9ba"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/Language.Dat", new FileData
                                    {
                                        MD5 = "0f6271e5e4d9bb8d38553511c8eabe3c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseCHS.txt", new FileData
                                    {
                                        MD5 = "eeeba9691ad59c7ed1443120b1b7ed10"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseCHT.txt", new FileData
                                    {
                                        MD5 = "f8b275537eff086a11060b564c14836f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseCSY.txt", new FileData
                                    {
                                        MD5 = "00f3846f767f439a734aa34b1cb4e96e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseDAN.txt", new FileData
                                    {
                                        MD5 = "acb5b837e88254443837343fa8746216"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseDEU.txt", new FileData
                                    {
                                        MD5 = "55b092654a243d2428e0e06a08e70bba"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseENU.txt", new FileData
                                    {
                                        MD5 = "bd6c1800be95935a9519b9293a673dd3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseESP.txt", new FileData
                                    {
                                        MD5 = "d0d3ba1563a78afd544e2070cda13f28"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseFIN.txt", new FileData
                                    {
                                        MD5 = "91f0a875cbb9d40d9b48f1822f9eaacf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseFRA.txt", new FileData
                                    {
                                        MD5 = "00789e7a2e8762fa30e6e068ea1298f1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseGRK.txt", new FileData
                                    {
                                        MD5 = "e8beceef97abff98318a113d3f39bc84"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseHNG.txt", new FileData
                                    {
                                        MD5 = "a5e90c2adb2520ec5fedbe8fe664d936"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseITA.txt", new FileData
                                    {
                                        MD5 = "f9bed2eba77ad51ba59fa8067dc46b94"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseJPN.txt", new FileData
                                    {
                                        MD5 = "9469706d3f00acb462b270ed7cb64d25"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseKOR.txt", new FileData
                                    {
                                        MD5 = "0e1fc4b658d54c5422cbf44056ad82bd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseNLD.txt", new FileData
                                    {
                                        MD5 = "1547bc63ddf7133e4ca2c09735a692c8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseNOR.txt", new FileData
                                    {
                                        MD5 = "c28695fadead97194cf062dee078a307"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licensePLK.txt", new FileData
                                    {
                                        MD5 = "105a3dbe0f9eb61045686e73fd1aef96"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licensePTB.txt", new FileData
                                    {
                                        MD5 = "e26caf0ed737e81a231fb92b0f54dff9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseSVE.txt", new FileData
                                    {
                                        MD5 = "ed5e955f7af04a4e05b3bf09881a9230"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseTHA.txt", new FileData
                                    {
                                        MD5 = "a949e2503b85ff72850eea5c563b803f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/licenseTRK.txt", new FileData
                                    {
                                        MD5 = "2a2c5de62f436fda6641078314c364c4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MMTableRev0.MSI", new FileData
                                    {
                                        MD5 = "5386d7047c640067193223866bfd5b03"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MMTableRev1.MSI", new FileData
                                    {
                                        MD5 = "a574150de1ed64fdc2dd9cb522fa403e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MMTableRev2.MSI", new FileData
                                    {
                                        MD5 = "866d8a36609efddd5bcfabed75cf836b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/Monet.ini", new FileData
                                    {
                                        MD5 = "19a00552536aaa2d32273551cfd93df4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetCHS.xml", new FileData
                                    {
                                        MD5 = "9db25ae423e54286b122bb86f53136fc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetCHT.xml", new FileData
                                    {
                                        MD5 = "949b9b12272b5b6e5659c8dccd7e5443"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetCSY.xml", new FileData
                                    {
                                        MD5 = "d21b902607400e90c27a992f2d1dbc07"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetDAN.xml", new FileData
                                    {
                                        MD5 = "526fbcf2a308a3a4d10cf1f84b67dcb3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetDEU.xml", new FileData
                                    {
                                        MD5 = "a9d8f470dcc115b6ad6e4e130e95da12"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetENU.xml", new FileData
                                    {
                                        MD5 = "b2c5d820c60fc005ac5bf0f7d1ea120b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetESP.xml", new FileData
                                    {
                                        MD5 = "fc15617bd6b0dd3f097fad3f3082e84c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetFIN.xml", new FileData
                                    {
                                        MD5 = "8e4a02b7803bd9de6991ebc2253aa00a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetFRA.xml", new FileData
                                    {
                                        MD5 = "df3228ce20e431d6e1a479b1c0332ca6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetGRK.xml", new FileData
                                    {
                                        MD5 = "4be63a7e3e7ad33a30782fa061071d69"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetHNG.xml", new FileData
                                    {
                                        MD5 = "fe6986447764c385b2925d8798b86a85"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetITA.xml", new FileData
                                    {
                                        MD5 = "a532efb03c79311f036afe8e4b3b1f29"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetJPN.xml", new FileData
                                    {
                                        MD5 = "3fc2c1abe51e08ebce07a7b6f95cf92f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetKOR.xml", new FileData
                                    {
                                        MD5 = "51b6cb27a51528be4d9f0340ad8a9590"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetNLD.xml", new FileData
                                    {
                                        MD5 = "6052f009ebea8ccdef16e3e7f9856c72"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetNOR.xml", new FileData
                                    {
                                        MD5 = "42d59e320c1cd2f556e02685cd957b9d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetPLK.xml", new FileData
                                    {
                                        MD5 = "fe9ef55332aa9a7f78a0c32cff48ba9f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetPTB.xml", new FileData
                                    {
                                        MD5 = "b2260ce79a8466484fde286cbc4af0ec"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetRSA.xml", new FileData
                                    {
                                        MD5 = "78273ee98001356832df882f2c06342d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetSVE.xml", new FileData
                                    {
                                        MD5 = "a2905186a3bb2a1cd51c2bbcead7e337"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetTHA.xml", new FileData
                                    {
                                        MD5 = "38bf1d8e953a90ae7c5af050d5ff5f1a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/MonetTRK.xml", new FileData
                                    {
                                        MD5 = "aeb2cc90db17f58aef01d8201f4cd002"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/OEM.Dat", new FileData
                                    {
                                        MD5 = "b48c02ac7dfbdd896d482d0a484e6d19"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/OS.Dat", new FileData
                                    {
                                        MD5 = "4106267606d1c8d3d753fadc702c6e11"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/OSMajorMinor.Dat", new FileData
                                    {
                                        MD5 = "b11720b0a36e3af57c078af4cce2f5c7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/OSServicePacks.Dat", new FileData
                                    {
                                        MD5 = "ce8d2181073b464d12f2f597feaffd66"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/PackageSubType.Dat", new FileData
                                    {
                                        MD5 = "a06e885b0a59f9d133560d5d62f941eb"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/PackageType.Dat", new FileData
                                    {
                                        MD5 = "d4300930295db990807468e92f09fba1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/Profile1.cfg", new FileData
                                    {
                                        MD5 = "8e088d0219364a052ef982c2ac299e49"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/Security.Dat", new FileData
                                    {
                                        MD5 = "d0d6d183050ccdd63c5a9a02ed27ba03"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/Splash.bmp", new FileData
                                    {
                                        MD5 = "1273007f6be02d48ea37ee42f04b2bcf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/tvtablerev1.MSI", new FileData
                                    {
                                        MD5 = "2fcf228dbe24f18de16044478026a6dd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Config/TVW_USB_ID.MSI", new FileData
                                    {
                                        MD5 = "4b2b529c0c8658743e59777d7fb359e6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Images/a.jpg", new FileData
                                    {
                                        MD5 = "4e34d374f5f5d63ce46e1074267c033a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Images/b.jpg", new FileData
                                    {
                                        MD5 = "80159b0a6f207ef57eeb6b35b1addb30"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Images/c.jpg", new FileData
                                    {
                                        MD5 = "785ea6df6d225288b66d2c52df197ad8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Images/d.jpg", new FileData
                                    {
                                        MD5 = "760bc707d70843e285740088bb789ffe"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Images/e.jpg", new FileData
                                    {
                                        MD5 = "02641e37de7a8ff27e0dc3bca0d42a55"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Images/f.jpg", new FileData
                                    {
                                        MD5 = "785ea6df6d225288b66d2c52df197ad8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/mfc110u.dll", new FileData
                                    {
                                        MD5 = "2d79817dd5aea2a2a4449e72f20491e0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/msvcp110.dll", new FileData
                                    {
                                        MD5 = "3e29914113ec4b968ba5eb1f6d194a0a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/msvcr110.dll", new FileData
                                    {
                                        MD5 = "4ba25d2cbe1587a841dcfb8c8c4a6ea6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/AVT/AVT.msi", new FileData
                                    {
                                        MD5 = "ad129571d350a33f5eaf311438e3e1b2"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/AVT64/AVT64.msi", new FileData
                                    {
                                        MD5 = "4d9c79353712270a6a63c8170f8c5cb2"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Branding/Branding.msi", new FileData
                                    {
                                        MD5 = "dfea28f61a5eac1bd092ec0a80726cfd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1028.mst", new FileData
                                    {
                                        MD5 = "cfe9df144c41bd387b3d4c908d881281"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1029.mst", new FileData
                                    {
                                        MD5 = "7184ddcaa3e06810a7d9061eb05b41d4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1030.mst", new FileData
                                    {
                                        MD5 = "9533230cb635c76f4ca877d00952ea7e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1031.mst", new FileData
                                    {
                                        MD5 = "c99a6ee92ff35a2a95a9052739fdf998"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1032.mst", new FileData
                                    {
                                        MD5 = "4602c56cb044f7368564fd313d03ee00"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1033.mst", new FileData
                                    {
                                        MD5 = "638ad9eaa01c41e6657c523265ee4d28"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1034.mst", new FileData
                                    {
                                        MD5 = "1b4a7fa255ab869f6ca016e7989a2378"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1035.mst", new FileData
                                    {
                                        MD5 = "0f0ac52702b41aebcfbccf0d4499dfdc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1036.mst", new FileData
                                    {
                                        MD5 = "b0dd8ca074351ec1051d16daf1cadf88"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1038.mst", new FileData
                                    {
                                        MD5 = "2daa426ec3ee99327c42ea27e691d151"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1040.mst", new FileData
                                    {
                                        MD5 = "af8c0d79abd0ce0635aded81e70caabb"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1041.mst", new FileData
                                    {
                                        MD5 = "6db5be8288e9f44641bbfb305ccc8122"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1042.mst", new FileData
                                    {
                                        MD5 = "9a92f0f7911a26d9d90edcf32a35585a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1043.mst", new FileData
                                    {
                                        MD5 = "70754dbb6fca5cce389123c6172e4804"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1044.mst", new FileData
                                    {
                                        MD5 = "b7137c612efa6477ea005b016bcf28be"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1045.mst", new FileData
                                    {
                                        MD5 = "403a115d58f1858d5aa9d5a36150da4b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1046.mst", new FileData
                                    {
                                        MD5 = "eb21be7e958be1692335fea9febf79c6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1049.mst", new FileData
                                    {
                                        MD5 = "66f1e10acfe89ce0b297854704b1553f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1053.mst", new FileData
                                    {
                                        MD5 = "4b4153334a13f2365de0b5c207481aac"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1054.mst", new FileData
                                    {
                                        MD5 = "eff719568a907b227bec62d8dfb558b6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/1055.mst", new FileData
                                    {
                                        MD5 = "465c125d590ccf1344eaad278c3e46d4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/2052.mst", new FileData
                                    {
                                        MD5 = "69cf86b24146aa8ba7a7532a7384f0ef"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/2070.mst", new FileData
                                    {
                                        MD5 = "6173a99ad51876e5103b728c823221f4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/3084.mst", new FileData
                                    {
                                        MD5 = "5df18ff4046e5303c3363996d87e8a56"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Core-Static-Net4/ccc-core-static.msi",
                                    new FileData
                                    {
                                        MD5 = "887c672d1c5f57dca301c93d123d3f18"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1028.mst", new FileData
                                    {
                                        MD5 = "6ef83cf2527834352ccc8f754e5821e2"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1029.mst", new FileData
                                    {
                                        MD5 = "d99777c23700e1161129956e55f7fa9a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1030.mst", new FileData
                                    {
                                        MD5 = "bda40cda2b67a01877a858e7fbf5e8ae"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1031.mst", new FileData
                                    {
                                        MD5 = "074538ea1bb91542190b7d64d38d4c40"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1032.mst", new FileData
                                    {
                                        MD5 = "47d7cdf294921ea7a5942c78fda3f4e4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1033.mst", new FileData
                                    {
                                        MD5 = "7e3b06add96a15fca12b521ad5dd7a26"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1034.mst", new FileData
                                    {
                                        MD5 = "a0653c8d816a3dc5d87269a8975cef6f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1035.mst", new FileData
                                    {
                                        MD5 = "d1907c1102d8c22adecc796cc6fa076c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1036.mst", new FileData
                                    {
                                        MD5 = "e360cc91ad049fe10834a0c8e25a35cf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1038.mst", new FileData
                                    {
                                        MD5 = "d105341e8588307f377202404cca0b7a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1040.mst", new FileData
                                    {
                                        MD5 = "3c4cc05a745a673aaa4b93150ea344a5"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1041.mst", new FileData
                                    {
                                        MD5 = "e81247682f5b01db2b6cc24ab6657f58"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1042.mst", new FileData
                                    {
                                        MD5 = "c632025d08f06bca28ee2d6228187227"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1043.mst", new FileData
                                    {
                                        MD5 = "587e6a5628ae75b13f0ee0aead6083a7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1044.mst", new FileData
                                    {
                                        MD5 = "dfa4e02005741a2ca305105db1098ffc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1045.mst", new FileData
                                    {
                                        MD5 = "c8c4e3b78d5c21c91604d39ec672aef5"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1046.mst", new FileData
                                    {
                                        MD5 = "bceacb2b192fad3de1e4343e9c8ac4ee"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1049.mst", new FileData
                                    {
                                        MD5 = "1301cdeaefe097b8786cc02f118aa61e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1053.mst", new FileData
                                    {
                                        MD5 = "b7b061ffb09a7a467e132de5b47a3b81"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1054.mst", new FileData
                                    {
                                        MD5 = "1de88c20f2f3fdf1485062edb523b8c3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/1055.mst", new FileData
                                    {
                                        MD5 = "445f70b556f105118023c0e4cb9418aa"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/2052.mst", new FileData
                                    {
                                        MD5 = "e95e6ee5f74490eb6cfec8c337abb5bb"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/2070.mst", new FileData
                                    {
                                        MD5 = "b26d51e1081245922a99cc64662864dd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/3084.mst", new FileData
                                    {
                                        MD5 = "e7b5c243987606d3b7c98db1acaf8b50"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel/ccc-fuel.msi", new FileData
                                    {
                                        MD5 = "b7bb9f4012fcda83cd01b0f3ad936e3b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1028.mst", new FileData
                                    {
                                        MD5 = "b169fc44fc779d33d8e53ad0b438be9f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1029.mst", new FileData
                                    {
                                        MD5 = "961c8c4951b32ccd27a775f8b0cd96c7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1030.mst", new FileData
                                    {
                                        MD5 = "e763771643747a02a0e879da676c920c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1031.mst", new FileData
                                    {
                                        MD5 = "d5ddfaa1394ac100c82bb8104bc1da9b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1032.mst", new FileData
                                    {
                                        MD5 = "fa8f807f2a332b27d9a538d449b6e352"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1033.mst", new FileData
                                    {
                                        MD5 = "5f4d37486eb40165d28970e8a91884e3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1034.mst", new FileData
                                    {
                                        MD5 = "90933731fc36654a16c45899eda0b176"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1035.mst", new FileData
                                    {
                                        MD5 = "46ba14644af45db7d3c278793d771ef8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1036.mst", new FileData
                                    {
                                        MD5 = "5184e222d17c7a2de84a76effcf3c85e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1038.mst", new FileData
                                    {
                                        MD5 = "92b80791c963eb11f2b6851c49376263"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1040.mst", new FileData
                                    {
                                        MD5 = "e620beeb5cb5e4cb3569c412ec0a479c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1041.mst", new FileData
                                    {
                                        MD5 = "72052d1a8b172699d5be2856e1515ce6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1042.mst", new FileData
                                    {
                                        MD5 = "923ab96005276ac889c70efcd70b5d28"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1043.mst", new FileData
                                    {
                                        MD5 = "ff986ddeefa54e45a720594ebb2ab42f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1044.mst", new FileData
                                    {
                                        MD5 = "783fe8b14adcffde7c23736147726ba5"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1045.mst", new FileData
                                    {
                                        MD5 = "bd358f62f91b751fbec306d3e8760b50"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1046.mst", new FileData
                                    {
                                        MD5 = "215a97a11172fac0902c7f02973dc0be"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1049.mst", new FileData
                                    {
                                        MD5 = "feee6ac1f0e519c4cd52a5c177063ca7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1053.mst", new FileData
                                    {
                                        MD5 = "cfa90547332ea95a0a0372a0116fc4be"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1054.mst", new FileData
                                    {
                                        MD5 = "2e44a46807eb8ff2e1eb8ac5ae2716ba"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/1055.mst", new FileData
                                    {
                                        MD5 = "99e7653114ce6a4d66a7a4ea2b18090b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/2052.mst", new FileData
                                    {
                                        MD5 = "dafcfaf60e250ae889d4ebc2a1f1c618"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/2070.mst", new FileData
                                    {
                                        MD5 = "51af71f67e82a0343638e1a550505604"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/3084.mst", new FileData
                                    {
                                        MD5 = "9a3808c2f3a21b4820460127420f3b6d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Fuel64/ccc-fuel.msi", new FileData
                                    {
                                        MD5 = "96236ef8c380c0ec3e45f0df87c7bbf8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/cs/ccc-help-cs.msi", new FileData
                                    {
                                        MD5 = "c0bb6f9a0a21faecf132d37080a9a3af"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/da/ccc-help-da.msi", new FileData
                                    {
                                        MD5 = "b57ed9fe4094119d587c8cff610504c7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/de/ccc-help-de.msi", new FileData
                                    {
                                        MD5 = "f97e04f0da5ae547fe7b9a6b29561c4f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/el/ccc-help-el.msi", new FileData
                                    {
                                        MD5 = "328cc30248518e8840fd61797f55c034"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/en-us/ccc-help-en-US.msi", new FileData
                                    {
                                        MD5 = "6f940e923bef37deb6119faf9ecc04bb"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/es/ccc-help-es.msi", new FileData
                                    {
                                        MD5 = "43ba640efb14ea08a37e6f9c840c7b54"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/fi/ccc-help-fi.msi", new FileData
                                    {
                                        MD5 = "fd9de00186894e4f3362a95bed91e0b0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/fr/ccc-help-fr.msi", new FileData
                                    {
                                        MD5 = "250d2e2b185ba4fcae51be9a983e7998"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/hu/ccc-help-hu.msi", new FileData
                                    {
                                        MD5 = "e0893af092a8c4642834a083a2444faf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/it/ccc-help-it.msi", new FileData
                                    {
                                        MD5 = "2bc75330e0145d742b193d5bf34c34a0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/ja/ccc-help-ja.msi", new FileData
                                    {
                                        MD5 = "6ba86ad0a5c40473d2f5587cceda88df"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/ko/ccc-help-ko.msi", new FileData
                                    {
                                        MD5 = "a1face9bf754480a657b94d7637361e6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/nl/ccc-help-nl.msi", new FileData
                                    {
                                        MD5 = "339ce95279068bdfee0f0cc524e67269"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/no/ccc-help-no.msi", new FileData
                                    {
                                        MD5 = "77c66f216605dcf7dc5a0a801a4e7c53"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/pl/ccc-help-pl.msi", new FileData
                                    {
                                        MD5 = "980d962e7a7282af32ec887962559953"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/pt-BR/ccc-help-pt-BR.msi", new FileData
                                    {
                                        MD5 = "56252bb219e3a1d805f10515c24d0670"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/ru/ccc-help-ru.msi", new FileData
                                    {
                                        MD5 = "7db77a34cd2906d4fcb0c1ba0a7b9d19"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/sv/ccc-help-sv.msi", new FileData
                                    {
                                        MD5 = "5a3e6fd3895c628d42c8e44ab11760db"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/th/ccc-help-th.msi", new FileData
                                    {
                                        MD5 = "cedab16ca9601d30f5f973620ed40d6e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/tr/ccc-help-tr.msi", new FileData
                                    {
                                        MD5 = "1fb889e04c1eff4b98b74ad8beff33f8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/zh-CHS/ccc-help-chs.msi", new FileData
                                    {
                                        MD5 = "724d852ddee2c99c97760bb5760e3922"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Help/zh-CHT/ccc-help-cht.msi", new FileData
                                    {
                                        MD5 = "865d91431c290860962c0ca161263060"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Localisation/All/ccc-all.msi", new FileData
                                    {
                                        MD5 = "43737afca736db95f6ad00ebbea83023"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/MOM-InstallProxy-Net4/ccc-mom-installproxy.msi",
                                    new FileData
                                    {
                                        MD5 = "22da89e0e8405da43868b065f8b8525f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Profiles/Desktop/ccc-profiles-desktop.msi",
                                    new FileData
                                    {
                                        MD5 = "d71b15dd940a7653247acb0a129f590f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Utility-Net4/ccc-utility.msi", new FileData
                                    {
                                        MD5 = "6564e7dcdb901c8995ae7f2ef953b03c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CCC2/Utility64-Net4/ccc-utility64.msi",
                                    new FileData
                                    {
                                        MD5 = "2e133c93f78d24decf18e2d2725176ab"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1028.mst", new FileData
                                    {
                                        MD5 = "854c3d16537d0ddc21db9e80009efe67"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1029.mst", new FileData
                                    {
                                        MD5 = "359b492b5d3f53e16a79a9e342a09583"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1030.mst", new FileData
                                    {
                                        MD5 = "c996c3af7c6152d3db2e0c1073506040"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1031.mst", new FileData
                                    {
                                        MD5 = "e01d43bdd142eddd06d4a19bcba709c9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1032.mst", new FileData
                                    {
                                        MD5 = "fd98b2bf81a5a762c5877a46011f922e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1033.mst", new FileData
                                    {
                                        MD5 = "e5abffae9c23c05bc58cca91c20a34fb"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1034.mst", new FileData
                                    {
                                        MD5 = "6d64d534bb3d9d5f43deb573aab07adc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1035.mst", new FileData
                                    {
                                        MD5 = "9216a0020c9ca2260c5f96ca500e0d11"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1036.mst", new FileData
                                    {
                                        MD5 = "4c7576e0a70c1cc29cd9c0af1188a545"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1038.mst", new FileData
                                    {
                                        MD5 = "14a866c725bbac62bef46129c2e82d7b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1040.mst", new FileData
                                    {
                                        MD5 = "dfa494665285410808f3aa2a1768d640"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1041.mst", new FileData
                                    {
                                        MD5 = "8e1b03030790e1b994f8aaecb011ed1d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1042.mst", new FileData
                                    {
                                        MD5 = "36b3365e1ac6a34ff81adb6a5742c588"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1043.mst", new FileData
                                    {
                                        MD5 = "809e1af51df570447fcbac5d5e77c16a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1044.mst", new FileData
                                    {
                                        MD5 = "a0c548c93f4af4877802d808a681dd1c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1045.mst", new FileData
                                    {
                                        MD5 = "52b6ac8391a757809c30644316b73967"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1046.mst", new FileData
                                    {
                                        MD5 = "4024ee64f743894508a9610e33757d64"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1049.mst", new FileData
                                    {
                                        MD5 = "3084c4be0ef930aa3bd88dc8ef9f4359"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1053.mst", new FileData
                                    {
                                        MD5 = "d9de61e2c4c0400c22b4c6e017f5410e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1054.mst", new FileData
                                    {
                                        MD5 = "7d486e45d97c2d88a841a17ec0a7e6ab"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/1055.mst", new FileData
                                    {
                                        MD5 = "7197dc5ee4e32667f94c43b4e3edc6f7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/2052.mst", new FileData
                                    {
                                        MD5 = "e1b7f8c053c9cdbe24cc8f82fbf260af"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win32/ATICatalystInstallManager.msi",
                                    new FileData
                                    {
                                        MD5 = "ccb2b6c3bd5a72b381de3032492a4b43"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1028.mst", new FileData
                                    {
                                        MD5 = "511962735885837c5b4b172c38696944"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1029.mst", new FileData
                                    {
                                        MD5 = "1456ea379d7e2878219bb24fa390302d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1030.mst", new FileData
                                    {
                                        MD5 = "765129e94bc6cd838cc81ddbfc3726c8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1031.mst", new FileData
                                    {
                                        MD5 = "a39f872f25da22ad8a997a802a4b8b15"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1032.mst", new FileData
                                    {
                                        MD5 = "e0d75229a46d3a6fad4fb83fd3603504"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1033.mst", new FileData
                                    {
                                        MD5 = "a40e61faaecf0cb3e829f30c2da10b9e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1034.mst", new FileData
                                    {
                                        MD5 = "c29fcdaafa7d8553dc4f0adaf4d8a2f4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1035.mst", new FileData
                                    {
                                        MD5 = "d2f3cf0fd4c33c28d047b9ef92974fef"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1036.mst", new FileData
                                    {
                                        MD5 = "bb72bfe9078c49a02c463f1dfefd0356"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1038.mst", new FileData
                                    {
                                        MD5 = "28b10dc9383a4653dce8d105126392dd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1040.mst", new FileData
                                    {
                                        MD5 = "628408858eae9199384823e88e4ecd1c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1041.mst", new FileData
                                    {
                                        MD5 = "0da6ebacbbd130639c7a33a34444471f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1042.mst", new FileData
                                    {
                                        MD5 = "895e1d1456e5436f10c8555411e8b00f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1043.mst", new FileData
                                    {
                                        MD5 = "6fc13b18fe8f182ab300ecdc47e50a9a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1044.mst", new FileData
                                    {
                                        MD5 = "2ad16c92989ee7f5a1ed8db01c76073a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1045.mst", new FileData
                                    {
                                        MD5 = "fa4345f5b208fba520c4b1de875a3deb"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1046.mst", new FileData
                                    {
                                        MD5 = "24a048475ad2a936bf24944363372672"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1049.mst", new FileData
                                    {
                                        MD5 = "09b2f0d3d0d5a0551a64c7f91b50edc3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1053.mst", new FileData
                                    {
                                        MD5 = "abea8f95589ecc58dc78cc1fbb4e7a98"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1054.mst", new FileData
                                    {
                                        MD5 = "0332bd5d6c3c0f990b4b01ae165951e0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/1055.mst", new FileData
                                    {
                                        MD5 = "f377427e6aa46788143a48ca82e186f0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/2052.mst", new FileData
                                    {
                                        MD5 = "ca8b683bd842c7131d8f93ffff3a16cc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/CIM/Win64/ATICatalystInstallManager.msi",
                                    new FileData
                                    {
                                        MD5 = "97b364f2d0b4ee07c668d6dd73749c3f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/DotNet45/DotNet45.msi", new FileData
                                    {
                                        MD5 = "b7d6d245ea4ea68bbd60ca372d918783"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/DotNet45/dotnet45/dotnetfx45_full_x86_x64.exe",
                                    new FileData
                                    {
                                        MD5 = "d02dc8b69a702a47c083278938c4d2f1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/Raptr/Raptr.msi", new FileData
                                    {
                                        MD5 = "8c9d8794711f51a5a7738b875f51116e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/Raptr/RaptrInstaller/amd_ge_installer.exe",
                                    new FileData
                                    {
                                        MD5 = "bfcb798d1605b0ae95d05b9a2a52e597"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vc1064.msi", new FileData
                                    {
                                        MD5 = "1779026d83f566454849cb0b9062c165"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1028/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "129d8e8824b0d545adc29e571a6e2c02"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1028/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "12df3535e4c4ef95a8cb03fd509b5874"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1028/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "7c136b92983cec25f85336056e45f3e8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1031/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "117dabb5a055b09b6db6bcba8f911073"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1031/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "b13ff959adc5c3e9c4ba4c4a76244464"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1031/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "7c9ae49b3a400c728a55dd1cacc8ffb2"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1033/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "19d028345aadcc05697eec6d8c5b5874"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1033/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "5486ff60b072102ee3231fd743b290a1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1033/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "9547d24ac04b4d0d1dbf84f74f54faf7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1036/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "bbbbb0bda00fda985bb39fee5fd04ff8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1036/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "4ce519f7e9754ec03768edeedaeed926"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1036/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "93f57216fe49e7e2a75844edfccc2e09"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1040/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "f1602100f6c135ab5d8026e9248baf02"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1040/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "fe6b23186c2d77f7612bf7b1018a9b2a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1040/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "e4860fc5d4c114d5c0781714f3bf041a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1041/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "89d66a0b94450729015d021bc8f859e9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1041/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "6f86b79dbf15e810331df2ca77f1043a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1041/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "278fd7595b580a016705d00be363612f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1042/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "8203e9fc25a5720afb8c43e8be10c3b0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1042/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "e87ad0b3bf73f3e76500f28e195f7dc0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1042/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "fcfd69ec15a6897a940b0435439bf5fc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1049/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "5b95efbc01dc97ee9a6c6f64a49aa62d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1049/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "1290be72ed991a3a800a6b2a124073b2"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/1049/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "7ef74af6ab5760950a1d233c582099f1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/2052/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "6e5bddf58163b11c79577b35a87a4424"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/2052/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "150b5c3d1b452dccbe8f1313fda1b18c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/2052/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "407cdb7e1c2c862b486cde45f863ae6e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/3082/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "a920d4f55eae5febab1082ab2bcc2439"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/3082/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "05a95593c61c744759e52caf5e13502e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/3082/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "b057315a8c04df29b7e4fd2b257b75f4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/DHtmlHeader.html",
                                    new FileData
                                    {
                                        MD5 = "cd131d41791a543cc6f6ed1ea5bd257c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/DisplayIcon.ico",
                                    new FileData
                                    {
                                        MD5 = "f9657d290048e169ffabbbb9c7412be0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Print.ico",
                                    new FileData
                                    {
                                        MD5 = "7e55ddc6d611176e697d01c90a1212cf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Rotate1.ico",
                                    new FileData
                                    {
                                        MD5 = "26a00597735c5f504cf8b3e7e9a7a4c1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Rotate2.ico",
                                    new FileData
                                    {
                                        MD5 = "8419caa81f2377e09b7f2f6218e505ae"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Rotate3.ico",
                                    new FileData
                                    {
                                        MD5 = "924fd539523541d42dad43290e6c0db5"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Rotate4.ico",
                                    new FileData
                                    {
                                        MD5 = "bb55b5086a9da3097fb216c065d15709"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Rotate5.ico",
                                    new FileData
                                    {
                                        MD5 = "3b4861f93b465d724c60670b64fccfcf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Rotate6.ico",
                                    new FileData
                                    {
                                        MD5 = "70006bf18a39d258012875aefb92a3d1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Rotate7.ico",
                                    new FileData
                                    {
                                        MD5 = "fb4dfebe83f554faf1a5cec033a804d9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Rotate8.ico",
                                    new FileData
                                    {
                                        MD5 = "d1c53003264dce4effaf462c807e2d96"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Save.ico",
                                    new FileData
                                    {
                                        MD5 = "7d62e82d960a938c98da02b1d5201bd5"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Setup.ico",
                                    new FileData
                                    {
                                        MD5 = "3d25d679e0ff0b8c94273dcd8b07049d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/stop.ico",
                                    new FileData
                                    {
                                        MD5 = "5dfa8d3abcf4962d9ec41cfc7c0f75e3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/SysReqMet.ico",
                                    new FileData
                                    {
                                        MD5 = "661cbd315e9b23ba1ca19edab978f478"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/SysReqNotMet.ico",
                                    new FileData
                                    {
                                        MD5 = "ee2c05cc9d14c29f586d40eb90c610a9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/Thumbs.db",
                                    new FileData
                                    {
                                        MD5 = "b8c966f9c351e5a532acd1f3655081a2"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Graphics/warn.ico",
                                    new FileData
                                    {
                                        MD5 = "b2b1d79591fca103959806a4bf27d036"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/header.bmp", new FileData
                                    {
                                        MD5 = "3ad1a8c3b96993bcdf45244be2c00eef"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/ParameterInfo.xml",
                                    new FileData
                                    {
                                        MD5 = "03e01a43300d94a371458e14d5e41781"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Setup.exe", new FileData
                                    {
                                        MD5 = "006f8a615020a4a17f5e63801485df46"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/SetupEngine.dll",
                                    new FileData
                                    {
                                        MD5 = "84c1daf5f30ff99895ecab3a55354bcf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/SetupUi.dll", new FileData
                                    {
                                        MD5 = "eb881e3dddc84b20bd92abcec444455f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/SetupUi.xsd", new FileData
                                    {
                                        MD5 = "2fadd9e618eff8175f2a6e8b95c0cacc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/SplashScreen.bmp",
                                    new FileData
                                    {
                                        MD5 = "43b254d97b4fb6f9974ad3f935762c55"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/sqmapi.dll", new FileData
                                    {
                                        MD5 = "3f0363b40376047eff6a9b97d633b750"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/Strings.xml", new FileData
                                    {
                                        MD5 = "332adf643747297b9bfa9527eaefe084"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/UiInfo.xml", new FileData
                                    {
                                        MD5 = "812f8d2e53f076366fa3a214bb4cf558"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/vc_red.cab", new FileData
                                    {
                                        MD5 = "96253c1d1b54044a8640e9932dfca0b9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/vc_red.msi", new FileData
                                    {
                                        MD5 = "93bb8e3e96a206b39175345111d452e2"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx64/vcredist_x64/watermark.bmp",
                                    new FileData
                                    {
                                        MD5 = "1a5caafacfc8c7766e404d019249cf67"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vc1032.msi", new FileData
                                    {
                                        MD5 = "d9ad763dfbe6684d846eec67d4f69373"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1028/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "129d8e8824b0d545adc29e571a6e2c02"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1028/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "7fc06a77d9aafca9fb19fafa0f919100"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1028/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "7c136b92983cec25f85336056e45f3e8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1031/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "117dabb5a055b09b6db6bcba8f911073"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1031/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "b83c3803712e61811c438f6e98790369"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1031/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "7c9ae49b3a400c728a55dd1cacc8ffb2"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1033/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "19d028345aadcc05697eec6d8c5b5874"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1033/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "d642e322d1e8b739510ca540f8e779f9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1033/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "9547d24ac04b4d0d1dbf84f74f54faf7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1036/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "bbbbb0bda00fda985bb39fee5fd04ff8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1036/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "e382abc19294f779d2833287242e7bc6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1036/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "93f57216fe49e7e2a75844edfccc2e09"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1040/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "f1602100f6c135ab5d8026e9248baf02"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1040/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "0af948fe4142e34092f9dd47a4b8c275"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1040/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "e4860fc5d4c114d5c0781714f3bf041a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1041/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "89d66a0b94450729015d021bc8f859e9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1041/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "7fcfbc308b0c42dcbd8365ba62bada05"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1041/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "278fd7595b580a016705d00be363612f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1042/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "8203e9fc25a5720afb8c43e8be10c3b0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1042/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "71dfd70ae141f1d5c1366cb661b354b2"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1042/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "fcfd69ec15a6897a940b0435439bf5fc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1049/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "5b95efbc01dc97ee9a6c6f64a49aa62d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1049/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "0eeb554d0b9f9fcdb22401e2532e9cd0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/1049/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "7ef74af6ab5760950a1d233c582099f1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/2052/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "6e5bddf58163b11c79577b35a87a4424"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/2052/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "52b1dc12ce4153aa759fb3bbe04d01fc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/2052/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "407cdb7e1c2c862b486cde45f863ae6e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/3082/eula.rtf",
                                    new FileData
                                    {
                                        MD5 = "a920d4f55eae5febab1082ab2bcc2439"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/3082/LocalizedData.xml",
                                    new FileData
                                    {
                                        MD5 = "5397a12d466d55d566b4209e0e4f92d3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/3082/SetupResources.dll",
                                    new FileData
                                    {
                                        MD5 = "b057315a8c04df29b7e4fd2b257b75f4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/DHtmlHeader.html",
                                    new FileData
                                    {
                                        MD5 = "cd131d41791a543cc6f6ed1ea5bd257c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/DisplayIcon.ico",
                                    new FileData
                                    {
                                        MD5 = "f9657d290048e169ffabbbb9c7412be0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Print.ico",
                                    new FileData
                                    {
                                        MD5 = "7e55ddc6d611176e697d01c90a1212cf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Rotate1.ico",
                                    new FileData
                                    {
                                        MD5 = "26a00597735c5f504cf8b3e7e9a7a4c1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Rotate2.ico",
                                    new FileData
                                    {
                                        MD5 = "8419caa81f2377e09b7f2f6218e505ae"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Rotate3.ico",
                                    new FileData
                                    {
                                        MD5 = "924fd539523541d42dad43290e6c0db5"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Rotate4.ico",
                                    new FileData
                                    {
                                        MD5 = "bb55b5086a9da3097fb216c065d15709"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Rotate5.ico",
                                    new FileData
                                    {
                                        MD5 = "3b4861f93b465d724c60670b64fccfcf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Rotate6.ico",
                                    new FileData
                                    {
                                        MD5 = "70006bf18a39d258012875aefb92a3d1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Rotate7.ico",
                                    new FileData
                                    {
                                        MD5 = "fb4dfebe83f554faf1a5cec033a804d9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Rotate8.ico",
                                    new FileData
                                    {
                                        MD5 = "d1c53003264dce4effaf462c807e2d96"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Save.ico",
                                    new FileData
                                    {
                                        MD5 = "7d62e82d960a938c98da02b1d5201bd5"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Setup.ico",
                                    new FileData
                                    {
                                        MD5 = "3d25d679e0ff0b8c94273dcd8b07049d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/stop.ico",
                                    new FileData
                                    {
                                        MD5 = "5dfa8d3abcf4962d9ec41cfc7c0f75e3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/SysReqMet.ico",
                                    new FileData
                                    {
                                        MD5 = "661cbd315e9b23ba1ca19edab978f478"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/SysReqNotMet.ico",
                                    new FileData
                                    {
                                        MD5 = "ee2c05cc9d14c29f586d40eb90c610a9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/Thumbs.db",
                                    new FileData
                                    {
                                        MD5 = "30fd4ebf3890d461c50c90769ec1fef8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Graphics/warn.ico",
                                    new FileData
                                    {
                                        MD5 = "b2b1d79591fca103959806a4bf27d036"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/header.bmp", new FileData
                                    {
                                        MD5 = "3ad1a8c3b96993bcdf45244be2c00eef"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/ParameterInfo.xml",
                                    new FileData
                                    {
                                        MD5 = "66590f13f4c9ba563a9180bdf25a5b80"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Setup.exe", new FileData
                                    {
                                        MD5 = "006f8a615020a4a17f5e63801485df46"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/SetupEngine.dll",
                                    new FileData
                                    {
                                        MD5 = "84c1daf5f30ff99895ecab3a55354bcf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/SetupUi.dll", new FileData
                                    {
                                        MD5 = "eb881e3dddc84b20bd92abcec444455f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/SetupUi.xsd", new FileData
                                    {
                                        MD5 = "2fadd9e618eff8175f2a6e8b95c0cacc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/SplashScreen.bmp",
                                    new FileData
                                    {
                                        MD5 = "43b254d97b4fb6f9974ad3f935762c55"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/sqmapi.dll", new FileData
                                    {
                                        MD5 = "3f0363b40376047eff6a9b97d633b750"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/Strings.xml", new FileData
                                    {
                                        MD5 = "332adf643747297b9bfa9527eaefe084"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/UiInfo.xml", new FileData
                                    {
                                        MD5 = "812f8d2e53f076366fa3a214bb4cf558"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/vc_red.cab", new FileData
                                    {
                                        MD5 = "6c59fecf51931fb4540e571ae0310098"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/vc_red.msi", new FileData
                                    {
                                        MD5 = "cd2b99bb86ba6a499110c72b78b9324e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC10RTx86/vcredist_x86/watermark.bmp",
                                    new FileData
                                    {
                                        MD5 = "1a5caafacfc8c7766e404d019249cf67"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC12RTx64/vc1264.msi", new FileData
                                    {
                                        MD5 = "7ebd921d8521a4202c4dc4beacfc2360"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC12RTx64/vcredist_x64/vcredist_x64.exe",
                                    new FileData
                                    {
                                        MD5 = "ccae0434ac161e2ff081a13985c801fd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC12RTx86/vc1232.msi", new FileData
                                    {
                                        MD5 = "76d8c95be1ae92165c530d90ad91790a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Apps/VC12RTx86/vcredist_x86/vcredist_x86.exe",
                                    new FileData
                                    {
                                        MD5 = "2b6889ac60e866fcca633ef0ddc50df5"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmafd/WB/amdkmafd.cat", new FileData
                                    {
                                        MD5 = "e2db6b829061d710c7191c76e3945e94"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmafd/WB/amdkmafd.inf", new FileData
                                    {
                                        MD5 = "630e906c9b13b0dbbade8218f8b386c9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmafd/WB/amdkmafd.msi", new FileData
                                    {
                                        MD5 = "b6e9f9fe0ebc94e4422a7a26487d6f00"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmafd/WB/amdkmafd.sys", new FileData
                                    {
                                        MD5 = "88aca84c30b22430ff148a198cbfa6f3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmafd/WB64A/amdkmafd.cat", new FileData
                                    {
                                        MD5 = "95ae0f0fa2e55aa3f5952fd23a3a36bd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmafd/WB64A/amdkmafd.inf", new FileData
                                    {
                                        MD5 = "ded2daef5c2e2b58d67a968f9558ff7c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmafd/WB64A/amdkmafd.msi", new FileData
                                    {
                                        MD5 = "41991fc1336c1d42b873390fa9bdbe04"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmafd/WB64A/amdkmafd.sys", new FileData
                                    {
                                        MD5 = "f2ff8c1b41b3784edbd5c6d5397f403c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/W7/amdkmpfd.cat", new FileData
                                    {
                                        MD5 = "bf34490de3b56cf57e082137cc19c8b1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/W7/amdkmpfd.inf", new FileData
                                    {
                                        MD5 = "710c07a2932f2ad0380d342e16d29cfd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/W7/amdkmpfd.msi", new FileData
                                    {
                                        MD5 = "e17dca68bc7c85246efdbfb99321faaf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/W7/amdkmpfd.sys", new FileData
                                    {
                                        MD5 = "7d8be574d4f82ee5b8577c800d36264c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/W764A/amdkmpfd.cat", new FileData
                                    {
                                        MD5 = "fbf8e6401ad6c13c870fcb5cc9e8f946"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/W764A/amdkmpfd.inf", new FileData
                                    {
                                        MD5 = "63932be475368828cc66d6edb329e9de"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/W764A/amdkmpfd.msi", new FileData
                                    {
                                        MD5 = "df6def5d18f64ce84818438c28548852"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/W764A/amdkmpfd.sys", new FileData
                                    {
                                        MD5 = "ef4680f07516f6d61f6e0ba1d34b3a3a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/WB/amdkmpfd.cat", new FileData
                                    {
                                        MD5 = "263b71975c9d5b6a9eabb1270a18aadd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/WB/amdkmpfd.inf", new FileData
                                    {
                                        MD5 = "710c07a2932f2ad0380d342e16d29cfd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/WB/amdkmpfd.msi", new FileData
                                    {
                                        MD5 = "7f6fd1bd2f1897decf2beb2a8b05ca3e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/WB/amdkmpfd.sys", new FileData
                                    {
                                        MD5 = "6b5359c0ac9b1e35fbdf117b023b0d3c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/WB64A/amdkmpfd.cat", new FileData
                                    {
                                        MD5 = "9e8adf5708680386ba5209b1bfb26f6b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/WB64A/amdkmpfd.inf", new FileData
                                    {
                                        MD5 = "63932be475368828cc66d6edb329e9de"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/WB64A/amdkmpfd.msi", new FileData
                                    {
                                        MD5 = "04867e76359b16a862a8963343eb9a4a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/amdkmpfd/WB64A/amdkmpfd.sys", new FileData
                                    {
                                        MD5 = "c04f35935bf6274f5593b78c7b295760"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amd_opencl32.dll",
                                    new FileData
                                    {
                                        MD5 = "db4e3e4e5ef17b9c32e8b90e11f784f0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdacpksd.sy_",
                                    new FileData
                                    {
                                        MD5 = "b4c560934a3fc3a42d32f49097afdb36"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdave32.dl_",
                                    new FileData
                                    {
                                        MD5 = "896c269911b4be1101e56a8f19721f41"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdh264enc32.dll",
                                    new FileData
                                    {
                                        MD5 = "25aa6b458e3d008de04a2e95022ad96b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdhcp32.dl_",
                                    new FileData
                                    {
                                        MD5 = "9d1c16735dc88307b34c84e742c64646"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdhdl32.dl_",
                                    new FileData
                                    {
                                        MD5 = "72fffdc803519124e55ca74e77c73b23"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdhwdecoder_32.dll",
                                    new FileData
                                    {
                                        MD5 = "f1d013ccbbf857373f93376d25a6bda6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdicdxx.da_",
                                    new FileData
                                    {
                                        MD5 = "a2598a2f05e59e3ee781f2aad6763531"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdkmpfd.cbz",
                                    new FileData
                                    {
                                        MD5 = "8e4ad9e392a126b16ad2280d6bc156f4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdkmpfd.csz",
                                    new FileData
                                    {
                                        MD5 = "8e4ad9e392a126b16ad2280d6bc156f4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdkmpfd.ibz",
                                    new FileData
                                    {
                                        MD5 = "a7d7eab99d448d1466b0fac451aefde7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdkmpfd.isz",
                                    new FileData
                                    {
                                        MD5 = "a7d7eab99d448d1466b0fac451aefde7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdkmpfd.sbz",
                                    new FileData
                                    {
                                        MD5 = "895ef4232fb2807803aded78fe498d9f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdkmpfd.ssz",
                                    new FileData
                                    {
                                        MD5 = "895ef4232fb2807803aded78fe498d9f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdmantle32.dl_",
                                    new FileData
                                    {
                                        MD5 = "b382942d546cc1ce11ad77655ff9bc4b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdmftdecoder_32.dll",
                                    new FileData
                                    {
                                        MD5 = "3189885ce02a9089529f870fee5b40b0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdmftvideodecoder_32.dll",
                                    new FileData
                                    {
                                        MD5 = "9a692919fd81127440356c280a5b3e6a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdmiracast.dl_",
                                    new FileData
                                    {
                                        MD5 = "9bd614b5c43c9b209546059c45129950"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdmmcl.dl_",
                                    new FileData
                                    {
                                        MD5 = "ddd377774abe0ee1dac168ed5a78664f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdocl_as32.exe",
                                    new FileData
                                    {
                                        MD5 = "56b986d13c74903fe27b71ba85c76037"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdocl_ld32.exe",
                                    new FileData
                                    {
                                        MD5 = "28f4f5bac73505f71b8aec95b7fbe1dd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdocl.dll",
                                    new FileData
                                    {
                                        MD5 = "1af5140fef2438190265893e3c0e568c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/amdpcom32.dl_",
                                    new FileData
                                    {
                                        MD5 = "3e449b51fd33d824cdfae593fadbb405"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/ati2erec.dl_",
                                    new FileData
                                    {
                                        MD5 = "f9caa08328f859e55e28d4afbb78375d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiadlxx.dl_",
                                    new FileData
                                    {
                                        MD5 = "b70146c6e0fda4a8d00559e00aa96ce8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiapfxx.blb",
                                    new FileData
                                    {
                                        MD5 = "3680cb14b27abe6781f76820bb08080e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiapfxx.ex_",
                                    new FileData
                                    {
                                        MD5 = "7dae69ba6cadcfe5fb1398ee8dda8e20"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atibtmon.ex_",
                                    new FileData
                                    {
                                        MD5 = "a3a859635bcf4cafbb59e84d4579dc3f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/aticalcl.dl_",
                                    new FileData
                                    {
                                        MD5 = "61dc07ed93b951f9e08485e5ba8f8f6b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/aticaldd.dl_",
                                    new FileData
                                    {
                                        MD5 = "4bd635eb081ac557faa3e3db455e7d1f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/aticalrt.dl_",
                                    new FileData
                                    {
                                        MD5 = "2a92d472baefbde77c977bc11da6f24b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/aticfx32.dl_",
                                    new FileData
                                    {
                                        MD5 = "2f3b970753e0a167d1d1b682fc62b529"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atidemgy.dl_",
                                    new FileData
                                    {
                                        MD5 = "c78581a9b7a182f40bd1cdc23d402116"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atidxx32.dl_",
                                    new FileData
                                    {
                                        MD5 = "b36553778ed225120e4474ca26aa746d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atieclxx.ex_",
                                    new FileData
                                    {
                                        MD5 = "f1ea56c99c1b41fe65c8795759bee2ae"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiesrxx.ex_",
                                    new FileData
                                    {
                                        MD5 = "ed553734e50198a0d28a186a1c02da6a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atigktxx.dl_",
                                    new FileData
                                    {
                                        MD5 = "058a20d3d2d577ed3c8dbefe7b0ca32d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiglpxx.dl_",
                                    new FileData
                                    {
                                        MD5 = "6023c2e8941b82457c28a1c14a78a8b7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiicdxx.da_",
                                    new FileData
                                    {
                                        MD5 = "156aa76dfd19389969a48f06e9494fda"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atikmdag.sy_",
                                    new FileData
                                    {
                                        MD5 = "c97aaa96a70e2f20497848c0faff7ce8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atikmpag.sy_",
                                    new FileData
                                    {
                                        MD5 = "5d3f874495edaf5eccb8261d903cf71d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atimpc32.dl_",
                                    new FileData
                                    {
                                        MD5 = "db1ef400af1eabdf012d8158930f73f7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atimuixx.dl_",
                                    new FileData
                                    {
                                        MD5 = "56f307ab3fc552152c90e383c3eacda9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiodcli.ex_",
                                    new FileData
                                    {
                                        MD5 = "189fc384b16089f0ab1275c2eb1d3023"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiode.ex_",
                                    new FileData
                                    {
                                        MD5 = "0506304024976524b7d3e03169259a23"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atioglxx.dl_",
                                    new FileData
                                    {
                                        MD5 = "8b987699f1950ea76df2e28c6e0bb53f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atipblag.dat",
                                    new FileData
                                    {
                                        MD5 = "64a0869f18560cd529120ade00155c3e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atisamu32.dl_",
                                    new FileData
                                    {
                                        MD5 = "d0f1776e508092ce20948067fb5f9218"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atitmmxx.dl_",
                                    new FileData
                                    {
                                        MD5 = "e5d81212f0b661933f6e200bcb412f2d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiu9pag.dl_",
                                    new FileData
                                    {
                                        MD5 = "b2b846276c616464758d002cdbcb02f1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiumdag.dl_",
                                    new FileData
                                    {
                                        MD5 = "7a372bc9bc89b4204b061279b6ece1e9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiumdva.ca_",
                                    new FileData
                                    {
                                        MD5 = "a69a8de36726a83c097a4a17a41fea39"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiumdva.dl_",
                                    new FileData
                                    {
                                        MD5 = "da42377d4b58ff43abb6f25f7fb11290"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/atiuxpag.dl_",
                                    new FileData
                                    {
                                        MD5 = "b61f04c94b220f2e7fe1cb9121aa30a3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/ativce02.dat",
                                    new FileData
                                    {
                                        MD5 = "f395ab268d6f69c808dcb646f60ae258"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/ativce03.dat",
                                    new FileData
                                    {
                                        MD5 = "a5bf352289ae436891325ff6ff235651"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/ativvaxy_cik_nd.dat",
                                    new FileData
                                    {
                                        MD5 = "a13e40a30edd65a76ab887940577a076"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/ativvaxy_cik.dat",
                                    new FileData
                                    {
                                        MD5 = "c9c61717ac29ade62125ed45ab3a9cf0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/ativvaxy_vi_nd.dat",
                                    new FileData
                                    {
                                        MD5 = "4332acf08e7f7f7712f4775426632d80"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/ativvaxy_vi.dat",
                                    new FileData
                                    {
                                        MD5 = "c622804b5377f28155cddcc3987db93a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/ativvsva.dat",
                                    new FileData
                                    {
                                        MD5 = "7c163ede63854539828f5b2c1bc529fd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/ativvsvl.dat",
                                    new FileData
                                    {
                                        MD5 = "219d7091dd1d93728392337fe9c7add6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/clinfo.exe",
                                    new FileData
                                    {
                                        MD5 = "96def427d29aaa90842db94641c27aaf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/coinst_14.20.dll",
                                    new FileData
                                    {
                                        MD5 = "295a76becb89bac24350b67ae7478941"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/kapp_ci.sbin",
                                    new FileData
                                    {
                                        MD5 = "16b37bdc0011f8c00c2b78b41f9a8a0a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/kapp_si.sbin",
                                    new FileData
                                    {
                                        MD5 = "9ea161fcfbf405e9aba23717aa8d47a3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/mantle32.dl_",
                                    new FileData
                                    {
                                        MD5 = "1daacda395eae5f24d4f867df26ce476"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/mantleaxl32.dl_",
                                    new FileData
                                    {
                                        MD5 = "a0fc34cafa9711607086f21c5665c33e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/openvideo.dll",
                                    new FileData
                                    {
                                        MD5 = "1337cc75bafbc2ec49ee68db383472fc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/ovdecode.dll",
                                    new FileData
                                    {
                                        MD5 = "1da41a94ea099d2adb2f4cf78d45e116"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/samu_krnl_ci.sbin",
                                    new FileData
                                    {
                                        MD5 = "585939d3405360774b139bf740866018"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/B172487/samu_krnl_isv_ci.sbin",
                                    new FileData
                                    {
                                        MD5 = "a769b352b827590ea4ccac16e6269e33"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/CB172976.cat", new FileData
                                    {
                                        MD5 = "d83fac44b98f5bd1c074da13b791d07f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/CB172976.inf", new FileData
                                    {
                                        MD5 = "d45dffc6cf2e3c0cb7587871f88d00fd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/CB172976.msi", new FileData
                                    {
                                        MD5 = "dc99feaf5647caf7e6656c240506cd5c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/cw172976.cat", new FileData
                                    {
                                        MD5 = "eb3abd8af3c86fdc6736fc10a40a67b9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/CW172976.inf", new FileData
                                    {
                                        MD5 = "42f88f4ecd89f3dc6b1b59213cc5d7bd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB_INF/CW172976.msi", new FileData
                                    {
                                        MD5 = "74b0b55c8bd8b7d8620e88df1d96c282"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amd_opencl32.dll",
                                    new FileData
                                    {
                                        MD5 = "db4e3e4e5ef17b9c32e8b90e11f784f0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amd_opencl64.dll",
                                    new FileData
                                    {
                                        MD5 = "d302c5c94ad02aecfbd281939f676ffd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdacpksd.sy_",
                                    new FileData
                                    {
                                        MD5 = "84594206d32d89b1c74f81c239fb22d0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdave32.dl_",
                                    new FileData
                                    {
                                        MD5 = "896c269911b4be1101e56a8f19721f41"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdave64.dl_",
                                    new FileData
                                    {
                                        MD5 = "1dfe4afdfe75e50d054b2258dc2f8152"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdh264enc32.dll",
                                    new FileData
                                    {
                                        MD5 = "25aa6b458e3d008de04a2e95022ad96b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdh264enc64.dll",
                                    new FileData
                                    {
                                        MD5 = "8e2a27ce19ce155bea1289bba9388360"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdhcp32.dl_",
                                    new FileData
                                    {
                                        MD5 = "431c676dd7c44f75bee3de17f1c2e9d1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdhcp64.dl_",
                                    new FileData
                                    {
                                        MD5 = "fa11a71b9396631d78ce3bc4bce2267a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdhdl32.dl_",
                                    new FileData
                                    {
                                        MD5 = "72fffdc803519124e55ca74e77c73b23"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdhdl64.dl_",
                                    new FileData
                                    {
                                        MD5 = "974249da9fcf7820bf85205bd26e839a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdhsars.dl_",
                                    new FileData
                                    {
                                        MD5 = "1899a6bae132be27355571b7ff2f5f9b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdhwdecoder_32.dll",
                                    new FileData
                                    {
                                        MD5 = "f1d013ccbbf857373f93376d25a6bda6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdhwdecoder_64.dll",
                                    new FileData
                                    {
                                        MD5 = "d10cdad79971d462f6fde4056b8f3c29"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdicdxx.da_",
                                    new FileData
                                    {
                                        MD5 = "a2598a2f05e59e3ee781f2aad6763531"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdkfd.sy_",
                                    new FileData
                                    {
                                        MD5 = "ff373ddbcd18a101b8d59c4a7c526cd7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdkmpfd.cbz",
                                    new FileData
                                    {
                                        MD5 = "7c3a015b05ba6140056e54a9e2522285"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdkmpfd.csz",
                                    new FileData
                                    {
                                        MD5 = "7c3a015b05ba6140056e54a9e2522285"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdkmpfd.ibz",
                                    new FileData
                                    {
                                        MD5 = "510d1a4050e0fee8e58d59d5a97c91d6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdkmpfd.isz",
                                    new FileData
                                    {
                                        MD5 = "510d1a4050e0fee8e58d59d5a97c91d6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdkmpfd.sbz",
                                    new FileData
                                    {
                                        MD5 = "e567b9b867d6e360f92549c7a895b84d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdkmpfd.ssz",
                                    new FileData
                                    {
                                        MD5 = "e567b9b867d6e360f92549c7a895b84d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdmantle32.dl_",
                                    new FileData
                                    {
                                        MD5 = "b382942d546cc1ce11ad77655ff9bc4b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdmantle64.dl_",
                                    new FileData
                                    {
                                        MD5 = "49044e1da4cc1094856c75d07b33a171"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdmftdecoder_32.dll",
                                    new FileData
                                    {
                                        MD5 = "3189885ce02a9089529f870fee5b40b0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdmftdecoder_64.dll",
                                    new FileData
                                    {
                                        MD5 = "33adfed8f7ad43ec6750810ec18a3fc1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdmftvideodecoder_32.dll",
                                    new FileData
                                    {
                                        MD5 = "d650a7eed1adf2b311a2003138636ea8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdmftvideodecoder_64.dll",
                                    new FileData
                                    {
                                        MD5 = "d692656590c64d6fcb8fe33a25505467"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdmiracast.dl_",
                                    new FileData
                                    {
                                        MD5 = "4313e82331bc57ed8da7d1cb49198260"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdmmcl.dl_",
                                    new FileData
                                    {
                                        MD5 = "ddd377774abe0ee1dac168ed5a78664f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdmmcl6.dl_",
                                    new FileData
                                    {
                                        MD5 = "7102aa5164d2f95e5e4e2aed673229ce"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdocl_as32.exe",
                                    new FileData
                                    {
                                        MD5 = "56b986d13c74903fe27b71ba85c76037"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdocl_as64.exe",
                                    new FileData
                                    {
                                        MD5 = "ecc9d68f5bef5cd67be2d2f758661980"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdocl_ld32.exe",
                                    new FileData
                                    {
                                        MD5 = "28f4f5bac73505f71b8aec95b7fbe1dd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdocl_ld64.exe",
                                    new FileData
                                    {
                                        MD5 = "dd3e0fe46f9ab3f9a339f4dd3b2b2e4c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdocl.dll",
                                    new FileData
                                    {
                                        MD5 = "1af5140fef2438190265893e3c0e568c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdocl64.dll",
                                    new FileData
                                    {
                                        MD5 = "de299d0c73d2cb349119ec51e171590e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdpcom32.dl_",
                                    new FileData
                                    {
                                        MD5 = "fa782a46b09645ea6e7502c2f06ac1cf"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/amdpcom64.dl_",
                                    new FileData
                                    {
                                        MD5 = "e466ccf6948ad2216ab3322c09670023"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/ati2erec.dl_",
                                    new FileData
                                    {
                                        MD5 = "f9caa08328f859e55e28d4afbb78375d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiadlxx.dl_",
                                    new FileData
                                    {
                                        MD5 = "e21412c51f750b9b0a450145eb17d5ff"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiadlxy.dl_",
                                    new FileData
                                    {
                                        MD5 = "b70146c6e0fda4a8d00559e00aa96ce8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiapfxx.blb",
                                    new FileData
                                    {
                                        MD5 = "3680cb14b27abe6781f76820bb08080e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiapfxx.ex_",
                                    new FileData
                                    {
                                        MD5 = "7dae69ba6cadcfe5fb1398ee8dda8e20"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atibtmon.ex_",
                                    new FileData
                                    {
                                        MD5 = "a3a859635bcf4cafbb59e84d4579dc3f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/aticalcl.dl_",
                                    new FileData
                                    {
                                        MD5 = "61dc07ed93b951f9e08485e5ba8f8f6b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/aticalcl64.dl_",
                                    new FileData
                                    {
                                        MD5 = "a54c6c79e80651b72313558aea43e67b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/aticaldd.dl_",
                                    new FileData
                                    {
                                        MD5 = "4bd635eb081ac557faa3e3db455e7d1f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/aticaldd64.dl_",
                                    new FileData
                                    {
                                        MD5 = "e390241635c6934acbae519de87890dc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/aticalrt.dl_",
                                    new FileData
                                    {
                                        MD5 = "2a92d472baefbde77c977bc11da6f24b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/aticalrt64.dl_",
                                    new FileData
                                    {
                                        MD5 = "ccd17fc9658816ce9020ac9b16c21e89"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/aticfx32.dl_",
                                    new FileData
                                    {
                                        MD5 = "21d715bac9432e8e48c6cc194a39506c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/aticfx64.dl_",
                                    new FileData
                                    {
                                        MD5 = "95c94d0dd767452cb6ee22466c8b7fac"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atidemgy.dl_",
                                    new FileData
                                    {
                                        MD5 = "c78581a9b7a182f40bd1cdc23d402116"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atidxx32.dl_",
                                    new FileData
                                    {
                                        MD5 = "6aa00de8d836df8354123562f8ef2584"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atidxx64.dl_",
                                    new FileData
                                    {
                                        MD5 = "c8f7c5e65ca21997650f86774dfe3b31"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atieclxx.ex_",
                                    new FileData
                                    {
                                        MD5 = "7af8f2de7731ecf3c7f6510562b0012e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiesrxx.ex_",
                                    new FileData
                                    {
                                        MD5 = "071a9f1522d34a6a1770f891f0aa73fd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atig6pxx.dl_",
                                    new FileData
                                    {
                                        MD5 = "e5a446bb0d90e1babd4531becf738148"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atig6txx.dl_",
                                    new FileData
                                    {
                                        MD5 = "0950eea537191cc3592b604a115013b4"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atigktxx.dl_",
                                    new FileData
                                    {
                                        MD5 = "058a20d3d2d577ed3c8dbefe7b0ca32d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiglpxx.dl_",
                                    new FileData
                                    {
                                        MD5 = "6023c2e8941b82457c28a1c14a78a8b7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiicdxx.da_",
                                    new FileData
                                    {
                                        MD5 = "156aa76dfd19389969a48f06e9494fda"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atikmdag.sy_",
                                    new FileData
                                    {
                                        MD5 = "e319e2853415ae2e3a91cb6ad1755c7e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atikmpag.sy_",
                                    new FileData
                                    {
                                        MD5 = "28c49e69e2a64fb7ad021b0088d02315"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atimpc32.dl_",
                                    new FileData
                                    {
                                        MD5 = "960b6047502d413907d7e7f3e93f7d4a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atimpc64.dl_",
                                    new FileData
                                    {
                                        MD5 = "0aaed7d374e60aebd90a39a0e43b0ad9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atimuixx.dl_",
                                    new FileData
                                    {
                                        MD5 = "a9f80260aea725ed999514097f660b4b"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atio6axx.dl_",
                                    new FileData
                                    {
                                        MD5 = "b9cc7f7894da435ec4dc57f7b2b2379a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiodcli.ex_",
                                    new FileData
                                    {
                                        MD5 = "7d37be8f501966da15b5190c5d8f47c3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiode.ex_",
                                    new FileData
                                    {
                                        MD5 = "b83525663020a80549d127b78e39c88f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atioglxx.dl_",
                                    new FileData
                                    {
                                        MD5 = "8b987699f1950ea76df2e28c6e0bb53f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atipblag.dat",
                                    new FileData
                                    {
                                        MD5 = "64a0869f18560cd529120ade00155c3e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atisamu32.dl_",
                                    new FileData
                                    {
                                        MD5 = "d0f1776e508092ce20948067fb5f9218"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atisamu64.dl_",
                                    new FileData
                                    {
                                        MD5 = "9fcad73fd3c4ae0b25352a458bcee2d3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atitmm64.dl_",
                                    new FileData
                                    {
                                        MD5 = "5558d32615500bcd6641b8cda9d5db8d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiu9p64.dl_",
                                    new FileData
                                    {
                                        MD5 = "08c1a7cd35929ae547decfbec947682f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiu9pag.dl_",
                                    new FileData
                                    {
                                        MD5 = "a1902fedac99b6e7676f849e48393c91"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiumd64.dl_",
                                    new FileData
                                    {
                                        MD5 = "ad01051b0dfaa06c024f810c4e9c98a5"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiumd6a.ca_",
                                    new FileData
                                    {
                                        MD5 = "355272df280a8539aab12c3c50d0f6e7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiumd6a.dl_",
                                    new FileData
                                    {
                                        MD5 = "6c2406ee98e74a5dea77443655c8cc96"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiumdag.dl_",
                                    new FileData
                                    {
                                        MD5 = "15db71089ab38dbf1bd3ea414a25a35f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiumdva.ca_",
                                    new FileData
                                    {
                                        MD5 = "a69a8de36726a83c097a4a17a41fea39"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiumdva.dl_",
                                    new FileData
                                    {
                                        MD5 = "68d0ef4b2a0d6461a041566dc021a545"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiuxp64.dl_",
                                    new FileData
                                    {
                                        MD5 = "969d706c71a548b1b2277c069ee41386"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/atiuxpag.dl_",
                                    new FileData
                                    {
                                        MD5 = "810345ff30c30e67899d49aebc98be72"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/ativce02.dat",
                                    new FileData
                                    {
                                        MD5 = "f395ab268d6f69c808dcb646f60ae258"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/ativce03.dat",
                                    new FileData
                                    {
                                        MD5 = "a5bf352289ae436891325ff6ff235651"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/ativvaxy_cik_nd.dat",
                                    new FileData
                                    {
                                        MD5 = "a13e40a30edd65a76ab887940577a076"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/ativvaxy_cik.dat",
                                    new FileData
                                    {
                                        MD5 = "c9c61717ac29ade62125ed45ab3a9cf0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/ativvaxy_vi_nd.dat",
                                    new FileData
                                    {
                                        MD5 = "4332acf08e7f7f7712f4775426632d80"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/ativvaxy_vi.dat",
                                    new FileData
                                    {
                                        MD5 = "c622804b5377f28155cddcc3987db93a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/ativvsva.dat",
                                    new FileData
                                    {
                                        MD5 = "7c163ede63854539828f5b2c1bc529fd"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/ativvsvl.dat",
                                    new FileData
                                    {
                                        MD5 = "219d7091dd1d93728392337fe9c7add6"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/clinfo.exe",
                                    new FileData
                                    {
                                        MD5 = "46b4b363f828ea2a24903c784d69b509"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/coinst_14.20.dll",
                                    new FileData
                                    {
                                        MD5 = "21e39d73cdfa23a2e93dfadbec748776"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/kapp_ci.sbin",
                                    new FileData
                                    {
                                        MD5 = "16b37bdc0011f8c00c2b78b41f9a8a0a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/kapp_si.sbin",
                                    new FileData
                                    {
                                        MD5 = "9ea161fcfbf405e9aba23717aa8d47a3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/mantle32.dl_",
                                    new FileData
                                    {
                                        MD5 = "1daacda395eae5f24d4f867df26ce476"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/mantle64.dl_",
                                    new FileData
                                    {
                                        MD5 = "1114899724cfade65262ce981bff4e1a"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/mantleaxl32.dl_",
                                    new FileData
                                    {
                                        MD5 = "a0fc34cafa9711607086f21c5665c33e"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/mantleaxl64.dl_",
                                    new FileData
                                    {
                                        MD5 = "4fc2f260331807d3877c2905d70e5003"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/openvideo.dll",
                                    new FileData
                                    {
                                        MD5 = "1337cc75bafbc2ec49ee68db383472fc"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/openvideo64.dll",
                                    new FileData
                                    {
                                        MD5 = "53994beeb1c7de2013cbb395e6570ff1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/ovdecode.dll",
                                    new FileData
                                    {
                                        MD5 = "1da41a94ea099d2adb2f4cf78d45e116"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/ovdecode64.dll",
                                    new FileData
                                    {
                                        MD5 = "d05d44d7c98eaddcfe4bea9251d6aaca"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/samu_krnl_ci.sbin",
                                    new FileData
                                    {
                                        MD5 = "585939d3405360774b139bf740866018"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/B172487/samu_krnl_isv_ci.sbin",
                                    new FileData
                                    {
                                        MD5 = "a769b352b827590ea4ccac16e6269e33"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/c7172976.cat", new FileData
                                    {
                                        MD5 = "bc09c63e11b8381af7f48262f7d6b524"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/C7172976.inf", new FileData
                                    {
                                        MD5 = "c41b6f0ee89312ffbeeaf993d1a3cb9f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/C7172976.msi", new FileData
                                    {
                                        MD5 = "bda68cbc0597954101d33910cafd0ed1"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/CU172976.cat", new FileData
                                    {
                                        MD5 = "d7635dc52a7f5d5707a62db06950021c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/CU172976.inf", new FileData
                                    {
                                        MD5 = "840e831f272eb4d3a2f576b4e99e3d24"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/Display/WB6A_INF/CU172976.msi", new FileData
                                    {
                                        MD5 = "90532d551252fa6948ccf2c3ef5b91f3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W7/atihdw73.cat", new FileData
                                    {
                                        MD5 = "440e535fa4e4b9accabbe607a738799c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W7/AtihdW73.inf", new FileData
                                    {
                                        MD5 = "0952ed669cf18df925e637c184f11594"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W7/AtihdW73.msi", new FileData
                                    {
                                        MD5 = "ccb641ed5a1eee19ac59690569bf4c99"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W7/atihdw73.sys", new FileData
                                    {
                                        MD5 = "8d065a0d80593c3b0fcdbadce050e01c"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W7/delayapo.dll", new FileData
                                    {
                                        MD5 = "b28c7eb879728e4aabdfb3ca8073c17d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W764A/atihdw76.cat", new FileData
                                    {
                                        MD5 = "743563c46045a9ce17ce44dcbfc577c7"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W764A/AtihdW76.inf", new FileData
                                    {
                                        MD5 = "231d19c1fb4d62a70d03007fa5262e6d"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W764A/AtihdW76.msi", new FileData
                                    {
                                        MD5 = "cc5667c34997eb98e820b4b01cf37a02"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W764A/atihdw76.sys", new FileData
                                    {
                                        MD5 = "ff50a62efa151ebcfcdd37a76ca9ea92"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/W764A/delayapo.dll", new FileData
                                    {
                                        MD5 = "bc49936bdd6b28a7b60c9f5c85cf5461"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB/amdacpksl.sys", new FileData
                                    {
                                        MD5 = "e19d6fc0e4b9807fc4ecc5f23ead0335"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB/atihdwb3.cat", new FileData
                                    {
                                        MD5 = "3abaa835cf6bdac301f9c374c419b779"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB/AtihdWB3.inf", new FileData
                                    {
                                        MD5 = "76205bc84813afe71ad287d7065a7db8"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB/AtihdWB3.msi", new FileData
                                    {
                                        MD5 = "4dc56e762478fcaeafd164b7f05a7ffe"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB/atihdwb3.sys", new FileData
                                    {
                                        MD5 = "ec739addb4d58f96dbb7f934387c7ea0"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB/delayapo.dll", new FileData
                                    {
                                        MD5 = "56c0328f245031c867ef6f5e7d9315f9"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB64A/amdacpksl.sys", new FileData
                                    {
                                        MD5 = "4cea306bac2e3dca0cd740003bc70b95"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB64A/atihdwb6.cat", new FileData
                                    {
                                        MD5 = "d0591135dc376fca260986003034b831"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB64A/AtihdWB6.inf", new FileData
                                    {
                                        MD5 = "83f769d263fef38bd5546c09d2ba01ec"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB64A/AtihdWB6.msi", new FileData
                                    {
                                        MD5 = "06bf45936c159b58299bdf27bea9773f"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB64A/atihdwb6.sys", new FileData
                                    {
                                        MD5 = "517334a411cd079ee9aef4c2167875a5"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Packages/Drivers/WDM/HDMI/WB64A/delayapo.dll", new FileData
                                    {
                                        MD5 = "dd4b1773cb86ecc2476aeeddfa7ceff3"
                                    }
                                },
                                {
                                    "Driver/Win7_Win8.1/Setup.exe", new FileData
                                    {
                                        MD5 = "825b08ae4b376f69ed6e18fab885bede"
                                    }
                                },
                                {
                                    "gcapi_dll.dll", new FileData
                                    {
                                        MD5 = "d496480a00abde0655c0fdce9530b43e"
                                    }
                                },
                                {
                                    "Manual/GPUTweak_Streaming.pdf", new FileData
                                    {
                                        MD5 = "fc7b7957b50d89767a67f69eaca19d62"
                                    }
                                },
                                {
                                    "Manual/GPUTweak.pdf", new FileData
                                    {
                                        MD5 = "6631243b48956b2a78ccc7b68bc75e35"
                                    }
                                },
                                {
                                    "Manual/setup.exe", new FileData
                                    {
                                        MD5 = "b3b1b05052c6a25ddf19259bcb8c510f"
                                    }
                                },
                                {
                                    "Manual/UserManual.pdf", new FileData
                                    {
                                        MD5 = "10a9f8dca474e5f3713f9d1472fe56e1"
                                    }
                                },
                                {
                                    "Readme.txt", new FileData
                                    {
                                        MD5 = "1af06f2196ff3ebdd1d491e01e994029"
                                    }
                                },
                                {
                                    "Setup.ini", new FileData
                                    {
                                        MD5 = "ea79982ff80a7b658ec215392a15e1e9"
                                    }
                                },
                                {
                                    "UI.exe", new FileData
                                    {
                                        MD5 = "d5fabe00f14f4f833e6f6a4b086defdc"
                                    }
                                },
                                {
                                    "Utility/APRP/APRP.msi", new FileData
                                    {
                                        MD5 = "e257c87859c77934bc099f3fd595c706"
                                    }
                                },
                                {
                                    "Utility/APRP/setup.exe", new FileData
                                    {
                                        MD5 = "50877563cdb66654de7b848621c1da8d"
                                    }
                                },
                                {
                                    "Utility/ChkVGA.exe", new FileData
                                    {
                                        MD5 = "7a94752792b79b2264753d5c38fb058b"
                                    }
                                },
                                {
                                    "Utility/GoogleChrome/Chrome.exe", new FileData
                                    {
                                        MD5 = "0a625ce79ee3971efe6f1c31c5f58f4e"
                                    }
                                },
                                {
                                    "Utility/GoogleChrome/gcapi_dll.dll", new FileData
                                    {
                                        MD5 = "d496480a00abde0655c0fdce9530b43e"
                                    }
                                },
                                {
                                    "Utility/GoogleChrome/setup.exe", new FileData
                                    {
                                        MD5 = "c4fbd3dd1d3106c99d78fc72d02dc0fa"
                                    }
                                },
                                {
                                    "Utility/GoogleToolbar/gtapi_signed.dll", new FileData
                                    {
                                        MD5 = "23700aa70d1751d592d8641fc0e0660f"
                                    }
                                },
                                {
                                    "Utility/GoogleToolbar/gtapi_signed64.dll", new FileData
                                    {
                                        MD5 = "7ce72b3f4b7354a56c6740422701615c"
                                    }
                                },
                                {
                                    "Utility/GoogleToolbar/setup.exe", new FileData
                                    {
                                        MD5 = "ed8b323cd060a081a8603f6bb297fdc0"
                                    }
                                },
                                {
                                    "Utility/GoogleToolbar/Toolbar.exe", new FileData
                                    {
                                        MD5 = "b0ca1e1ffd7c87c6f2e5249c13a26f82"
                                    }
                                },
                                {
                                    "Utility/GPUTweak/setup.exe", new FileData
                                    {
                                        MD5 = "3278b946f5cedb2beecb888b6973055f"
                                    }
                                },
                                {
                                    "Utility/GPUTweak/setup.iss", new FileData
                                    {
                                        MD5 = "79a01fbb32cbc7a8ca11da5955d1789c"
                                    }
                                },
                                {
                                    "Utility/GPUTweak/x64_setup.iss", new FileData
                                    {
                                        MD5 = "57e1c9d392992d658611b23fe8e28b0b"
                                    }
                                },
                                {
                                    "Utility/Streaming/setup.exe", new FileData
                                    {
                                        MD5 = "b3664c60cbf851675362a64db5803e4d"
                                    }
                                },
                                {
                                    "Utility/Streaming/setup.iss", new FileData
                                    {
                                        MD5 = "df47a88b3e9353932f7351896008ca20"
                                    }
                                },
                                {
                                    "Utility/Streaming/setup.log", new FileData
                                    {
                                        MD5 = "933b3b8f1cdeab4efa839392691050a1"
                                    }
                                },
                                {
                                    "Utility/Streaming/x64_setup.iss", new FileData
                                    {
                                        MD5 = "770d9d56f787d49e66be31413e03ebc3"
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