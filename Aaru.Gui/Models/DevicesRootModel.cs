using System.Collections.ObjectModel;

namespace Aaru.Gui.Models
{
    public class DevicesRootModel : RootModel
    {
        public DevicesRootModel() => Devices = new ObservableCollection<DeviceModel>();

        public ObservableCollection<DeviceModel> Devices { get; }
    }
}