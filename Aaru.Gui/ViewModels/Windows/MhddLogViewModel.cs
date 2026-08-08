// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MhddLogViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model for MHDD log viewer.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Aaru.Gui.Views.Windows;
using Aaru.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Sentry;

namespace Aaru.Gui.ViewModels.Windows;

public partial class MhddLogViewModel : ViewModelBase
{
    readonly MhddLogView _window;
    [ObservableProperty]
    string _device;
    [ObservableProperty]
    string _filePath;
    [ObservableProperty]
    string _firmware;
    [ObservableProperty]
    string _mhddVersion;
    [ObservableProperty]
    string _scanBlockSize;
    [ObservableProperty]
    ObservableCollection<(ulong startingSector, double duration)> _sectorData;
    [ObservableProperty]
    string _sectorSize;
    [ObservableProperty]
    string _serialNumber;
    [ObservableProperty]
    string _totalSectors;

    public MhddLogViewModel(MhddLogView window, [NotNull] string filePath)
    {
        _window     = window;
        FilePath    = filePath;
        _sectorData = [];
    }

    public void LoadData()
    {
        Stream stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        stream.Seek(0, SeekOrigin.Begin);
        var buffer = new byte[4];
        stream.ReadExactly(buffer, 0, 4);
        var pointer = BitConverter.ToInt32(buffer, 0);
        int d       = stream.ReadByte();
        int a       = stream.ReadByte();
        stream.ReadExactly(buffer, 0, 4);
        string ver = Encoding.ASCII.GetString(buffer, 0, 4);

        if(pointer > stream.Length || d != 0x0D || a != 0x0A || ver != "VER:")
        {
            stream.Close();

            _ = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                        UI.The_specified_file_is_not_a_correct_MHDD_log_file,
                                                        ButtonEnum.Ok,
                                                        Icon.Error)
                                 .ShowWindowDialogAsync(_window);

            _window.Close();

            return;
        }

        stream.Position = 4;

        buffer = new byte[pointer - 4];
        stream.ReadExactly(buffer, 0, buffer.Length);
        string header = Encoding.ASCII.GetString(buffer, 0, buffer.Length);

        try
        {
            // Parse VER field
            Match versionMatch = VersionRegex().Match(header);

            if(versionMatch.Success) MhddVersion = $"[green]{versionMatch.Groups[1].Value}[/]";

            // Parse DEVICE field
            Match deviceMatch = DeviceRegex().Match(header);

            if(deviceMatch.Success) Device = $"[pink]{deviceMatch.Groups[1].Value.Trim()}[/]";

            // Parse F/W field
            Match firmwareMatch = FirmwareRegex().Match(header);

            if(firmwareMatch.Success) Firmware = $"[rosybrown]{firmwareMatch.Groups[1].Value}[/]";

            // Parse S/N field
            Match serialMatch = SerialNumberRegex().Match(header);

            if(serialMatch.Success) SerialNumber = $"[purple]{serialMatch.Groups[1].Value}[/]";

            // Parse SECTORS field
            Match sectorsMatch = SectorsRegex().Match(header);

            if(sectorsMatch.Success)
                TotalSectors = $"[teal]{ParseNumberWithSeparator(sectorsMatch.Groups[1].Value)}[/]";

            // Parse SECTOR SIZE field
            Match sectorSizeMatch = SectorSizeRegex().Match(header);

            if(sectorSizeMatch.Success)
            {
                SectorSize = string.Format(UI._0_bytes_markup,
                                           ParseNumberWithSeparator(sectorSizeMatch.Groups[1].Value));
            }

            // Parse SCAN BLOCK SIZE field
            Match scanBlockMatch = ScanBlockSizeRegex().Match(header);

            if(scanBlockMatch.Success)
            {
                ScanBlockSize = string.Format(UI._0_sectors_markup,
                                              ParseNumberWithSeparator(scanBlockMatch.Groups[1].Value));
            }
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);

            _ = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                        string.Format(UI.Error_parsing_MHDD_log_header_0, ex.Message),
                                                        ButtonEnum.Ok,
                                                        Icon.Error)
                                 .ShowWindowDialogAsync(_window);

            _window.Close();

            stream.Close();

            return;
        }

        stream.Position = pointer;

        buffer = new byte[8];
        SectorData.Clear();

        while(stream.Position < stream.Length)
        {
            stream.ReadExactly(buffer, 0, 8);
            var sector = BitConverter.ToUInt64(buffer, 0);
            stream.ReadExactly(buffer, 0, 8);
            double duration = BitConverter.ToUInt64(buffer, 0) / 1000.0;
            SectorData.Add((sector, duration));
        }

        stream.Close();
    }

    /// <summary>
    ///     Parses a number string that may contain thousands separators (en-US culture).
    /// </summary>
    /// <param name="value">The number string (e.g., "243,587" or "2,448")</param>
    /// <returns>The parsed number without separators</returns>
    static ulong ParseNumberWithSeparator(string value) => ulong.Parse(value.Replace(",", ""));

    [GeneratedRegex(@"VER:\s*(\S+)")]
    private static partial Regex VersionRegex();

    [GeneratedRegex(@"DEVICE:\s*(.+?)(?=\n|$)")]
    private static partial Regex DeviceRegex();

    [GeneratedRegex(@"F/W:\s*(\S+)")]
    private static partial Regex FirmwareRegex();

    [GeneratedRegex(@"S/N:\s*(\S+)")]
    private static partial Regex SerialNumberRegex();

    [GeneratedRegex(@"SECTORS:\s*([\d,]+)")]
    private static partial Regex SectorsRegex();

    [GeneratedRegex(@"SECTOR SIZE:\s*([\d,]+)\s*bytes")]
    private static partial Regex SectorSizeRegex();

    [GeneratedRegex(@"SCAN BLOCK SIZE:\s*([\d,]+)\s*sectors")]
    private static partial Regex ScanBlockSizeRegex();
}