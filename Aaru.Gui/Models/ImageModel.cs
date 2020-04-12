using System.Collections.ObjectModel;
using Aaru.CommonTypes.Interfaces;
using Aaru.Gui.ViewModels;
using Avalonia.Media.Imaging;

namespace Aaru.Gui.Models
{
    public class ImageModel
    {
        public ImageModel() => PartitionSchemesOrFileSystems = new ObservableCollection<RootModel>();

        public string                          Path                          { get; set; }
        public string                          FileName                      { get; set; }
        public Bitmap                          Icon                          { get; set; }
        public ObservableCollection<RootModel> PartitionSchemesOrFileSystems { get; }
        public IMediaImage                     Image                         { get; set; }
        public ImageInfoViewModel              ViewModel                     { get; set; }
    }
}