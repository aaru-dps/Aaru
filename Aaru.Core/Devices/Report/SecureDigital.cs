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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Metadata;
using Aaru.Console;
using Spectre.Console;

namespace Aaru.Core.Devices.Report;

/// <summary>Implements creating a device report for a SecureDigital or MultiMediaCard flash card</summary>
public sealed partial class DeviceReport
{
    /// <summary>Creates a device report for a SecureDigital or MultiMediaCard flash card</summary>
    public MmcSd MmcSdReport()
    {
        var    report = new MmcSd();
        var    sense  = true;
        byte[] cid    = [];
        byte[] csd    = [];
        byte[] ecsd   = [];
        byte[] scr    = [];

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(Localization.Core.Trying_to_get_CID).IsIndeterminate();
            sense = _dev.ReadCid(out cid, out _, _dev.Timeout, out _);
        });

        if(!sense)
        {
            AaruConsole.WriteLine(Localization.Core.CID_obtained_correctly);

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
            AaruConsole.WriteLine(Localization.Core.Could_not_read_CID);

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(Localization.Core.Trying_to_get_CSD).IsIndeterminate();
            sense = _dev.ReadCsd(out csd, out _, _dev.Timeout, out _);
        });

        if(!sense)
        {
            AaruConsole.WriteLine(Localization.Core.CSD_obtained_correctly);
            report.CSD = csd;
        }
        else
            AaruConsole.WriteLine(Localization.Core.Could_not_read_CSD);

        sense = true;
        byte[] ocr = null;

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(Localization.Core.Trying_to_get_OCR).IsIndeterminate();

            sense = _dev.Type switch
                    {
                        DeviceType.MMC           => _dev.ReadOcr(out ocr, out _, _dev.Timeout, out _),
                        DeviceType.SecureDigital => _dev.ReadSdocr(out ocr, out _, _dev.Timeout, out _),
                        _                        => sense
                    };
        });

        if(!sense)
        {
            AaruConsole.WriteLine(Localization.Core.OCR_obtained_correctly);
            report.OCR = ocr;
        }
        else
            AaruConsole.WriteLine(Localization.Core.Could_not_read_OCR);

        switch(_dev.Type)
        {
            case DeviceType.MMC:
            {
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(Localization.Core.Trying_to_get_Extended_CSD).IsIndeterminate();
                    sense = _dev.ReadExtendedCsd(out ecsd, out _, _dev.Timeout, out _);
                });

                if(!sense)
                {
                    AaruConsole.WriteLine(Localization.Core.Extended_CSD_obtained_correctly);
                    report.ExtendedCSD = ecsd;
                }
                else
                    AaruConsole.WriteLine(Localization.Core.Could_not_read_Extended_CSD);

                break;
            }
            case DeviceType.SecureDigital:
            {
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(Localization.Core.Trying_to_get_SCR).IsIndeterminate();
                    sense = _dev.ReadScr(out scr, out _, _dev.Timeout, out _);
                });

                if(!sense)
                {
                    AaruConsole.WriteLine(Localization.Core.SCR_obtained_correctly);
                    report.SCR = scr;
                }
                else
                    AaruConsole.WriteLine(Localization.Core.Could_not_read_SCR);

                break;
            }
        }

        return report;
    }
}