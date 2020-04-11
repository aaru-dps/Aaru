using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;

namespace Aaru.Gui.Models
{
    public class PartitionSchemeModel : RootModel
    {
        public PartitionSchemeModel() => Partitions = new ObservableCollection<PartitionModel>();

        public Bitmap                               Icon       { get; set; }
        public ObservableCollection<PartitionModel> Partitions { get; }
    }
}