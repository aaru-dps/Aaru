using System.Collections.ObjectModel;

namespace Aaru.Gui.Models
{
    public class ImagesRootModel : RootModel
    {
        public ImagesRootModel() => Images = new ObservableCollection<ImageModel>();

        public ObservableCollection<ImageModel> Images { get; }
    }
}