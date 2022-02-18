// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SecureDigital.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from SecureDigital and MultiMediaCard devices.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Metadata;
using Aaru.Console;
using Spectre.Console;

namespace Aaru.Core.Devices.Report
{
    /// <summary>Implements creating a device report for a SecureDigital or MultiMediaCard flash card</summary>
    public sealed partial class DeviceReport
    {
        /// <summary>Creates a device report for a SecureDigital or MultiMediaCard flash card</summary>
        public MmcSd MmcSdReport()
        {
            var    report = new MmcSd();
            bool   sense  = true;
            byte[] cid    = Array.Empty<byte>();
            byte[] csd    = Array.Empty<byte>();
            byte[] ecsd   = Array.Empty<byte>();
            byte[] scr    = Array.Empty<byte>();

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying to get CID...").IsIndeterminate();
                sense = _dev.ReadCid(out cid, out _, _dev.Timeout, out _);
            });

            if(!sense)
            {
                AaruConsole.WriteLine("CID obtained correctly...");

                switch(_dev.Type)
                {
                    case DeviceType.SecureDigital:
                        // Clear serial number and manufacturing date
                        cid[9]  = 0;
                        cid[10] = 0;
                        cid[11] = 0;
                        cid[12] = 0;
                        cid[13] = 0;
                        cid[14] = 0;

                        break;
                    case DeviceType.MMC:
                        // Clear serial number and manufacturing date
                        cid[10] = 0;
                        cid[11] = 0;
                        cid[12] = 0;
                        cid[13] = 0;
                        cid[14] = 0;

                        break;
                }

                report.CID = cid;
            }
            else
                AaruConsole.WriteLine("Could not read CID...");

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying to get CSD...").IsIndeterminate();
                sense = _dev.ReadCsd(out csd, out _, _dev.Timeout, out _);
            });

            if(!sense)
            {
                AaruConsole.WriteLine("CSD obtained correctly...");
                report.CSD = csd;
            }
            else
                AaruConsole.WriteLine("Could not read CSD...");

            sense = true;
            byte[] ocr = null;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying to get OCR...").IsIndeterminate();

                switch(_dev.Type)
                {
                    case DeviceType.MMC:
                    {
                        sense = _dev.ReadOcr(out ocr, out _, _dev.Timeout, out _);

                        break;
                    }
                    case DeviceType.SecureDigital:
                    {
                        sense = _dev.ReadSdocr(out ocr, out _, _dev.Timeout, out _);

                        break;
                    }
                }
            });

            if(!sense)
            {
                AaruConsole.WriteLine("OCR obtained correctly...");
                report.OCR = ocr;
            }
            else
                AaruConsole.WriteLine("Could not read OCR...");

            switch(_dev.Type)
            {
                case DeviceType.MMC:
                {
                    Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask("Trying to get Extended CSD...").IsIndeterminate();
                        sense = _dev.ReadExtendedCsd(out ecsd, out _, _dev.Timeout, out _);
                    });

                    if(!sense)
                    {
                        AaruConsole.WriteLine("Extended CSD obtained correctly...");
                        report.ExtendedCSD = ecsd;
                    }
                    else
                        AaruConsole.WriteLine("Could not read Extended CSD...");

                    break;
                }
                case DeviceType.SecureDigital:
                {
                    Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask("Trying to get SCR...").IsIndeterminate();
                        sense = _dev.ReadScr(out scr, out _, _dev.Timeout, out _);
                    });

                    if(!sense)
                    {
                        AaruConsole.WriteLine("SCR obtained correctly...");
                        report.SCR = scr;
                    }
                    else
                        AaruConsole.WriteLine("Could not read SCR...");

                    break;
                }
            }

            return report;
        }
    }
}