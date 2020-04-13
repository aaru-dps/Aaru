using Aaru.CommonTypes.Interfaces;
using ReactiveUI;

namespace Aaru.Gui.ViewModels
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