using Aaru.Gui.ViewModels.Panels;
using Avalonia.Media.Imaging;

namespace Aaru.Gui.Models
{
    public class MediaModel
    {
        public Bitmap             Icon            { get; set; }
        public string             Name            { get; set; }
        public string             DevicePath      { get; set; }
        public bool               NonRemovable    { get; set; }
        public bool               NoMediaInserted { get; set; }
        public MediaInfoViewModel ViewModel       { get; set; }
    }
}