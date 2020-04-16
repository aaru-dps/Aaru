using System.Collections.ObjectModel;
using Aaru.CommonTypes;
using Aaru.Gui.ViewModels.Panels;
using Avalonia.Media.Imaging;

namespace Aaru.Gui.Models
{
    public class PartitionModel
    {
        public PartitionModel() => FileSystems = new ObservableCollection<FileSystemModel>();

        public string                                Name        { get; set; }
        public Bitmap                                Icon        { get; set; }
        public ObservableCollection<FileSystemModel> FileSystems { get; }
        public Partition                             Partition   { get; set; }
        public PartitionViewModel                    ViewModel   { get; set; }
    }
}