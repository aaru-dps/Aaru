using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;

namespace Aaru.Gui.Models
{
    public class DeviceModel
    {
        public DeviceModel() => Media = new ObservableCollection<MediaModel>();

        public Bitmap Icon { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public ObservableCollection<MediaModel> Media { get; }
    }
}