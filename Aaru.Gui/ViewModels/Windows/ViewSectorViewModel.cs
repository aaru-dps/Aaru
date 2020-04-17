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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Interfaces;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Windows
{
    public class ViewSectorViewModel : ViewModelBase
    {
        const    int         HEX_COLUMNS = 32;
        readonly IMediaImage inputFormat;
        bool                 _longSectorChecked;
        bool                 _longSectorVisible;
        string               _printHexText;
        double               _sectorNumber;
        string               _title;
        string               _totalSectorsText;

        public ViewSectorViewModel(IMediaImage inputFormat)
        {
            this.inputFormat = inputFormat;

            try
            {
                inputFormat.ReadSectorLong(0);
                LongSectorChecked = true;
            }
            catch
            {
                LongSectorVisible = false;
            }

            TotalSectorsText = $"of {inputFormat.Info.Sectors}";
            SectorNumber     = 0;
        }

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public double SectorNumber
        {
            get => _sectorNumber;
            set
            {
                this.RaiseAndSetIfChanged(ref _sectorNumber, value);

                PrintHexText =
                    PrintHex.
                        ByteArrayToHexArrayString(LongSectorChecked ? inputFormat.ReadSectorLong((ulong)SectorNumber) : inputFormat.ReadSector((ulong)SectorNumber),
                                                  HEX_COLUMNS);
            }
        }

        public string TotalSectorsText { get; }

        public bool LongSectorChecked
        {
            get => _longSectorChecked;
            set => this.RaiseAndSetIfChanged(ref _longSectorChecked, value);
        }

        public bool LongSectorVisible { get; }

        public string PrintHexText
        {
            get => _printHexText;
            set => this.RaiseAndSetIfChanged(ref _printHexText, value);
        }
    }
}