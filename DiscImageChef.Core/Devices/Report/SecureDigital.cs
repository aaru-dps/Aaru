// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Devices;
using DiscImageChef.Metadata;
using DiscImageChef.Console;

namespace DiscImageChef.Core.Devices.Report
{
    public static class SecureDigital
    {
        public static void Report(Device dev, ref DeviceReport report, bool debug, ref bool removable)
        {
            if(report == null)
                return;

            if(dev.Type == DeviceType.MMC)
                report.MultiMediaCard = new mmcsdType();
            else if(dev.Type == DeviceType.SecureDigital)
                report.SecureDigital = new mmcsdType();

            DicConsole.WriteLine("Trying to get CID...");
            bool sense = dev.ReadCID(out byte[] cid, out uint[] response, dev.Timeout, out double duration);

            if(!sense)
            {
                DicConsole.WriteLine("CID obtained correctly...");

                if(dev.Type == DeviceType.SecureDigital)
                {
                    // Clear serial number and manufacturing date
                    cid[9] = 0;
                    cid[10] = 0;
                    cid[11] = 0;
                    cid[12] = 0;
                    cid[13] = 0;
                    cid[14] = 0;
                    report.SecureDigital.CID = cid;
                }
                else if(dev.Type == DeviceType.MMC)
                {
                    // Clear serial number and manufacturing date
                    cid[10] = 0;
                    cid[11] = 0;
                    cid[12] = 0;
                    cid[13] = 0;
                    cid[14] = 0;
                    report.MultiMediaCard.CID = cid;
                }
            }
            else
                DicConsole.WriteLine("Could not read CID...");

            DicConsole.WriteLine("Trying to get CSD...");
            sense = dev.ReadCSD(out byte[] csd, out response, dev.Timeout, out duration);

            if(!sense)
            {
                DicConsole.WriteLine("CSD obtained correctly...");

                if(dev.Type == DeviceType.MMC)
                    report.MultiMediaCard.CSD = csd;
                else if(dev.Type == DeviceType.SecureDigital)
                    report.SecureDigital.CSD = csd;
            }
            else
                DicConsole.WriteLine("Could not read CSD...");

            if(dev.Type == DeviceType.MMC)
            {
                DicConsole.WriteLine("Trying to get OCR...");
                sense = dev.ReadOCR(out byte[] ocr, out response, dev.Timeout, out duration);

                if(!sense)
                {
                    DicConsole.WriteLine("OCR obtained correctly...");
                    report.MultiMediaCard.OCR = ocr;
                }
                else
                    DicConsole.WriteLine("Could not read OCR...");

                DicConsole.WriteLine("Trying to get Extended CSD...");
                sense = dev.ReadExtendedCSD(out byte[] ecsd, out response, dev.Timeout, out duration);

                if(!sense)
                {
                    DicConsole.WriteLine("Extended CSD obtained correctly...");
                    report.MultiMediaCard.ExtendedCSD = ecsd;
                }
                else
                    DicConsole.WriteLine("Could not read Extended CSD...");
            }
            else if(dev.Type == DeviceType.SecureDigital)
            {
                DicConsole.WriteLine("Trying to get OCR...");
                sense = dev.ReadSDOCR(out byte[] ocr, out response, dev.Timeout, out duration);

                if(!sense)
                {
                    DicConsole.WriteLine("OCR obtained correctly...");
                    report.SecureDigital.OCR = ocr;
                }
                else
                    DicConsole.WriteLine("Could not read OCR...");

                DicConsole.WriteLine("Trying to get SCR...");
                sense = dev.ReadSCR(out byte[] scr, out response, dev.Timeout, out duration);

                if(!sense)
                {
                    DicConsole.WriteLine("SCR obtained correctly...");
                    report.SecureDigital.SCR = scr;
                }
                else
                    DicConsole.WriteLine("Could not read SCR...");
            }

        }
    }
}
