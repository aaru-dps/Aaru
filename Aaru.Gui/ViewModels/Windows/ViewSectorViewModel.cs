// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ViewSectorViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the sector viewing window.
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

using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Gui.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace Aaru.Gui.ViewModels.Windows;

public sealed partial class ViewSectorViewModel : ViewModelBase
{
    readonly IMediaImage _inputFormat;
    [ObservableProperty]
    List<ColorRange> _highlightRanges;
    [ObservableProperty]
    bool _longSectorVisible;
    [ObservableProperty]
    string _printHexText;
    [ObservableProperty]
    byte[] _sectorData;
    [ObservableProperty]
    string _totalSectorsText;

    // TODO: Show message when sector was not dumped
    public ViewSectorViewModel([NotNull] IMediaImage inputFormat)
    {
        _inputFormat = inputFormat;

        ErrorNumber errno = inputFormat.ReadSectorLong(0, false, out _, out _);

        LongSectorVisible = errno == ErrorNumber.NoError;
        TotalSectorsText  = $"of {inputFormat.Info.Sectors}";
        SectorNumber      = 0;
    }

    public bool LongSectorChecked
    {
        get;
        set
        {
            SetProperty(ref field, value);
            ReadSectorAt((long)SectorNumber);
        }
    }

    public double SectorNumber
    {
        get;
        set
        {
            SetProperty(ref field, value);
            ReadSectorAt((long)SectorNumber);
        }
    }

    void ReadSectorAt(long displaySector)
    {
        bool  negative      = displaySector < 0;
        ulong sectorAddress = negative ? (ulong)-displaySector : (ulong)displaySector;

        ErrorNumber errno = LongSectorChecked
                                ? _inputFormat.ReadSectorLong(sectorAddress, negative, out byte[] sector, out _)
                                : _inputFormat.ReadSector(sectorAddress, negative, out sector, out _);

        if(errno != ErrorNumber.NoError) return;

        SectorData = sector;
        ColorSector();
    }

    void ColorSector()
    {
        HighlightRanges = [];

        if(SectorData.LongLength == 2064 && LongSectorChecked)
        {
            // DVD sector

            ColorRange dvdIdSi = new ColorRange
            {
                Color = Brushes.DeepPink,
                Start = 0,
                End   = 0
            };

            ColorRange dvdIdSn = new ColorRange
            {
                Color = Brushes.Yellow,
                Start = 1,
                End   = 3
            };

            ColorRange dvdIed = new ColorRange
            {
                Color = Brushes.Green,
                Start = 4,
                End   = 5
            };

            ColorRange dvdCprmai = new ColorRange
            {
                Color = Brushes.Orange,
                Start = 6,
                End   = 11
            };

            if(_inputFormat.Info.MediaType is CommonTypes.MediaType.GOD or CommonTypes.MediaType.WOD)
            {
                dvdCprmai = new ColorRange
                {
                    Color = Brushes.Orange,
                    Start = 2054,
                    End   = 2059
                };
            }

            ColorRange dvdEdc = new ColorRange
            {
                Color = Brushes.LimeGreen,
                Start = 2060,
                End   = 2063
            };

            HighlightRanges = [dvdIdSi, dvdIdSn, dvdIed, dvdCprmai, dvdEdc];

            return;
        }

        if(SectorData.Length == 2052 && LongSectorChecked)
        {
            // Blu-ray sector

            ColorRange bdEdc = new ColorRange
            {
                Color = Brushes.LimeGreen,
                Start = 2048,
                End   = 2051
            };

            HighlightRanges = [bdEdc];

            return;
        }

        // Not a standard CD sector
        if(SectorData.Length != 2352) return;

        // Not a data sector
        if(SectorData[0]  != 0x00 ||
           SectorData[1]  != 0xFF ||
           SectorData[2]  != 0xFF ||
           SectorData[3]  != 0xFF ||
           SectorData[4]  != 0xFF ||
           SectorData[5]  != 0xFF ||
           SectorData[6]  != 0xFF ||
           SectorData[7]  != 0xFF ||
           SectorData[8]  != 0xFF ||
           SectorData[9]  != 0xFF ||
           SectorData[10] != 0xFF ||
           SectorData[11] != 0x00)
            return;

        List<ColorRange> ranges = [];

        var sync = new ColorRange
        {
            Color = Brushes.DeepPink,
            Start = 0,
            End   = 11
        };

        // Synchronization field
        ranges.Add(sync);

        var msf = new ColorRange
        {
            Color = Brushes.Orange,
            Start = 12,
            End   = 14
        };

        // Block MSF address
        ranges.Add(msf);

        var mode = new ColorRange
        {
            Color = Brushes.Yellow,
            Start = 15,
            End   = 15
        };

        // Data mode
        ranges.Add(mode);

        ColorRange edc;
        ColorRange ecc_P;
        ColorRange ecc_Q;

        switch(SectorData[15])
        {
            // MODE 1
            case 1:
                edc = new ColorRange
                {
                    Color = Brushes.LimeGreen,
                    Start = 2064,
                    End   = 2067
                };

                ecc_P = new ColorRange
                {
                    Color = Brushes.Cyan,
                    Start = 2076,
                    End   = 2247
                };

                ecc_Q = new ColorRange
                {
                    Color = Brushes.Blue,
                    Start = 2248,
                    End   = 2351
                };

                // EDC
                ranges.Add(edc);

                // P parity symbols
                ranges.Add(ecc_P);

                // Q parity symbols
                ranges.Add(ecc_Q);

                break;

            // MODE 2
            case 2:
                // MODE 2 FORM 2
                if((SectorData[18] & 0x20) == 0x20)
                {
                    var subheader = new ColorRange
                    {
                        Color = Brushes.Red,
                        Start = 16,
                        End   = 23
                    };

                    edc = new ColorRange
                    {
                        Color = Brushes.LimeGreen,
                        Start = 2348,
                        End   = 2351
                    };

                    // Subheader
                    ranges.Add(subheader);

                    // EDC
                    ranges.Add(edc);
                }

                // MODE 2 FORM 1
                else
                {
                    var subheader = new ColorRange
                    {
                        Color = Brushes.Red,
                        Start = 16,
                        End   = 23
                    };

                    edc = new ColorRange
                    {
                        Color = Brushes.LimeGreen,
                        Start = 2072,
                        End   = 2075
                    };

                    ecc_P = new ColorRange
                    {
                        Color = Brushes.Cyan,
                        Start = 2076,
                        End   = 2247
                    };

                    ecc_Q = new ColorRange
                    {
                        Color = Brushes.Blue,
                        Start = 2248,
                        End   = 2351
                    };

                    // Subheader
                    ranges.Add(subheader);

                    // EDC
                    ranges.Add(edc);

                    // P parity symbols
                    ranges.Add(ecc_P);

                    // Q parity symbols
                    ranges.Add(ecc_Q);
                }

                break;
        }

        HighlightRanges = ranges;
    }
}