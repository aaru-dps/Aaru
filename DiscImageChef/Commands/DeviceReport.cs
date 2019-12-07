// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceReport.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'device-report' verb.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interop;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Database;
using DiscImageChef.Database.Models;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;
using Mono.Options;
using Newtonsoft.Json;
using Command = Mono.Options.Command;
using Device = DiscImageChef.Devices.Device;
using DeviceReport = DiscImageChef.Core.Devices.Report.DeviceReport;

namespace DiscImageChef.Commands
{
    internal class DeviceReportCommand : Command
    {
        string devicePath;

        bool showHelp;

        public DeviceReportCommand() : base("device-report",
                                            "Tests the device capabilities and creates an JSON report of them.") =>
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}", "", $"usage: DiscImageChef {Name} devicepath", "",
                Help,
                {
                    "help|h|?", "Show this message and exit.", v => showHelp = v != null
                }
            };

        public override int Invoke(IEnumerable<string> arguments)
        {
            List<string> extra = Options.Parse(arguments);

            if(showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);

                return(int)ErrorNumber.HelpRequested;
            }

            MainClass.PrintCopyright();

            if(MainClass.Debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(MainClass.Verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("device-report");

            if(extra.Count > 1)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");

                return(int)ErrorNumber.UnexpectedArgumentCount;
            }

            if(extra.Count == 0)
            {
                DicConsole.ErrorWriteLine("Missing device path.");

                return(int)ErrorNumber.MissingArgument;
            }

            devicePath = extra[0];

            DicConsole.DebugWriteLine("Device-Report command", "--debug={0}", MainClass.Debug);
            DicConsole.DebugWriteLine("Device-Report command", "--device={0}", devicePath);
            DicConsole.DebugWriteLine("Device-Report command", "--verbose={0}", MainClass.Verbose);

            if(devicePath.Length == 2   &&
               devicePath[1]     == ':' &&
               devicePath[0]     != '/' &&
               char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Device dev;

            try
            {
                dev = new Device(devicePath);

                if(dev.IsRemote)
                    Statistics.AddRemote(dev.RemoteApplication, dev.RemoteVersion, dev.RemoteOperatingSystem,
                                         dev.RemoteOperatingSystemVersion, dev.RemoteArchitecture);

                if(dev.Error)
                {
                    DicConsole.ErrorWriteLine(Error.Print(dev.LastError));

                    return(int)ErrorNumber.CannotOpenDevice;
                }
            }
            catch(DeviceException e)
            {
                DicConsole.ErrorWriteLine(e.Message ?? Error.Print(e.LastError));

                return(int)ErrorNumber.CannotOpenDevice;
            }

            Statistics.AddDevice(dev);

            bool isAdmin;

            if(dev.IsRemote)
                isAdmin = dev.IsRemoteAdmin;
            else
                isAdmin = DetectOS.IsAdmin;

            if(!isAdmin)
            {
                DicConsole.
                    ErrorWriteLine("Because of the commands sent to a device, device report must be run with administrative privileges.");

                DicConsole.ErrorWriteLine("Not continuing.");

                return(int)ErrorNumber.NotEnoughPermissions;
            }

            var report = new DeviceReportV2
            {
                Manufacturer = dev.Manufacturer, Model = dev.Model, Revision = dev.Revision, Type = dev.Type
            };

            bool   removable = false;
            string jsonFile;

            if(!string.IsNullOrWhiteSpace(dev.Manufacturer) &&
               !string.IsNullOrWhiteSpace(dev.Revision))
                jsonFile = dev.Manufacturer + "_" + dev.Model + "_" + dev.Revision + ".json";
            else if(!string.IsNullOrWhiteSpace(dev.Manufacturer))
                jsonFile = dev.Manufacturer + "_" + dev.Model + ".json";
            else if(!string.IsNullOrWhiteSpace(dev.Revision))
                jsonFile = dev.Model + "_" + dev.Revision + ".json";
            else
                jsonFile = dev.Model + ".json";

            jsonFile = jsonFile.Replace('\\', '_').Replace('/', '_').Replace('?', '_');

            var reporter = new DeviceReport(dev, MainClass.Debug);

            ConsoleKeyInfo pressedKey;

            if(dev.IsUsb)
            {
                pressedKey = new ConsoleKeyInfo();

                while(pressedKey.Key != ConsoleKey.Y &&
                      pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the device natively USB (in case of doubt, press Y)? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                if(pressedKey.Key == ConsoleKey.Y)
                {
                    report.USB = reporter.UsbReport();

                    pressedKey = new ConsoleKeyInfo();

                    while(pressedKey.Key != ConsoleKey.Y &&
                          pressedKey.Key != ConsoleKey.N)
                    {
                        DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                        pressedKey = System.Console.ReadKey();
                        DicConsole.WriteLine();
                    }

                    report.USB.RemovableMedia = pressedKey.Key == ConsoleKey.Y;
                    removable                 = report.USB.RemovableMedia;
                }
            }

            if(dev.IsFireWire)
            {
                pressedKey = new ConsoleKeyInfo();

                while(pressedKey.Key != ConsoleKey.Y &&
                      pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the device natively FireWire (in case of doubt, press Y)? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                if(pressedKey.Key != ConsoleKey.Y)
                {
                    report.FireWire = reporter.FireWireReport();

                    pressedKey = new ConsoleKeyInfo();

                    while(pressedKey.Key != ConsoleKey.Y &&
                          pressedKey.Key != ConsoleKey.N)
                    {
                        DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                        pressedKey = System.Console.ReadKey();
                        DicConsole.WriteLine();
                    }

                    report.FireWire.RemovableMedia = pressedKey.Key == ConsoleKey.Y;
                    removable                      = report.FireWire.RemovableMedia;
                }
            }

            if(dev.IsPcmcia)
                report.PCMCIA = reporter.PcmciaReport();

            byte[] buffer;
            string mediumTypeName;
            string mediumModel;

            switch(dev.Type)
            {
                case DeviceType.ATA:
                {
                    DicConsole.WriteLine("Querying ATA IDENTIFY...");

                    dev.AtaIdentify(out buffer, out _, dev.Timeout, out _);

                    if(!Identify.Decode(buffer).HasValue)
                        break;

                    report.ATA = new Ata
                    {
                        Identify = DeviceReport.ClearIdentify(buffer)
                    };

                    if(report.ATA.IdentifyDevice == null)
                        break;

                    if((ushort)report.ATA.IdentifyDevice?.GeneralConfiguration == 0x848A)
                    {
                        report.CompactFlash = true;
                        removable           = false;
                    }
                    else if(!removable &&
                            report.ATA.IdentifyDevice?.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.
                                                                                             Removable) == true)
                    {
                        pressedKey = new ConsoleKeyInfo();

                        while(pressedKey.Key != ConsoleKey.Y &&
                              pressedKey.Key != ConsoleKey.N)
                        {
                            DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                            pressedKey = System.Console.ReadKey();
                            DicConsole.WriteLine();
                        }

                        removable = pressedKey.Key == ConsoleKey.Y;
                    }

                    if(removable)
                    {
                        DicConsole.
                            WriteLine("Please remove any media from the device and press any key when it is out.");

                        System.Console.ReadKey(true);
                        DicConsole.WriteLine("Querying ATA IDENTIFY...");
                        dev.AtaIdentify(out buffer, out _, dev.Timeout, out _);
                        report.ATA.Identify = DeviceReport.ClearIdentify(buffer);
                        List<TestedMedia> mediaTests = new List<TestedMedia>();

                        pressedKey = new ConsoleKeyInfo();

                        while(pressedKey.Key != ConsoleKey.N)
                        {
                            pressedKey = new ConsoleKeyInfo();

                            while(pressedKey.Key != ConsoleKey.Y &&
                                  pressedKey.Key != ConsoleKey.N)
                            {
                                DicConsole.Write("Do you have media that you can insert in the drive? (Y/N): ");
                                pressedKey = System.Console.ReadKey();
                                DicConsole.WriteLine();
                            }

                            if(pressedKey.Key != ConsoleKey.Y)
                                continue;

                            DicConsole.WriteLine("Please insert it in the drive and press any key when it is ready.");
                            System.Console.ReadKey(true);

                            DicConsole.Write("Please write a description of the media type and press enter: ");
                            mediumTypeName = System.Console.ReadLine();
                            DicConsole.Write("Please write the media model and press enter: ");
                            mediumModel = System.Console.ReadLine();

                            TestedMedia mediaTest = reporter.ReportAtaMedia();
                            mediaTest.MediumTypeName = mediumTypeName;
                            mediaTest.Model          = mediumModel;

                            mediaTests.Add(mediaTest);
                        }

                        report.ATA.RemovableMedias = mediaTests;
                    }
                    else
                    {
                        report.ATA.ReadCapabilities = reporter.ReportAta(report.ATA.IdentifyDevice.Value);
                    }

                    break;
                }

                case DeviceType.MMC:
                    report.MultiMediaCard = reporter.MmcSdReport();

                    break;
                case DeviceType.SecureDigital:
                    report.SecureDigital = reporter.MmcSdReport();

                    break;
                case DeviceType.NVMe: throw new NotImplementedException("NVMe devices not yet supported.");
                case DeviceType.ATAPI:
                    DicConsole.WriteLine("Querying ATAPI IDENTIFY...");

                    dev.AtapiIdentify(out buffer, out _, dev.Timeout, out _);

                    if(Identify.Decode(buffer).HasValue)
                        report.ATAPI = new Ata
                        {
                            Identify = DeviceReport.ClearIdentify(buffer)
                        };

                    goto case DeviceType.SCSI;
                case DeviceType.SCSI:
                    if(!dev.IsUsb      &&
                       !dev.IsFireWire &&
                       dev.IsRemovable)
                    {
                        pressedKey = new ConsoleKeyInfo();

                        while(pressedKey.Key != ConsoleKey.Y &&
                              pressedKey.Key != ConsoleKey.N)
                        {
                            DicConsole.
                                Write("Is the media removable from the reading/writing elements (flash memories ARE NOT removable)? (Y/N): ");

                            pressedKey = System.Console.ReadKey();
                            DicConsole.WriteLine();
                        }

                        removable = pressedKey.Key == ConsoleKey.Y;
                    }

                    if(removable)
                    {
                        switch(dev.ScsiType)
                        {
                            case PeripheralDeviceTypes.MultiMediaDevice:
                                dev.AllowMediumRemoval(out buffer, dev.Timeout, out _);
                                dev.EjectTray(out buffer, dev.Timeout, out _);

                                break;
                            case PeripheralDeviceTypes.SequentialAccess:
                                dev.SpcAllowMediumRemoval(out buffer, dev.Timeout, out _);
                                DicConsole.WriteLine("Asking drive to unload tape (can take a few minutes)...");
                                dev.Unload(out buffer, dev.Timeout, out _);

                                break;
                        }

                        DicConsole.
                            WriteLine("Please remove any media from the device and press any key when it is out.");

                        System.Console.ReadKey(true);
                    }

                    report.SCSI = reporter.ReportScsiInquiry();

                    if(report.SCSI == null)
                        break;

                    report.SCSI.EVPDPages =
                        reporter.ReportEvpdPages(StringHandlers.
                                                 CToString(report.SCSI.Inquiry?.VendorIdentification)?.Trim().
                                                 ToLowerInvariant());

                    reporter.ReportScsiModes(ref report, out byte[] cdromMode);

                    string mediumManufacturer;
                    byte[] senseBuffer;
                    bool   sense;

                    switch(dev.ScsiType)
                    {
                        case PeripheralDeviceTypes.MultiMediaDevice:
                        {
                            bool iomegaRev = dev.Manufacturer.ToLowerInvariant() == "iomega" &&
                                             dev.Model.ToLowerInvariant().StartsWith("rrd");

                            List<string> mediaTypes = new List<string>();

                            report.SCSI.MultiMediaDevice = new Mmc
                            {
                                ModeSense2AData = cdromMode, Features = reporter.ReportMmcFeatures()
                            };

                            if(cdromMode != null &&
                               !iomegaRev)
                            {
                                mediaTypes.Add("CD-ROM");
                                mediaTypes.Add("Audio CD");
                                mediaTypes.Add("Enhanced CD (aka E-CD, CD-Plus or CD+)");

                                if(report.SCSI.MultiMediaDevice.ModeSense2A.ReadCDR)
                                    mediaTypes.Add("CD-R");

                                if(report.SCSI.MultiMediaDevice.ModeSense2A.ReadCDRW)
                                {
                                    mediaTypes.Add("CD-RW Ultra Speed (marked 16x or higher)");
                                    mediaTypes.Add("CD-RW High Speed (marked between 8x and 12x)");
                                    mediaTypes.Add("CD-RW (marked 4x or lower)");
                                }

                                if(report.SCSI.MultiMediaDevice.ModeSense2A.ReadDVDROM)
                                    mediaTypes.Add("DVD-ROM");

                                if(report.SCSI.MultiMediaDevice.ModeSense2A.ReadDVDRAM)
                                {
                                    mediaTypes.Add("DVD-RAM (1st gen, marked 2.6Gb or 5.2Gb)");
                                    mediaTypes.Add("DVD-RAM (2nd gen, marked 4.7Gb or 9.4Gb)");
                                }

                                if(report.SCSI.MultiMediaDevice.ModeSense2A.ReadDVDR)
                                    mediaTypes.Add("DVD-R");
                            }

                            if(report.SCSI.MultiMediaDevice.Features != null &&
                               !iomegaRev)
                            {
                                if(report.SCSI.MultiMediaDevice.Features.CanReadBD      ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadBDR     ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadBDRE1   ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadBDRE2   ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadBDROM   ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadOldBDR  ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadOldBDRE ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadOldBDROM)
                                {
                                    if(!mediaTypes.Contains("BD-ROM"))
                                        mediaTypes.Add("BD-ROM");

                                    if(!mediaTypes.Contains("BD-R HTL (not LTH)"))
                                        mediaTypes.Add("BD-R HTL (not LTH)");

                                    if(!mediaTypes.Contains("BD-RE"))
                                        mediaTypes.Add("BD-RE");

                                    if(!mediaTypes.Contains("BD-R LTH"))
                                        mediaTypes.Add("BD-R LTH");

                                    if(!mediaTypes.Contains("BD-R Triple Layer (100Gb)"))
                                        mediaTypes.Add("BD-R Triple Layer (100Gb)");

                                    if(!mediaTypes.Contains("BD-R Quad Layer (128Gb)"))
                                        mediaTypes.Add("BD-R Quad Layer (128Gb)");

                                    if(!mediaTypes.Contains("Ultra HD Blu-ray movie"))
                                        mediaTypes.Add("Ultra HD Blu-ray movie");

                                    if(!mediaTypes.Contains("PlayStation 3 game"))
                                        mediaTypes.Add("PlayStation 3 game");

                                    if(!mediaTypes.Contains("PlayStation 4 game"))
                                        mediaTypes.Add("PlayStation 4 game");

                                    if(!mediaTypes.Contains("Xbox One game"))
                                        mediaTypes.Add("Xbox One game");

                                    if(!mediaTypes.Contains("Nintendo Wii U game"))
                                        mediaTypes.Add("Nintendo Wii U game");
                                }

                                if(report.SCSI.MultiMediaDevice.Features.CanReadCD ||
                                   report.SCSI.MultiMediaDevice.Features.MultiRead)
                                {
                                    if(!mediaTypes.Contains("CD-ROM"))
                                        mediaTypes.Add("CD-ROM");

                                    if(!mediaTypes.Contains("Audio CD"))
                                        mediaTypes.Add("Audio CD");

                                    if(!mediaTypes.Contains("Enhanced CD (aka E-CD, CD-Plus or CD+)"))
                                        mediaTypes.Add("Enhanced CD (aka E-CD, CD-Plus or CD+)");

                                    if(!mediaTypes.Contains("CD-R"))
                                        mediaTypes.Add("CD-R");

                                    if(!mediaTypes.Contains("CD-RW Ultra Speed (marked 16x or higher)"))
                                        mediaTypes.Add("CD-RW Ultra Speed (marked 16x or higher)");

                                    if(!mediaTypes.Contains("CD-RW High Speed (marked between 8x and 12x)"))
                                        mediaTypes.Add("CD-RW High Speed (marked between 8x and 12x)");

                                    if(!mediaTypes.Contains("CD-RW (marked 4x or lower)"))
                                        mediaTypes.Add("CD-RW (marked 4x or lower)");
                                }

                                if(report.SCSI.MultiMediaDevice.Features.CanReadCDMRW)
                                    if(!mediaTypes.Contains("CD-MRW"))
                                        mediaTypes.Add("CD-MRW");

                                if(report.SCSI.MultiMediaDevice.Features.CanReadDDCD)
                                {
                                    if(!mediaTypes.Contains("DDCD-ROM"))
                                        mediaTypes.Add("DDCD-ROM");

                                    if(!mediaTypes.Contains("DDCD-R"))
                                        mediaTypes.Add("DDCD-R");

                                    if(!mediaTypes.Contains("DDCD-RW"))
                                        mediaTypes.Add("DDCD-RW");
                                }

                                if(report.SCSI.MultiMediaDevice.Features.CanReadDVD        ||
                                   report.SCSI.MultiMediaDevice.Features.DVDMultiRead      ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusR   ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRDL ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRW  ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRWDL)
                                {
                                    if(!mediaTypes.Contains("DVD-ROM"))
                                        mediaTypes.Add("DVD-ROM");

                                    if(!mediaTypes.Contains("DVD-R"))
                                        mediaTypes.Add("DVD-R");

                                    if(!mediaTypes.Contains("DVD-RW"))
                                        mediaTypes.Add("DVD-RW");

                                    if(!mediaTypes.Contains("DVD+R"))
                                        mediaTypes.Add("DVD+R");

                                    if(!mediaTypes.Contains("DVD+RW"))
                                        mediaTypes.Add("DVD+RW");

                                    if(!mediaTypes.Contains("DVD-R DL"))
                                        mediaTypes.Add("DVD-R DL");

                                    if(!mediaTypes.Contains("DVD+R DL"))
                                        mediaTypes.Add("DVD+R DL");

                                    if(!mediaTypes.Contains("Nintendo GameCube game"))
                                        mediaTypes.Add("Nintendo GameCube game");

                                    if(!mediaTypes.Contains("Nintendo Wii game"))
                                        mediaTypes.Add("Nintendo Wii game");
                                }

                                if(report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusMRW)
                                    if(!mediaTypes.Contains("DVD+MRW"))
                                        mediaTypes.Add("DVD+MRW");

                                if(report.SCSI.MultiMediaDevice.Features.CanReadHDDVD ||
                                   report.SCSI.MultiMediaDevice.Features.CanReadHDDVDR)
                                {
                                    if(!mediaTypes.Contains("HD DVD-ROM"))
                                        mediaTypes.Add("HD DVD-ROM");

                                    if(!mediaTypes.Contains("HD DVD-R"))
                                        mediaTypes.Add("HD DVD-R");

                                    if(!mediaTypes.Contains("HD DVD-RW"))
                                        mediaTypes.Add("HD DVD-RW");
                                }

                                if(report.SCSI.MultiMediaDevice.Features.CanReadHDDVDRAM)
                                    if(!mediaTypes.Contains("HD DVD-RAM"))
                                        mediaTypes.Add("HD DVD-RAM");
                            }

                            if(iomegaRev)
                            {
                                mediaTypes.Add("REV 35Gb");
                                mediaTypes.Add("REV 70Gb");
                                mediaTypes.Add("REV 120Gb");
                            }

                            // Very old CD drives do not contain mode page 2Ah neither GET CONFIGURATION, so just try all CDs on them
                            // Also don't get confident, some drives didn't know CD-RW but are able to read them
                            if(mediaTypes.Count == 0 ||
                               mediaTypes.Contains("CD-ROM"))
                            {
                                if(!mediaTypes.Contains("CD-ROM"))
                                    mediaTypes.Add("CD-ROM");

                                if(!mediaTypes.Contains("Audio CD"))
                                    mediaTypes.Add("Audio CD");

                                if(!mediaTypes.Contains("CD-R"))
                                    mediaTypes.Add("CD-R");

                                if(!mediaTypes.Contains("CD-RW Ultra Speed (marked 16x or higher)"))
                                    mediaTypes.Add("CD-RW Ultra Speed (marked 16x or higher)");

                                if(!mediaTypes.Contains("CD-RW High Speed (marked between 8x and 12x)"))
                                    mediaTypes.Add("CD-RW High Speed (marked between 8x and 12x)");

                                if(!mediaTypes.Contains("CD-RW (marked 4x or lower)"))
                                    mediaTypes.Add("CD-RW (marked 4x or lower)");

                                if(!mediaTypes.Contains("Enhanced CD (aka E-CD, CD-Plus or CD+)"))
                                    mediaTypes.Add("Enhanced CD (aka E-CD, CD-Plus or CD+)");
                            }

                            mediaTypes.Sort();

                            bool tryPlextor = false, tryHldtst = false, tryPioneer = false, tryNec = false;

                            tryPlextor |= dev.Manufacturer.ToLowerInvariant() == "plextor";
                            tryHldtst  |= dev.Manufacturer.ToLowerInvariant() == "hl-dt-st";
                            tryPioneer |= dev.Manufacturer.ToLowerInvariant() == "pioneer";
                            tryNec     |= dev.Manufacturer.ToLowerInvariant() == "nec";

                            if(MainClass.Debug &&
                               !iomegaRev)
                            {
                                if(!tryPlextor)
                                {
                                    pressedKey = new ConsoleKeyInfo();

                                    while(pressedKey.Key != ConsoleKey.Y &&
                                          pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole.
                                            Write("Do you have want to try Plextor vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");

                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    tryPlextor |= pressedKey.Key == ConsoleKey.Y;
                                }

                                if(!tryNec)
                                {
                                    pressedKey = new ConsoleKeyInfo();

                                    while(pressedKey.Key != ConsoleKey.Y &&
                                          pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole.
                                            Write("Do you have want to try NEC vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");

                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    tryNec |= pressedKey.Key == ConsoleKey.Y;
                                }

                                if(!tryPioneer)
                                {
                                    pressedKey = new ConsoleKeyInfo();

                                    while(pressedKey.Key != ConsoleKey.Y &&
                                          pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole.
                                            Write("Do you have want to try Pioneer vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");

                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    tryPioneer |= pressedKey.Key == ConsoleKey.Y;
                                }

                                if(!tryHldtst)
                                {
                                    pressedKey = new ConsoleKeyInfo();

                                    while(pressedKey.Key != ConsoleKey.Y &&
                                          pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole.
                                            Write("Do you have want to try HL-DT-ST (aka LG) vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");

                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    tryHldtst |= pressedKey.Key == ConsoleKey.Y;
                                }
                            }

                            if(dev.Model.StartsWith("PD-", StringComparison.Ordinal))
                                mediaTypes.Add("PD-650");

                            List<TestedMedia> mediaTests = new List<TestedMedia>();

                            foreach(string mediaType in mediaTypes)
                            {
                                pressedKey = new ConsoleKeyInfo();

                                while(pressedKey.Key != ConsoleKey.Y &&
                                      pressedKey.Key != ConsoleKey.N)
                                {
                                    DicConsole.Write("Do you have a {0} disc that you can insert in the drive? (Y/N): ",
                                                     mediaType);

                                    pressedKey = System.Console.ReadKey();
                                    DicConsole.WriteLine();
                                }

                                if(pressedKey.Key != ConsoleKey.Y)
                                    continue;

                                DicConsole.
                                    WriteLine("Please insert it in the drive and press any key when it is ready.");

                                System.Console.ReadKey(true);

                                bool mediaIsRecognized = true;

                                sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                if(sense)
                                {
                                    FixedSense? decSense = Sense.DecodeFixed(senseBuffer);

                                    if(decSense.HasValue)
                                    {
                                        if(decSense.Value.ASC == 0x3A)
                                        {
                                            int leftRetries = 50;

                                            while(leftRetries > 0)
                                            {
                                                DicConsole.Write("\rWaiting for drive to become ready");
                                                Thread.Sleep(2000);
                                                sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                if(!sense)
                                                    break;

                                                leftRetries--;
                                            }

                                            mediaIsRecognized &= !sense;
                                        }
                                        else if(decSense.Value.ASC  == 0x04 &&
                                                decSense.Value.ASCQ == 0x01)
                                        {
                                            int leftRetries = 50;

                                            while(leftRetries > 0)
                                            {
                                                DicConsole.Write("\rWaiting for drive to become ready");
                                                Thread.Sleep(2000);
                                                sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                if(!sense)
                                                    break;

                                                leftRetries--;
                                            }

                                            mediaIsRecognized &= !sense;
                                        }

                                        // These should be trapped by the OS but seems in some cases they're not
                                        else if(decSense.Value.ASC == 0x28)
                                        {
                                            int leftRetries = 50;

                                            while(leftRetries > 0)
                                            {
                                                DicConsole.Write("\rWaiting for drive to become ready");
                                                Thread.Sleep(2000);
                                                sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                if(!sense)
                                                    break;

                                                leftRetries--;
                                            }

                                            mediaIsRecognized &= !sense;
                                        }
                                        else
                                        {
                                            DicConsole.DebugWriteLine("Device-Report command",
                                                                      "Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                                      decSense.Value.SenseKey, decSense.Value.ASC,
                                                                      decSense.Value.ASCQ);

                                            mediaIsRecognized = false;
                                        }
                                    }
                                    else
                                    {
                                        DicConsole.DebugWriteLine("Device-Report command",
                                                                  "Got sense status but no sense buffer");

                                        mediaIsRecognized = false;
                                    }
                                }

                                var mediaTest = new TestedMedia();

                                if(mediaIsRecognized)
                                {
                                    mediaTest = reporter.ReportMmcMedia(mediaType, tryPlextor, tryPioneer, tryNec,
                                                                        tryHldtst);

                                    if(mediaTest is null)
                                        continue;

                                    if(mediaTest.SupportsReadLong == true &&
                                       mediaTest.LongBlockSize    == mediaTest.BlockSize)
                                    {
                                        pressedKey = new ConsoleKeyInfo();

                                        while(pressedKey.Key != ConsoleKey.Y &&
                                              pressedKey.Key != ConsoleKey.N)
                                        {
                                            DicConsole.
                                                Write("Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours) (Y/N): ");

                                            pressedKey = System.Console.ReadKey();
                                            DicConsole.WriteLine();
                                        }

                                        if(pressedKey.Key == ConsoleKey.Y)
                                        {
                                            for(ushort i = (ushort)mediaTest.BlockSize;; i++)
                                            {
                                                DicConsole.Write("\rTrying to READ LONG with a size of {0} bytes...",
                                                                 i);

                                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, i,
                                                                       dev.Timeout, out _);

                                                if(!sense)
                                                {
                                                    if(MainClass.Debug)
                                                        mediaTest.ReadLong10Data = buffer;

                                                    mediaTest.LongBlockSize = i;

                                                    break;
                                                }

                                                if(i == ushort.MaxValue)
                                                    break;
                                            }

                                            DicConsole.WriteLine();
                                        }
                                    }

                                    if(MainClass.Debug                    &&
                                       mediaTest.SupportsReadLong == true &&
                                       mediaTest.LongBlockSize    != mediaTest.BlockSize)
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                               (ushort)mediaTest.LongBlockSize, dev.Timeout, out _);

                                        if(!sense)
                                            mediaTest.ReadLong10Data = buffer;
                                    }

                                    // TODO: READ LONG (16)
                                }

                                mediaTest.MediumTypeName    = mediaType;
                                mediaTest.MediaIsRecognized = mediaIsRecognized;
                                mediaTests.Add(mediaTest);

                                dev.AllowMediumRemoval(out buffer, dev.Timeout, out _);
                                dev.EjectTray(out buffer, dev.Timeout, out _);
                            }

                            report.SCSI.MultiMediaDevice.TestedMedia = mediaTests;
                        }

                            break;
                        case PeripheralDeviceTypes.SequentialAccess:
                        {
                            report.SCSI.SequentialDevice = reporter.ReportScsiSsc();

                            List<TestedSequentialMedia> seqTests = new List<TestedSequentialMedia>();

                            pressedKey = new ConsoleKeyInfo();

                            while(pressedKey.Key != ConsoleKey.N)
                            {
                                pressedKey = new ConsoleKeyInfo();

                                while(pressedKey.Key != ConsoleKey.Y &&
                                      pressedKey.Key != ConsoleKey.N)
                                {
                                    DicConsole.Write("Do you have media that you can insert in the drive? (Y/N): ");
                                    pressedKey = System.Console.ReadKey();
                                    DicConsole.WriteLine();
                                }

                                if(pressedKey.Key != ConsoleKey.Y)
                                    continue;

                                DicConsole.
                                    WriteLine("Please insert it in the drive and press any key when it is ready.");

                                System.Console.ReadKey(true);

                                DicConsole.Write("Please write a description of the media type and press enter: ");
                                mediumTypeName = System.Console.ReadLine();
                                DicConsole.Write("Please write the media manufacturer and press enter: ");
                                mediumManufacturer = System.Console.ReadLine();
                                DicConsole.Write("Please write the media model and press enter: ");
                                mediumModel = System.Console.ReadLine();

                                bool mediaIsRecognized = true;

                                sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);
                                DicConsole.DebugWriteLine("Device reporting", "sense = {0}", sense);

                                if(sense)
                                {
                                    FixedSense? decSense = Sense.DecodeFixed(senseBuffer);

                                    if(decSense.HasValue)
                                    {
                                        if(decSense.Value.ASC == 0x3A)
                                        {
                                            int leftRetries = 50;

                                            while(leftRetries > 0)
                                            {
                                                DicConsole.Write("\rWaiting for drive to become ready");
                                                Thread.Sleep(2000);
                                                sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                if(!sense)
                                                    break;

                                                leftRetries--;
                                            }

                                            mediaIsRecognized &= !sense;
                                        }
                                        else if(decSense.Value.ASC  == 0x04 &&
                                                decSense.Value.ASCQ == 0x01)
                                        {
                                            int leftRetries = 50;

                                            while(leftRetries > 0)
                                            {
                                                DicConsole.Write("\rWaiting for drive to become ready");
                                                Thread.Sleep(2000);
                                                sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                if(!sense)
                                                    break;

                                                leftRetries--;
                                            }

                                            mediaIsRecognized &= !sense;
                                        }

                                        // These should be trapped by the OS but seems in some cases they're not
                                        else if(decSense.Value.ASC == 0x28)
                                        {
                                            int leftRetries = 50;

                                            while(leftRetries > 0)
                                            {
                                                DicConsole.Write("\rWaiting for drive to become ready");
                                                Thread.Sleep(2000);
                                                sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                if(!sense)
                                                    break;

                                                leftRetries--;
                                            }

                                            mediaIsRecognized &= !sense;
                                        }
                                        else
                                        {
                                            DicConsole.DebugWriteLine("Device-Report command",
                                                                      "Device not ready. Sense {0} ASC {1:X2}h ASCQ {2:X2}h",
                                                                      decSense.Value.SenseKey, decSense.Value.ASC,
                                                                      decSense.Value.ASCQ);

                                            mediaIsRecognized = false;
                                        }
                                    }
                                    else
                                    {
                                        DicConsole.DebugWriteLine("Device-Report command",
                                                                  "Got sense status but no sense buffer");

                                        mediaIsRecognized = false;
                                    }
                                }

                                var seqTest = new TestedSequentialMedia();

                                if(mediaIsRecognized)
                                    seqTest = reporter.ReportSscMedia();

                                seqTest.MediumTypeName    = mediumTypeName;
                                seqTest.Manufacturer      = mediumManufacturer;
                                seqTest.Model             = mediumModel;
                                seqTest.MediaIsRecognized = mediaIsRecognized;

                                seqTests.Add(seqTest);

                                dev.SpcAllowMediumRemoval(out buffer, dev.Timeout, out _);
                                DicConsole.WriteLine("Asking drive to unload tape (can take a few minutes)...");
                                dev.Unload(out buffer, dev.Timeout, out _);
                            }

                            report.SCSI.SequentialDevice.TestedMedia = seqTests;
                        }

                            break;
                        default:
                        {
                            if(removable)
                            {
                                List<TestedMedia> mediaTests = new List<TestedMedia>();

                                pressedKey = new ConsoleKeyInfo();

                                while(pressedKey.Key != ConsoleKey.N)
                                {
                                    pressedKey = new ConsoleKeyInfo();

                                    while(pressedKey.Key != ConsoleKey.Y &&
                                          pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole.Write("Do you have media that you can insert in the drive? (Y/N): ");
                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    if(pressedKey.Key != ConsoleKey.Y)
                                        continue;

                                    DicConsole.
                                        WriteLine("Please insert it in the drive and press any key when it is ready.");

                                    System.Console.ReadKey(true);

                                    DicConsole.Write("Please write a description of the media type and press enter: ");
                                    mediumTypeName = System.Console.ReadLine();
                                    DicConsole.Write("Please write the media manufacturer and press enter: ");
                                    mediumManufacturer = System.Console.ReadLine();
                                    DicConsole.Write("Please write the media model and press enter: ");
                                    mediumModel = System.Console.ReadLine();

                                    bool mediaIsRecognized = true;

                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                    if(sense)
                                    {
                                        FixedSense? decSense = Sense.DecodeFixed(senseBuffer);

                                        if(decSense.HasValue)
                                            if(decSense.Value.ASC == 0x3A)
                                            {
                                                int leftRetries = 20;

                                                while(leftRetries > 0)
                                                {
                                                    DicConsole.Write("\rWaiting for drive to become ready");
                                                    Thread.Sleep(2000);
                                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                    if(!sense)
                                                        break;

                                                    leftRetries--;
                                                }

                                                mediaIsRecognized &= !sense;
                                            }
                                            else if(decSense.Value.ASC  == 0x04 &&
                                                    decSense.Value.ASCQ == 0x01)
                                            {
                                                int leftRetries = 20;

                                                while(leftRetries > 0)
                                                {
                                                    DicConsole.Write("\rWaiting for drive to become ready");
                                                    Thread.Sleep(2000);
                                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                    if(!sense)
                                                        break;

                                                    leftRetries--;
                                                }

                                                mediaIsRecognized &= !sense;
                                            }
                                            else
                                            {
                                                mediaIsRecognized = false;
                                            }
                                        else
                                            mediaIsRecognized = false;
                                    }

                                    var mediaTest = new TestedMedia();

                                    if(mediaIsRecognized)
                                    {
                                        mediaTest = reporter.ReportScsiMedia();

                                        if(mediaTest.SupportsReadLong == true &&
                                           mediaTest.LongBlockSize    == mediaTest.BlockSize)
                                        {
                                            pressedKey = new ConsoleKeyInfo();

                                            while(pressedKey.Key != ConsoleKey.Y &&
                                                  pressedKey.Key != ConsoleKey.N)
                                            {
                                                DicConsole.
                                                    Write("Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours) (Y/N): ");

                                                pressedKey = System.Console.ReadKey();
                                                DicConsole.WriteLine();
                                            }

                                            if(pressedKey.Key == ConsoleKey.Y)
                                            {
                                                for(ushort i = (ushort)mediaTest.BlockSize;; i++)
                                                {
                                                    DicConsole.
                                                        Write("\rTrying to READ LONG with a size of {0} bytes...", i);

                                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                           i, dev.Timeout, out _);

                                                    if(!sense)
                                                    {
                                                        mediaTest.LongBlockSize = i;

                                                        break;
                                                    }

                                                    if(i == ushort.MaxValue)
                                                        break;
                                                }

                                                DicConsole.WriteLine();
                                            }
                                        }

                                        if(MainClass.Debug                    &&
                                           mediaTest.SupportsReadLong == true &&
                                           mediaTest.LongBlockSize    != mediaTest.BlockSize)
                                        {
                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                   (ushort)mediaTest.LongBlockSize, dev.Timeout, out _);

                                            if(!sense)
                                                mediaTest.ReadLong10Data = buffer;
                                        }
                                    }

                                    mediaTest.MediumTypeName    = mediumTypeName;
                                    mediaTest.Manufacturer      = mediumManufacturer;
                                    mediaTest.Model             = mediumModel;
                                    mediaTest.MediaIsRecognized = mediaIsRecognized;

                                    mediaTests.Add(mediaTest);

                                    dev.AllowMediumRemoval(out buffer, dev.Timeout, out _);
                                    dev.EjectTray(out buffer, dev.Timeout, out _);
                                }

                                report.SCSI.RemovableMedias = mediaTests;
                            }
                            else
                            {
                                report.SCSI.ReadCapabilities = reporter.ReportScsi();

                                if(report.SCSI.ReadCapabilities.SupportsReadLong == true &&
                                   report.SCSI.ReadCapabilities.LongBlockSize ==
                                   report.SCSI.ReadCapabilities.BlockSize)
                                {
                                    pressedKey = new ConsoleKeyInfo();

                                    while(pressedKey.Key != ConsoleKey.Y &&
                                          pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole.
                                            Write("Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours) (Y/N): ");

                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    if(pressedKey.Key == ConsoleKey.Y)
                                    {
                                        for(ushort i = (ushort)report.SCSI.ReadCapabilities.BlockSize;; i++)
                                        {
                                            DicConsole.Write("\rTrying to READ LONG with a size of {0} bytes...", i);

                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, i,
                                                                   dev.Timeout, out _);

                                            if(!sense)
                                            {
                                                if(MainClass.Debug)
                                                    report.SCSI.ReadCapabilities.ReadLong10Data = buffer;

                                                report.SCSI.ReadCapabilities.LongBlockSize = i;

                                                break;
                                            }

                                            if(i == ushort.MaxValue)
                                                break;
                                        }

                                        DicConsole.WriteLine();
                                    }
                                }

                                if(MainClass.Debug                                       &&
                                   report.SCSI.ReadCapabilities.SupportsReadLong == true &&
                                   report.SCSI.ReadCapabilities.LongBlockSize !=
                                   report.SCSI.ReadCapabilities.BlockSize)
                                {
                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                           (ushort)report.SCSI.ReadCapabilities.LongBlockSize,
                                                           dev.Timeout, out _);

                                    if(!sense)
                                        report.SCSI.ReadCapabilities.ReadLong10Data = buffer;
                                }
                            }

                            break;
                        }
                    }

                    break;
                default: throw new NotSupportedException("Unknown device type.");
            }

            var jsonFs = new FileStream(jsonFile, FileMode.Create);
            var jsonSw = new StreamWriter(jsonFs);

            jsonSw.Write(JsonConvert.SerializeObject(report, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }));

            jsonSw.Close();
            jsonFs.Close();

            using(var ctx = DicContext.Create(Settings.Settings.LocalDbPath))
            {
                ctx.Reports.Add(new Report(report));
                ctx.SaveChanges();
            }

            // TODO:
            if(Settings.Settings.Current.ShareReports)
                Remote.SubmitReport(report);

            return(int)ErrorNumber.NoError;
        }
    }
}