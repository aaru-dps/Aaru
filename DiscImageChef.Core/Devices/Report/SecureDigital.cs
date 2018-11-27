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

using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;

namespace DiscImageChef.Core.Devices.Report
{
    /// <summary>
    ///     Implements creating a device report for a SecureDigital or MultiMediaCard flash card
    /// </summary>
    public partial class DeviceReport
    {
        /// <summary>
        ///     Creates a device report for a SecureDigital or MultiMediaCard flash card
        /// </summary>
        public MmcSd MmcSdReport()
        {
            MmcSd report = new MmcSd();

            DicConsole.WriteLine("Trying to get CID...");
            bool sense = dev.ReadCid(out byte[] cid, out _, dev.Timeout, out _);

            if(!sense)
            {
                DicConsole.WriteLine("CID obtained correctly...");

                switch(dev.Type)
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
            else DicConsole.WriteLine("Could not read CID...");

            DicConsole.WriteLine("Trying to get CSD...");
            sense = dev.ReadCsd(out byte[] csd, out _, dev.Timeout, out _);

            if(!sense)
            {
                DicConsole.WriteLine("CSD obtained correctly...");
                report.CSD = csd;
            }
            else DicConsole.WriteLine("Could not read CSD...");

            sense = true;
            byte[] ocr = null;
            DicConsole.WriteLine("Trying to get OCR...");
            switch(dev.Type)
            {
                case DeviceType.MMC:
                {
                    sense = dev.ReadOcr(out ocr, out _, dev.Timeout, out _);
                    break;
                }
                case DeviceType.SecureDigital:
                {
                    sense = dev.ReadSdocr(out ocr, out _, dev.Timeout, out _);
                    break;
                }
            }

            if(!sense)
            {
                DicConsole.WriteLine("OCR obtained correctly...");
                report.OCR = ocr;
            }
            else DicConsole.WriteLine("Could not read OCR...");

            switch(dev.Type)
            {
                case DeviceType.MMC:
                {
                    DicConsole.WriteLine("Trying to get Extended CSD...");
                    sense = dev.ReadExtendedCsd(out byte[] ecsd, out _, dev.Timeout, out _);

                    if(!sense)
                    {
                        DicConsole.WriteLine("Extended CSD obtained correctly...");
                        report.ExtendedCSD = ecsd;
                    }
                    else DicConsole.WriteLine("Could not read Extended CSD...");

                    break;
                }
                case DeviceType.SecureDigital:
                {
                    DicConsole.WriteLine("Trying to get SCR...");
                    sense = dev.ReadScr(out byte[] scr, out _, dev.Timeout, out _);

                    if(!sense)
                    {
                        DicConsole.WriteLine("SCR obtained correctly...");
                        report.SCR = scr;
                    }
                    else DicConsole.WriteLine("Could not read SCR...");

                    break;
                }
            }

            return report;
        }
    }
}