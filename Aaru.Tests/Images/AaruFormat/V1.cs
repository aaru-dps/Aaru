// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : V1.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Filters;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images.AaruFormat
{
    [TestFixture]
    public class V1
    {
        readonly string[] _testFiles =
        {
            "cdiready_the_apprentice.aif", "report_audiocd.aif", "report_cdr.aif", "report_cdrom.aif",
            "report_cdrw_2x.aif", "report_dvd+r.aif", "report_dvd-r.aif", "report_dvd-ram_v1.aif",
            "report_dvd-ram_v2.aif", "report_dvd+r_dl.aif", "report_dvd-rom.aif", "report_dvd+rw.aif",
            "report_dvd-rw.aif", "report_enhancedcd.aif", "test_audiocd_cdtext.aif",
            "test_audiocd_multiple_indexes.aif", "test_cdr_incd_finalized.aif", "test_enhancedcd.aif",
            "test_multi_karaoke_sampler.aif", "test_multisession.aif", "test_videocd.aif",
            "Nonstop-UX System V Release 4 B32 (Boot Tape).aif",
            "Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif",
            "Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif",
            "Nonstop-UX System V Release 4 B32 (Operating System).aif",
            "Nonstop-UX System V Release 4 B32 (Optional Packages).aif",
            "Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif",
            "Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif",
            "Nonstop-UX System V Release 4 B32 (Required Packages).aif", "OpenWindows.3.0.exabyte.aif",
            "OpenWindows.3.0.Q150.aif", "OS.MP.4.1C.exabyte.aif", "X.3.0.exabyte.aif", "X.3.Q150.aif"
        };

        readonly ulong[] _sectors =
        {
            // cdiready_the_apprentice.aif
            279300,

            // report_audiocd.aif
            247073,

            // report_cdr.aif
            254265,

            // report_cdrom.aif
            254265,

            // report_cdrw_2x.aif
            308224,

            // report_dvd+r.aif
            2146368,

            // report_dvd-r.aif
            2146368,

            // report_dvd-ram_v1.aif
            1218960,

            // report_dvd-ram_v2.aif
            2236704,

            // report_dvd+r_dl.aif
            16384000,

            // report_dvd-rom.aif
            2146368,

            // report_dvd+rw.aif
            2295104,

            // report_dvd-rw.aif
            2146368,

            // report_enhancedcd.aif
            303316,

            // test_audiocd_cdtext.aif
            277696,

            // test_audiocd_multiple_indexes.aif
            65536,

            // test_cdr_incd_finalized.aif
            350134,

            // test_enhancedcd.aif
            59206,

            // test_multi_karaoke_sampler.aif
            329158,

            // test_multisession.aif
            51168,

            // test_videocd.aif
            48794,

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            1604,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            15485,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            15,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            3298,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            3152,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            818,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            7,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            684,

            // OpenWindows.3.0.exabyte.aif
            73525,

            // OpenWindows.3.0.Q150.aif
            290,

            // OS.MP.4.1C.exabyte.aif
            37587,

            // X.3.0.exabyte.aif
            25046,

            // X.3.Q150.aif
            102
        };

        readonly uint[] _sectorSize =
        {
            // cdiready_the_apprentice.aif
            2352,

            // report_audiocd.aif
            2352,

            // report_cdr.aif
            2048,

            // report_cdrom.aif
            2048,

            // report_cdrw_2x.aif
            2048,

            // report_dvd+r.aif
            2048,

            // report_dvd-r.aif
            2048,

            // report_dvd-ram_v1.aif
            2048,

            // report_dvd-ram_v2.aif
            2048,

            // report_dvd+r_dl.aif
            2048,

            // report_dvd-rom.aif
            2048,

            // report_dvd+rw.aif
            2048,

            // report_dvd-rw.aif
            2048,

            // report_enhancedcd.aif
            2352,

            // test_audiocd_cdtext.aif
            2352,

            // test_audiocd_multiple_indexes.aif
            2352,

            // test_cdr_incd_finalized.aif
            2048,

            // test_enhancedcd.aif
            2352,

            // test_multi_karaoke_sampler.aif
            2352,

            // test_multisession.aif
            2048,

            // test_videocd.aif
            2328,

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            10240,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            512,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            28637,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            32256,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            32256,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            32256,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            26185,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            32256,

            // OpenWindows.3.0.exabyte.aif
            1024,

            // OpenWindows.3.0.Q150.aif
            262144,

            // OS.MP.4.1C.exabyte.aif
            8192,

            // X.3.0.exabyte.aif
            1024,

            // X.3.Q150.aif
            258048
        };

        readonly MediaType[] _mediaTypes =
        {
            // cdiready_the_apprentice.aif
            MediaType.CDIREADY,

            // report_audiocd.aif
            MediaType.CDDA,

            // report_cdr.aif
            MediaType.CDR,

            // report_cdrom.aif
            MediaType.CDROM,

            // report_cdrw_2x.aif
            MediaType.CDRW,

            // report_dvd+r.aif
            MediaType.DVDPR,

            // report_dvd-r.aif
            MediaType.DVDR,

            // report_dvd-ram_v1.aif
            MediaType.DVDRAM,

            // report_dvd-ram_v2.aif
            MediaType.DVDRAM,

            // report_dvd+r_dl.aif
            MediaType.DVDROM,

            // report_dvd-rom.aif
            MediaType.DVDROM,

            // report_dvd+rw.aif
            MediaType.DVDPRW,

            // report_dvd-rw.aif
            MediaType.DVDRWDL,

            // report_enhancedcd.aif
            MediaType.CD,

            // test_audiocd_cdtext.aif
            MediaType.CDR,

            // test_audiocd_multiple_indexes.aif
            MediaType.CDR,

            // test_cdr_incd_finalized.aif
            MediaType.CDR,

            // test_enhancedcd.aif
            MediaType.CDR,

            // test_multi_karaoke_sampler.aif
            MediaType.CD,

            // test_multisession.aif
            MediaType.CDR,

            // test_videocd.aif
            MediaType.CDR,

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            MediaType.UnknownTape,

            // OpenWindows.3.0.exabyte.aif
            MediaType.UnknownTape,

            // OpenWindows.3.0.Q150.aif
            MediaType.UnknownTape,

            // OS.MP.4.1C.exabyte.aif
            MediaType.UnknownTape,

            // X.3.0.exabyte.aif
            MediaType.UnknownTape,

            // X.3.Q150.aif
            MediaType.UnknownTape
        };

        readonly string[] _md5S =
        {
            // cdiready_the_apprentice.aif
            "ad6b898e5f93faf33967fe53fea7037e",

            // report_audiocd.aif
            "c9036cb72bcb67d469ca82eb7a66cb2a",

            // report_cdr.aif
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrom.aif
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw_2x.aif
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // report_dvd+r.aif
            "106f141400355476b499213f36a363f9",

            // report_dvd-r.aif
            "106f141400355476b499213f36a363f9",

            // report_dvd-ram_v1.aif
            "c22b7796791cd4299d74863ed04496c6",

            // report_dvd-ram_v2.aif
            "00b1d7c5e9855959a4d2f6b796aeaf4c",

            // report_dvd+r_dl.aif
            "63d0fd3f25ab503a1818b15ca5eb86b5",

            // report_dvd-rom.aif
            "106f141400355476b499213f36a363f9",

            // report_dvd+rw.aif
            "3c03ab1def372553f1b04afa0fdbc527",

            // report_dvd-rw.aif
            "106f141400355476b499213f36a363f9",

            // report_enhancedcd.aif
            "d10b427d18546a3c8f548edb6d911798",

            // test_audiocd_cdtext.aif
            "78466ec1a08d7804a6cb38f2ed89b10f",

            // test_audiocd_multiple_indexes.aif
            "d5d22e15dcf3f081d562b351611a8991",

            // test_cdr_incd_finalized.aif
            "edc146b00d622f92c6a9bb4648cbea82",

            // test_enhancedcd.aif
            "2fd88f1e8c21601017c937963d8fe5eb",

            // test_multi_karaoke_sampler.aif
            "fef9ff409aa2643ac0c0649e84346f5f",

            // test_multisession.aif
            "099011fe470ce7ca0ecb52368cd2efe5",

            // test_videocd.aif
            "a5531d15eefe70ff21718b3b5da08255",

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            "a6334d975523b3422fea522b0cc118a9",

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            "17ef78d9e5c53b976f530d4ca44223fd",

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            "6b6e80c4b3a48b2bc46571389eeaf78b",

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            "91b6115a718b9854b69478fee8e8644e",

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            "018c37c40f8df91ab9b098d643c9ae6c",

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            "181c9b00c236d14c7dfa4fa009c4559d",

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            "7dc46bb181077d215a5c93cc990da365",

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            "80e1d90052bf8c2df641398d0a30e630",

            // OpenWindows.3.0.exabyte.aif
            "8861f8c06a2e93ca5a81d729ad3e1de1",

            // OpenWindows.3.0.Q150.aif
            "2b944c7a353a63a48fdcf5517306fba6",

            // OS.MP.4.1C.exabyte.aif
            "a923a4fffb3456386bafd00c1d939224",

            // X.3.0.exabyte.aif
            "e625c03d7493dc22fe49f91f731446e8",

            // X.3.Q150.aif
            "198464b1daf8e674debf8eda0fcbf016"
        };

        readonly string[] _longMd5S =
        {
            // cdiready_the_apprentice.aif
            "8c897ff39ce1ae7b091bfd00fbc3c1bb",

            // report_audiocd.aif
            "c9036cb72bcb67d469ca82eb7a66cb2a",

            // report_cdr.aif
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrom.aif
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrw_2x.aif
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // report_dvd+r.aif
            "106f141400355476b499213f36a363f9",

            // report_dvd-r.aif
            "106f141400355476b499213f36a363f9",

            // report_dvd-ram_v1.aif
            "c22b7796791cd4299d74863ed04496c6",

            // report_dvd-ram_v2.aif
            "00b1d7c5e9855959a4d2f6b796aeaf4c",

            // report_dvd+r_dl.aif
            "63d0fd3f25ab503a1818b15ca5eb86b5",

            // report_dvd-rom.aif
            "106f141400355476b499213f36a363f9",

            // report_dvd+rw.aif
            "3c03ab1def372553f1b04afa0fdbc527",

            // report_dvd-rw.aif
            "106f141400355476b499213f36a363f9",

            // report_enhancedcd.aif
            "1c2ff79133d4db028ce415a8b03e70c2",

            // test_audiocd_cdtext.aif
            "78466ec1a08d7804a6cb38f2ed89b10f",

            // test_audiocd_multiple_indexes.aif
            "d5d22e15dcf3f081d562b351611a8991",

            // test_cdr_incd_finalized.aif
            "6b36340c27d5583e73539175eb87c683",

            // test_enhancedcd.aif
            "151e45bd1e949e0416d64eb89f48a55b",

            // test_multi_karaoke_sampler.aif
            "ef18dc4f63ad59c6294ab09da7704366",

            // test_multisession.aif
            "997fa9a35a2c9a6efbbbd55fcc9008f5",

            // test_videocd.aif
            "11a0d9994ee761655ef4d61c6cda99e9",

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            null,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            null,

            // OpenWindows.3.0.exabyte.aif
            null,

            // OpenWindows.3.0.Q150.aif
            null,

            // OS.MP.4.1C.exabyte.aif
            null,

            // X.3.0.exabyte.aif
            null,

            // X.3.Q150.aif
            null
        };

        readonly string[] _subchannelMd5S =
        {
            // cdiready_the_apprentice.aif
            "579e2b502d86bc1eb7d6aded2b752c36",

            // report_audiocd.aif
            "6d2ae02b362918f531ad414c736d349a",

            // report_cdr.aif
            "34b8e75c3038deceaea7d382f22740cb",

            // report_cdrom.aif
            "5d7f79a75e21f56e62d6fc894ee71ee6",

            // report_cdrw_2x.aif
            "80a59aaf861f925a530e1b0d7857fe25",

            // report_dvd+r.aif
            null,

            // report_dvd-r.aif
            null,

            // report_dvd-ram_v1.aif
            null,

            // report_dvd-ram_v2.aif
            null,

            // report_dvd+r_dl.aif
            null,

            // report_dvd-rom.aif
            null,

            // report_dvd+rw.aif
            null,

            // report_dvd-rw.aif
            null,

            // report_enhancedcd.aif
            "f80d8f55069a8815bd03cb2b6d9284b8",

            // test_audiocd_cdtext.aif
            "ac39ed98b7033da6aa936b4314574a2a",

            // test_audiocd_multiple_indexes.aif
            "3546cc3e1b2b3898de5a03083af9d6ee",

            // test_cdr_incd_finalized.aif
            "663da762a5bef780d09217fca9d23e08",

            // test_enhancedcd.aif
            "9b33f13d1dab986e981ba924797f464a",

            // test_multi_karaoke_sampler.aif
            "aa71734f6385319656e2f1a64af5328b",

            // test_multisession.aif
            "0eecfd65daf8a2aa9fea47cf2072350e",

            // test_videocd.aif
            "f49e383ccee2f3cb97aeb82fcb4fdb18",

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            null,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            null,

            // OpenWindows.3.0.exabyte.aif
            null,

            // OpenWindows.3.0.Q150.aif
            null,

            // OS.MP.4.1C.exabyte.aif
            null,

            // X.3.0.exabyte.aif
            null,

            // X.3.Q150.aif
            null
        };

        readonly int[] _tracks =
        {
            // cdiready_the_apprentice.aif
            23,

            // report_audiocd.aif
            14,

            // report_cdr.aif
            1,

            // report_cdrom.aif
            1,

            // report_cdrw_2x.aif
            1,

            // report_dvd+r.aif
            1,

            // report_dvd-r.aif
            1,

            // report_dvd-ram_v1.aif
            1,

            // report_dvd-ram_v2.aif
            1,

            // report_dvd+r_dl.aif
            1,

            // report_dvd-rom.aif
            1,

            // report_dvd+rw.aif
            1,

            // report_dvd-rw.aif
            1,

            // report_enhancedcd.aif
            14,

            // test_audiocd_cdtext.aif
            11,

            // test_audiocd_multiple_indexes.aif
            5,

            // test_cdr_incd_finalized.aif
            1,

            // test_enhancedcd.aif
            3,

            // test_multi_karaoke_sampler.aif
            16,

            // test_multisession.aif
            4,

            // test_videocd.aif
            2,

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            0,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            0,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            0,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            0,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            0,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            0,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            0,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            0,

            // OpenWindows.3.0.exabyte.aif
            0,

            // OpenWindows.3.0.Q150.aif
            0,

            // OS.MP.4.1C.exabyte.aif
            0,

            // X.3.0.exabyte.aif
            0,

            // X.3.Q150.aif
            0
        };

        readonly int[][] _trackSessions =
        {
            // cdiready_the_apprentice.aif
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.aif
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdr.aif
            new[]
            {
                1
            },

            // report_cdrom.aif
            new[]
            {
                1
            },

            // report_cdrw_2x.aif
            new[]
            {
                1
            },

            // report_dvd+r.aif
            new[]
            {
                1
            },

            // report_dvd-r.aif
            new[]
            {
                1
            },

            // report_dvd-ram_v1.aif
            new[]
            {
                1
            },

            // report_dvd-ram_v2.aif
            new[]
            {
                1
            },

            // report_dvd+r_dl.aif
            new[]
            {
                1
            },

            // report_dvd-rom.aif
            new[]
            {
                1
            },

            // report_dvd+rw.aif
            new[]
            {
                1
            },

            // report_dvd-rw.aif
            new[]
            {
                1
            },

            // report_enhancedcd.aif
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // test_audiocd_cdtext.aif
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_audiocd_multiple_indexes.aif
            new[]
            {
                1, 1, 1, 1, 1
            },

            // test_cdr_incd_finalized.aif
            new[]
            {
                1
            },

            // test_enhancedcd.aif
            new[]
            {
                1, 1, 2
            },

            // test_multi_karaoke_sampler.aif
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_multisession.aif
            new[]
            {
                1, 2, 3, 4
            },

            // test_videocd.aif
            new[]
            {
                1, 1
            },

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            null,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            null,

            // OpenWindows.3.0.exabyte.aif
            null,

            // OpenWindows.3.0.Q150.aif
            null,

            // OS.MP.4.1C.exabyte.aif
            null,

            // X.3.0.exabyte.aif
            null,

            // X.3.Q150.aif
            null
        };

        readonly ulong[][] _trackStarts =
        {
            // cdiready_the_apprentice.aif
            new ulong[]
            {
                0, 69150, 88650, 107475, 112050, 133500, 138075, 159675, 164625, 185250, 189975, 208725, 212850, 232050,
                236550, 241725, 255975, 256725, 265500, 267225, 269850, 271500, 274125
            },

            // report_audiocd.aif
            new ulong[]
            {
                0, 16399, 29901, 47800, 63164, 78775, 94582, 116975, 136016, 153922, 170601, 186389, 201649, 224299
            },

            // report_cdr.aif
            new ulong[]
            {
                0
            },

            // report_cdrom.aif
            new ulong[]
            {
                0
            },

            // report_cdrw_2x.aif
            new ulong[]
            {
                0
            },

            // report_dvd+r.aif
            new ulong[]
            {
                0
            },

            // report_dvd-r.aif
            new ulong[]
            {
                0
            },

            // report_dvd-ram_v1.aif
            new ulong[]
            {
                0
            },

            // report_dvd-ram_v2.aif
            new ulong[]
            {
                0
            },

            // report_dvd+r_dl.aif
            new ulong[]
            {
                0
            },

            // report_dvd-rom.aif
            new ulong[]
            {
                0
            },

            // report_dvd+rw.aif
            new ulong[]
            {
                0
            },

            // report_dvd-rw.aif
            new ulong[]
            {
                0
            },

            // report_enhancedcd.aif
            new ulong[]
            {
                0, 15661, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234034
            },

            // test_audiocd_cdtext.aif
            new ulong[]
            {
                0, 29752, 65034, 78426, 95080, 126147, 154959, 191685, 222776, 243438, 269600
            },

            // test_audiocd_multiple_indexes.aif
            new ulong[]
            {
                0, 4653, 13805, 36685, 54989
            },

            // test_cdr_incd_finalized.aif
            new ulong[]
            {
                0
            },

            // test_enhancedcd.aif
            new ulong[]
            {
                0, 14256, 40207
            },

            // test_multi_karaoke_sampler.aif
            new ulong[]
            {
                0, 1737, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            },

            // test_multisession.aif
            new ulong[]
            {
                0, 19387, 32714, 45232
            },

            // test_videocd.aif
            new ulong[]
            {
                0, 1108
            },

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            null,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            null,

            // OpenWindows.3.0.exabyte.aif
            null,

            // OpenWindows.3.0.Q150.aif
            null,

            // OS.MP.4.1C.exabyte.aif
            null,

            // X.3.0.exabyte.aif
            null,

            // X.3.Q150.aif
            null
        };

        readonly ulong[][] _trackEnds =
        {
            // cdiready_the_apprentice.aif
            new ulong[]
            {
                69149, 88649, 107474, 112049, 133499, 138074, 159674, 164624, 185249, 189974, 208724, 212849, 232049,
                236549, 241724, 255974, 256724, 265499, 267224, 269849, 271499, 274124, 279299
            },

            // report_audiocd.aif
            new ulong[]
            {
                16398, 29900, 47799, 63163, 78774, 94581, 116974, 136015, 153921, 170600, 186388, 201648, 224298, 247072
            },

            // report_cdr.aif
            new ulong[]
            {
                254264
            },

            // report_cdrom.aif
            new ulong[]
            {
                254264
            },

            // report_cdrw_2x.aif
            new ulong[]
            {
                308223
            },

            // report_dvd+r.aif
            new ulong[]
            {
                2146367
            },

            // report_dvd-r.aif
            new ulong[]
            {
                2146367
            },

            // report_dvd-ram_v1.aif
            new ulong[]
            {
                1218959
            },

            // report_dvd-ram_v2.aif
            new ulong[]
            {
                2236703
            },

            // report_dvd+r_dl.aif
            new ulong[]
            {
                16383999
            },

            // report_dvd-rom.aif
            new ulong[]
            {
                2146367
            },

            // report_dvd+rw.aif
            new ulong[]
            {
                2295103
            },

            // report_dvd-rw.aif
            new ulong[]
            {
                2146367
            },

            // report_enhancedcd.aif
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 234033,
                303315
            },

            // test_audiocd_cdtext.aif
            new ulong[]
            {
                29751, 65033, 78425, 95079, 126146, 154958, 191684, 222775, 243437, 269599, 277695
            },

            // test_audiocd_multiple_indexes.aif
            new ulong[]
            {
                4652, 13804, 36684, 54988, 65535
            },

            // test_cdr_incd_finalized.aif
            new ulong[]
            {
                350133
            },

            // test_enhancedcd.aif
            new ulong[]
            {
                14255, 40206, 59205
            },

            // test_multi_karaoke_sampler.aif
            new ulong[]
            {
                1736, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329157
            },

            // test_multisession.aif
            new ulong[]
            {
                19386, 32713, 45231, 51167
            },

            // test_videocd.aif
            new ulong[]
            {
                1107, 48793
            },

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            null,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            null,

            // OpenWindows.3.0.exabyte.aif
            null,

            // OpenWindows.3.0.Q150.aif
            null,

            // OS.MP.4.1C.exabyte.aif
            null,

            // X.3.0.exabyte.aif
            null,

            // X.3.Q150.aif
            null
        };

        readonly ulong[][] _trackPregaps =
        {
            // cdiready_the_apprentice.aif
            new ulong[]
            {
                0, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150,
                150, 150
            },

            // report_audiocd.aif
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // report_cdr.aif
            new ulong[]
            {
                150
            },

            // report_cdrom.aif
            new ulong[]
            {
                150
            },

            // report_cdrw_2x.aif
            new ulong[]
            {
                150
            },

            // report_dvd+r.aif
            new ulong[]
            {
                0
            },

            // report_dvd-r.aif
            new ulong[]
            {
                0
            },

            // report_dvd-ram_v1.aif
            new ulong[]
            {
                0
            },

            // report_dvd-ram_v2.aif
            new ulong[]
            {
                0
            },

            // report_dvd+r_dl.aif
            new ulong[]
            {
                0
            },

            // report_dvd-rom.aif
            new ulong[]
            {
                0
            },

            // report_dvd+rw.aif
            new ulong[]
            {
                0
            },

            // report_dvd-rw.aif
            new ulong[]
            {
                0
            },

            // report_enhancedcd.aif
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 146
            },

            // test_audiocd_cdtext.aif
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // test_audiocd_multiple_indexes.aif
            new ulong[]
            {
                150, 151, 70, 4500, 0
            },

            // test_cdr_incd_finalized.aif
            new ulong[]
            {
                150
            },

            // test_enhancedcd.aif
            new ulong[]
            {
                150, 149, 146
            },

            // test_multi_karaoke_sampler.aif
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multisession.aif
            new ulong[]
            {
                150, 146, 146, 146
            },

            // test_videocd.aif
            new ulong[]
            {
                150, 144
            },

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            null,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            null,

            // OpenWindows.3.0.exabyte.aif
            null,

            // OpenWindows.3.0.Q150.aif
            null,

            // OS.MP.4.1C.exabyte.aif
            null,

            // X.3.0.exabyte.aif
            null,

            // X.3.Q150.aif
            null
        };

        readonly byte[][] _trackFlags =
        {
            // cdiready_the_apprentice.aif
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.aif
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdr.aif
            new byte[]
            {
                4
            },

            // report_cdrom.aif
            new byte[]
            {
                4
            },

            // report_cdrw_2x.aif
            new byte[]
            {
                4
            },

            // report_dvd+r.aif
            //            null,
            new byte[]
            {
                0
            },

            // report_dvd-r.aif
            //            null,
            new byte[]
            {
                0
            },

            // report_dvd-ram_v1.aif
            //            null,
            new byte[]
            {
                0
            },

            // report_dvd-ram_v2.aif
            //            null,
            new byte[]
            {
                0
            },

            // report_dvd+r_dl.aif
            //            null,
            new byte[]
            {
                0
            },

            // report_dvd-rom.aif
            //            null,
            new byte[]
            {
                0
            },

            // report_dvd+rw.aif
            //            null,
            new byte[]
            {
                0
            },

            // report_dvd-rw.aif
            //            null,
            new byte[]
            {
                0
            },

            // report_enhancedcd.aif
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // test_audiocd_cdtext.aif
            new byte[]
            {
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // test_audiocd_multiple_indexes.aif
            new byte[]
            {
                2, 0, 0, 8, 1
            },

            // test_cdr_incd_finalized.aif
            new byte[]
            {
                7
            },

            // test_enhancedcd.aif
            new byte[]
            {
                0, 0, 4
            },

            // test_multi_karaoke_sampler.aif
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multisession.aif
            new byte[]
            {
                4, 4, 4, 4
            },

            // test_videocd.aif
            new byte[]
            {
                4, 4
            },

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            null,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            null,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            null,

            // OpenWindows.3.0.exabyte.aif
            null,

            // OpenWindows.3.0.Q150.aif
            null,

            // OS.MP.4.1C.exabyte.aif
            null,

            // X.3.0.exabyte.aif
            null,

            // X.3.Q150.aif
            null
        };

        readonly bool[] _isTape =
        {
            // cdiready_the_apprentice.aif
            false,

            // report_audiocd.aif
            false,

            // report_cdr.aif
            false,

            // report_cdrom.aif
            false,

            // report_cdrw_2x.aif
            false,

            // report_dvd+r.aif
            false,

            // report_dvd-r.aif
            false,

            // report_dvd-ram_v1.aif
            false,

            // report_dvd-ram_v2.aif
            false,

            // report_dvd+r_dl.aif
            false,

            // report_dvd-rom.aif
            false,

            // report_dvd+rw.aif
            false,

            // report_dvd-rw.aif
            false,

            // report_enhancedcd.aif
            false,

            // test_audiocd_cdtext.aif
            false,

            // test_audiocd_multiple_indexes.aif
            false,

            // test_cdr_incd_finalized.aif
            false,

            // test_enhancedcd.aif
            false,

            // test_multi_karaoke_sampler.aif
            false,

            // test_multisession.aif
            false,

            // test_videocd.aif
            false,

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            true,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            true,

            // OpenWindows.3.0.exabyte.aif
            true,

            // OpenWindows.3.0.Q150.aif
            true,

            // OS.MP.4.1C.exabyte.aif
            true,

            // X.3.0.exabyte.aif
            true,

            // X.3.Q150.aif
            true
        };

        readonly TapeFile[][] _tapeFiles =
        {
            // cdiready_the_apprentice.aif
            null,

            // report_audiocd.aif
            null,

            // report_cdr.aif
            null,

            // report_cdrom.aif
            null,

            // report_cdrw_2x.aif
            null,

            // report_dvd+r.aif
            null,

            // report_dvd-r.aif
            null,

            // report_dvd-ram_v1.aif
            null,

            // report_dvd-ram_v2.aif
            null,

            // report_dvd+r_dl.aif
            null,

            // report_dvd-rom.aif
            null,

            // report_dvd+rw.aif
            null,

            // report_dvd-rw.aif
            null,

            // report_enhancedcd.aif
            null,

            // test_audiocd_cdtext.aif
            null,

            // test_audiocd_multiple_indexes.aif
            null,

            // test_cdr_incd_finalized.aif
            null,

            // test_enhancedcd.aif
            null,

            // test_multi_karaoke_sampler.aif
            null,

            // test_multisession.aif
            null,

            // test_videocd.aif
            null,

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 1603,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 15484,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 14,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 3297,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 3151,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 817,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 6,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 683,
                    Partition  = 0
                }
            },

            // OpenWindows.3.0.exabyte.aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 0,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 1,
                    FirstBlock = 1,
                    LastBlock  = 164,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 2,
                    FirstBlock = 165,
                    LastBlock  = 2412,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 3,
                    FirstBlock = 2413,
                    LastBlock  = 5612,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 4,
                    FirstBlock = 5613,
                    LastBlock  = 73524,
                    Partition  = 0
                }
            },

            // OpenWindows.3.0.Q150.aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 0,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 1,
                    FirstBlock = 1,
                    LastBlock  = 1,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 2,
                    FirstBlock = 2,
                    LastBlock  = 10,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 3,
                    FirstBlock = 11,
                    LastBlock  = 23,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 4,
                    FirstBlock = 24,
                    LastBlock  = 289,
                    Partition  = 0
                }
            },

            // OS.MP.4.1C.exabyte.aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 1,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 1,
                    FirstBlock = 2,
                    LastBlock  = 3,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 2,
                    FirstBlock = 4,
                    LastBlock  = 6860,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 3,
                    FirstBlock = 6861,
                    LastBlock  = 13773,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 4,
                    FirstBlock = 13774,
                    LastBlock  = 20263,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 5,
                    FirstBlock = 20264,
                    LastBlock  = 20299,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 6,
                    FirstBlock = 20300,
                    LastBlock  = 22603,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 7,
                    FirstBlock = 22604,
                    LastBlock  = 23472,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 8,
                    FirstBlock = 23473,
                    LastBlock  = 24946,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 9,
                    FirstBlock = 24947,
                    LastBlock  = 26436,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 10,
                    FirstBlock = 26437,
                    LastBlock  = 27720,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 11,
                    FirstBlock = 27721,
                    LastBlock  = 31922,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 12,
                    FirstBlock = 31923,
                    LastBlock  = 32283,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 13,
                    FirstBlock = 32284,
                    LastBlock  = 32675,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 14,
                    FirstBlock = 32676,
                    LastBlock  = 33549,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 15,
                    FirstBlock = 33550,
                    LastBlock  = 33686,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 16,
                    FirstBlock = 33687,
                    LastBlock  = 33909,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 17,
                    FirstBlock = 33910,
                    LastBlock  = 33949,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 18,
                    FirstBlock = 33950,
                    LastBlock  = 34180,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 19,
                    FirstBlock = 34181,
                    LastBlock  = 34573,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 20,
                    FirstBlock = 34574,
                    LastBlock  = 35072,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 21,
                    FirstBlock = 35073,
                    LastBlock  = 35163,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 22,
                    FirstBlock = 35164,
                    LastBlock  = 35908,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 23,
                    FirstBlock = 35909,
                    LastBlock  = 35984,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 24,
                    FirstBlock = 35985,
                    LastBlock  = 36098,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 25,
                    FirstBlock = 36099,
                    LastBlock  = 36270,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 26,
                    FirstBlock = 36271,
                    LastBlock  = 36276,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 27,
                    FirstBlock = 36277,
                    LastBlock  = 36647,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 28,
                    FirstBlock = 36648,
                    LastBlock  = 37111,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 29,
                    FirstBlock = 37112,
                    LastBlock  = 37583,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 30,
                    FirstBlock = 37584,
                    LastBlock  = 37584,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 31,
                    FirstBlock = 37585,
                    LastBlock  = 37585,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 32,
                    FirstBlock = 37586,
                    LastBlock  = 37586,
                    Partition  = 0
                }
            },

            // X.3.0.exabyte.aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 0,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 1,
                    FirstBlock = 1,
                    LastBlock  = 61,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 2,
                    FirstBlock = 62,
                    LastBlock  = 149,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 3,
                    FirstBlock = 150,
                    LastBlock  = 2781,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 4,
                    FirstBlock = 2782,
                    LastBlock  = 11885,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 5,
                    FirstBlock = 11886,
                    LastBlock  = 25045,
                    Partition  = 0
                }
            },

            // X.3.Q150.aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 0,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 1,
                    FirstBlock = 1,
                    LastBlock  = 1,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 2,
                    FirstBlock = 2,
                    LastBlock  = 2,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 3,
                    FirstBlock = 3,
                    LastBlock  = 13,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 4,
                    FirstBlock = 14,
                    LastBlock  = 49,
                    Partition  = 0
                },
                new TapeFile
                {
                    File       = 5,
                    FirstBlock = 50,
                    LastBlock  = 101,
                    Partition  = 0
                }
            }
        };

        readonly TapePartition[][] _tapePartitions =
        {
            // cdiready_the_apprentice.aif
            null,

            // report_audiocd.aif
            null,

            // report_cdr.aif
            null,

            // report_cdrom.aif
            null,

            // report_cdrw_2x.aif
            null,

            // report_dvd+r.aif
            null,

            // report_dvd-r.aif
            null,

            // report_dvd-ram_v1.aif
            null,

            // report_dvd-ram_v2.aif
            null,

            // report_dvd+r_dl.aif
            null,

            // report_dvd-rom.aif
            null,

            // report_dvd+rw.aif
            null,

            // report_dvd-rw.aif
            null,

            // report_enhancedcd.aif
            null,

            // test_audiocd_cdtext.aif
            null,

            // test_audiocd_multiple_indexes.aif
            null,

            // test_cdr_incd_finalized.aif
            null,

            // test_enhancedcd.aif
            null,

            // test_multi_karaoke_sampler.aif
            null,

            // test_multisession.aif
            null,

            // test_videocd.aif
            null,

            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 1603,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 15484,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 14,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 3297,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 3151,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 817,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 6,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 683,
                    Number     = 0
                }
            },

            // OpenWindows.3.0.exabyte.aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 73524,
                    Number     = 0
                }
            },

            // OpenWindows.3.0.Q150.aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 289,
                    Number     = 0
                }
            },

            // OS.MP.4.1C.exabyte.aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 37586,
                    Number     = 0
                }
            },

            // X.3.0.exabyte.aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 25045,
                    Number     = 0
                }
            },

            // X.3.Q150.aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 101,
                    Number     = 0
                }
            }
        };

        [Test]
        public void Test()
        {
            Environment.CurrentDirectory =
                Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "AaruFormat", "V1");

            for(int i = 0; i < _testFiles.Length; i++)
            {
                var filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);

                var  image  = new DiscImages.AaruFormat();
                bool opened = image.Open(filter);

                Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(_sectors[i], image.Info.Sectors, $"Sectors: {_testFiles[i]}");
                        Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, $"Sector size: {_testFiles[i]}");
                        Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, $"Media type: {_testFiles[i]}");

                        if(image.Info.XmlMediaType != XmlMediaType.OpticalDisc)
                            return;

                        Assert.AreEqual(_tracks[i], image.Tracks.Count, $"Tracks: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackSession).Should().
                              BeEquivalentTo(_trackSessions[i], $"Track session: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackStartSector).Should().
                              BeEquivalentTo(_trackStarts[i], $"Track start: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackEndSector).Should().
                              BeEquivalentTo(_trackEnds[i], $"Track end: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackPregap).Should().
                              BeEquivalentTo(_trackPregaps[i], $"Track pregap: {_testFiles[i]}");

                        int trackNo = 0;

                        byte[] flags = new byte[image.Tracks.Count];

                        foreach(Track currentTrack in image.Tracks)
                        {
                            if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                                flags[trackNo] = image.ReadSectorTag(currentTrack.TrackSequence,
                                                                     SectorTagType.CdTrackFlags)[0];

                            trackNo++;
                        }

                        flags.Should().BeEquivalentTo(_trackFlags[i], $"Track flags: {_testFiles[i]}");
                    });
                }
            }
        }

        [Test]
        public void Hashes()
        {
            // How many sectors to read at once
            const uint sectorsToRead = 256;

            Environment.CurrentDirectory =
                Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "AaruFormat", "V1");

            Assert.Multiple(() =>
            {
                Parallel.For(0, _testFiles.Length, (i, state) =>
                {
                    var filter = new ZZZNoFilter();
                    filter.Open(_testFiles[i]);

                    var  image  = new DiscImages.AaruFormat();
                    bool opened = image.Open(filter);

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");
                    Md5Context ctx;

                    if(image.Info.XmlMediaType == XmlMediaType.OpticalDisc)
                    {
                        foreach(bool @long in new[]
                        {
                            false, true
                        })
                        {
                            ctx = new Md5Context();

                            foreach(Track currentTrack in image.Tracks)
                            {
                                ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                                ulong doneSectors = 0;

                                while(doneSectors < sectors)
                                {
                                    byte[] sector;

                                    if(sectors - doneSectors >= sectorsToRead)
                                    {
                                        sector =
                                            @long ? image.ReadSectorsLong(doneSectors, sectorsToRead,
                                                                          currentTrack.TrackSequence)
                                                : image.ReadSectors(doneSectors, sectorsToRead,
                                                                    currentTrack.TrackSequence);

                                        doneSectors += sectorsToRead;
                                    }
                                    else
                                    {
                                        sector =
                                            @long ? image.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
                                                                          currentTrack.TrackSequence)
                                                : image.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                    currentTrack.TrackSequence);

                                        doneSectors += sectors - doneSectors;
                                    }

                                    ctx.Update(sector);
                                }
                            }

                            Assert.AreEqual(@long ? _longMd5S[i] : _md5S[i], ctx.End(),
                                            $"{(@long ? "Long hash" : "Hash")}: {_testFiles[i]}");
                        }

                        if(!image.Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            return;

                        ctx = new Md5Context();

                        foreach(Track currentTrack in image.Tracks)
                        {
                            ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                            ulong doneSectors = 0;

                            while(doneSectors < sectors)
                            {
                                byte[] sector;

                                if(sectors - doneSectors >= sectorsToRead)
                                {
                                    sector = image.ReadSectorsTag(doneSectors, sectorsToRead,
                                                                  currentTrack.TrackSequence,
                                                                  SectorTagType.CdSectorSubchannel);

                                    doneSectors += sectorsToRead;
                                }
                                else
                                {
                                    sector = image.ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors),
                                                                  currentTrack.TrackSequence,
                                                                  SectorTagType.CdSectorSubchannel);

                                    doneSectors += sectors - doneSectors;
                                }

                                ctx.Update(sector);
                            }
                        }

                        Assert.AreEqual(_subchannelMd5S[i], ctx.End(), $"Subchannel hash: {_testFiles[i]}");
                    }
                    else
                    {
                        ctx = new Md5Context();
                        ulong doneSectors = 0;

                        while(doneSectors < image.Info.Sectors)
                        {
                            byte[] sector;

                            if(image.Info.Sectors - doneSectors >= sectorsToRead)
                            {
                                sector      =  image.ReadSectors(doneSectors, sectorsToRead);
                                doneSectors += sectorsToRead;
                            }
                            else
                            {
                                sector      =  image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors));
                                doneSectors += image.Info.Sectors - doneSectors;
                            }

                            ctx.Update(sector);
                        }

                        Assert.AreEqual(_md5S[i], ctx.End(), $"Hash: {_testFiles[i]}");
                    }
                });
            });
        }

        [Test]
        public void Tape()
        {
            Environment.CurrentDirectory =
                Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "AaruFormat", "V1");

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new ZZZNoFilter();
                    filter.Open(_testFiles[i]);

                    ITapeImage image  = new DiscImages.AaruFormat();
                    bool       opened = image.Open(filter);

                    bool foo = image.IsTape;

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");
                    Assert.AreEqual(_isTape[i], image.IsTape, $"Is tape?: {_testFiles[i]}");

                    if(!image.IsTape)
                        continue;

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            image.Files.Should().BeEquivalentTo(_tapeFiles[i], $"Tape files: {_testFiles[i]}");

                            image.TapePartitions.Should().
                                  BeEquivalentTo(_tapePartitions[i], $"Tape files: {_testFiles[i]}");
                        });
                    }
                }
            });
        }
    }
}