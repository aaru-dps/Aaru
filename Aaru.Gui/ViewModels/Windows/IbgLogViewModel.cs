// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IbgLogViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model for IMGBurn log viewer.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.Gui.Views.Windows;
using Aaru.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using Humanizer;
using JetBrains.Annotations;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Aaru.Gui.ViewModels.Windows;

public partial class IbgLogViewModel : ViewModelBase
{
    readonly IbgLogView _window;
    [ObservableProperty]
    string _bus;
    [ObservableProperty]
    string _capacity;
    [ObservableProperty]
    string _date;
    [ObservableProperty]
    string _device;
    [ObservableProperty]
    string _filePath;
    [ObservableProperty]
    string _firmware;
    [ObservableProperty]
    string _imagefile;
    [ObservableProperty]
    ulong _maxSector;
    [ObservableProperty]
    double _maxSpeed;
    [ObservableProperty]
    string _mediaSpeeds;
    [ObservableProperty]
    string _mediaType;
    [ObservableProperty]
    ulong _sectors;
    [ObservableProperty]
    string _speedAverage;
    [ObservableProperty]
    ObservableCollection<(ulong sector, double speedKbps)> _speedData = [];
    [ObservableProperty]
    string _speedEnd;
    [ObservableProperty]
    int _speedMultiplier = 1353;
    [ObservableProperty]
    string _speedStart;
    [ObservableProperty]
    string _timeTaken;
    [ObservableProperty]
    string _volumeIdentifier;

    public IbgLogViewModel(IbgLogView window, [NotNull] string filePath)
    {
        _window  = window;
        FilePath = filePath;
    }

    public void LoadData()
    {
        Stream stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        stream.Seek(0, SeekOrigin.Begin);
        var buffer = new byte[4];
        stream.ReadExactly(buffer, 0, 4);
        string id = Encoding.ASCII.GetString(buffer);

        if(id != "IBGD")
        {
            stream.Close();

            _ = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                        UI.The_specified_file_is_not_a_correct_IMGBurn_log_file,
                                                        ButtonEnum.Ok,
                                                        Icon.Error)
                                 .ShowWindowDialogAsync(_window);

            _window.Close();

            return;
        }

        stream.Position = 0;

        var sr = new StreamReader(stream);

        var                       inConfiguration = false;
        var                       inGraphValues   = false;
        int                       multiplier;
        var                       ibgCulture       = new CultureInfo("en-US");
        string                    device           = null;
        string                    firmware         = null;
        string                    bus              = null;
        DateTime                  date             = DateTime.MinValue;
        string                    mediaSpeeds      = null;
        string                    capacity         = null;
        ulong                     sectors          = 0;
        string                    imagefile        = null;
        string                    volumeIdentifier = null;
        string                    mediaType        = null;
        double                    speedStart       = 0;
        double                    speedEnd         = 0;
        double                    speedAverage     = 0;
        uint                      timeTaken        = 0;
        Dictionary<ulong, double> speeds           = [];

        while(!sr.EndOfStream)
        {
            string line = sr.ReadLine();

            if(line == "[START_CONFIGURATION]")
            {
                inConfiguration = true;

                continue;
            }

            if(inConfiguration)
            {
                if(line.StartsWith("DATE=", StringComparison.Ordinal))
                {
                    string dateString = line["DATE=".Length..];

                    DateTime.TryParseExact(dateString,
                                           "M/d/yyyy h:mm:ss tt",
                                           ibgCulture,
                                           DateTimeStyles.None,
                                           out date);
                }
                else if(line.StartsWith("DEVICE_MAKEMODEL=", StringComparison.Ordinal))
                    device = line["DEVICE_MAKEMODEL=".Length..];
                else if(line.StartsWith("DEVICE_FIRMWAREVERSION=", StringComparison.Ordinal))
                    firmware = line["DEVICE_FIRMWAREVERSION=".Length..];
                else if(line.StartsWith("DEVICE_BUSTYPE=", StringComparison.Ordinal))
                    bus = line["DEVICE_BUSTYPE=".Length..];
                else if(line.StartsWith("MEDIA_TYPE=", StringComparison.Ordinal))
                    mediaType = line["MEDIA_TYPE=".Length..];
                else if(line.StartsWith("MEDIA_SPEEDS=", StringComparison.Ordinal))
                {
                    mediaSpeeds = line["MEDIA_SPEEDS=".Length..];
                    if(mediaSpeeds == "N/A") mediaSpeeds = null;
                }
                else if(line.StartsWith("MEDIA_CAPACITY=", StringComparison.Ordinal))
                    capacity = line["MEDIA_CAPACITY=".Length..];
                else if(line.StartsWith("DATA_IMAGEFILE", StringComparison.Ordinal))
                {
                    imagefile = line["DATA_IMAGEFILE=".Length..];
                    if(imagefile == "/dev/null") imagefile = null;
                }
                else if(line.StartsWith("DATA_SECTORS=", StringComparison.Ordinal))
                    ulong.TryParse(line["DATA_SECTORS=".Length..], ibgCulture, out sectors);
                else if(line.StartsWith("DATA_VOLUMEIDENTIFIER=", StringComparison.Ordinal))
                    volumeIdentifier = line["DATA_VOLUMEIDENTIFIER=".Length..];
                else if(line.StartsWith("VERIFY_SPEED_START=", StringComparison.Ordinal))
                    double.TryParse(line["VERIFY_SPEED_START=".Length..], ibgCulture, out speedStart);
                else if(line.StartsWith("VERIFY_SPEED_END=", StringComparison.Ordinal))
                    double.TryParse(line["VERIFY_SPEED_END=".Length..], ibgCulture, out speedEnd);
                else if(line.StartsWith("VERIFY_SPEED_AVERAGE=", StringComparison.Ordinal))
                    double.TryParse(line["VERIFY_SPEED_AVERAGE=".Length..], ibgCulture, out speedAverage);
                else if(line.StartsWith("VERIFY_TIME_TAKEN=", StringComparison.Ordinal))
                    uint.TryParse(line["VERIFY_TIME_TAKEN=".Length..], ibgCulture, out timeTaken);
                else if(line == "[END_CONFIGURATION]") inConfiguration = false;

                continue;
            }

            switch(line)
            {
                case "[START_VERIFY_GRAPH_VALUES]":
                    inGraphValues = true;

                    continue;
                case "[END_VERIFY_GRAPH_VALUES]":
                    inGraphValues = false;

                    continue;
            }

            if(!inGraphValues) continue;

            string[] graphValues = line.Split(',');

            if(graphValues.Length == 4                                      &&
               ulong.TryParse(graphValues[1], ibgCulture, out ulong sector) &&
               double.TryParse(graphValues[0], ibgCulture, out double speed))
                speeds[sector] = speed;
        }

        double maxSpeedValue = 0;

        switch(mediaType)
        {
            case "HDD":
                multiplier    = 1353;
                maxSpeedValue = 1500000; // 1500 MB/s cap for graph scaling

                break;
            case "PD-650":
            case "CD-MO":
            case "CD-ROM":
            case "CD-R":
            case "CD-RW":
            case "DDCD-ROM":
            case "DDCD-R":
            case "DDCD-RW":
                multiplier    = 150;
                maxSpeedValue = 11250; // 52x CD-ROM cap for graph scaling

                break;
            case "DVD-ROM":
            case "DVD-R":
            case "DVD-RAM":
            case "DVD-RW":
            case "DVD-R DL":
            case "DVD-RW DL":
            case "DVD-Download":
            case "DVD+RW":
            case "DVD+R":
            case "DVD+RW DL":
            case "DVD+R DL":
                multiplier    = 1353;
                maxSpeedValue = 32472; // 24x DVD-ROM cap for graph scaling

                break;
            case "BD-ROM":
            case "BD-R":
            case "BD-RE":
                multiplier    = 4500;
                maxSpeedValue = 108000; // 24x BD-ROM cap for graph scaling

                break;
            case "HD DVD-ROM":
            case "HD DVD-R":
            case "HD DVD-RAM":
            case "HD DVD-RW":
            case "HD DVD-R DL":
            case "HD DVD-RW DL":
                multiplier    = 4500;
                maxSpeedValue = 36550; // 8x HD-DVD cap for graph scaling

                break;
            default:

                multiplier = 1353;

                break;
        }

        Dictionary<ulong, double> fixedSpeeds = [];

        foreach(KeyValuePair<ulong, double> kvp in speeds) fixedSpeeds[kvp.Key] = kvp.Value * multiplier;

        speeds       =  fixedSpeeds;
        speedStart   *= multiplier;
        speedEnd     *= multiplier;
        speedAverage *= multiplier;

        Device           = device      != null ? $"[pink]{device}[/]" : null;
        Firmware         = firmware    != null ? $"[rosybrown]{firmware}[/]" : null;
        Bus              = bus         != null ? $"[purple]{bus}[/]" : null;
        Date             = date        != DateTime.MinValue ? $"[yellow]{date}[/]" : null;
        MediaSpeeds      = mediaSpeeds != null ? $"[red]{mediaSpeeds}[/]" : null;
        Capacity         = capacity    != null ? string.Format(UI._0_sectors_markup, capacity) : null;
        Imagefile        = imagefile   != null ? $"[green]{imagefile}[/]" : null;
        VolumeIdentifier = !string.IsNullOrEmpty(volumeIdentifier) ? $"[cyan]{volumeIdentifier}[/]" : null;
        MediaType        = mediaType    != null ? $"[orange]{mediaType}[/]" : null;
        SpeedStart       = speedStart   != 0 ? string.Format(UI._0_N2_KB_s, speedStart) : null;
        SpeedEnd         = speedEnd     != 0 ? string.Format(UI._0_N2_KB_s, speedEnd) : null;
        SpeedAverage     = speedAverage != 0 ? string.Format(UI._0_N2_KB_s, speedAverage) : null;
        TimeTaken        = timeTaken    != 0 ? $"[aqua]{TimeSpan.FromSeconds(timeTaken).Humanize()}[/]" : null;

        // Populate graph data
        SpeedMultiplier = multiplier;
        MaxSector       = sectors;

        // Find max speed for Y-axis scaling
        if(maxSpeedValue == 0) maxSpeedValue = speeds.Select(static kvp => kvp.Value).Prepend(maxSpeedValue).Max();

        MaxSpeed = maxSpeedValue;

        // Populate speed data for graph
        SpeedData.Clear();

        foreach(KeyValuePair<ulong, double> kvp in speeds.OrderBy(static k => k.Key))
            SpeedData.Add((kvp.Key, kvp.Value));
    }
}