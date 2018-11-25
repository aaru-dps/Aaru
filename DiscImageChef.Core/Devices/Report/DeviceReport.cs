using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Report
{
    public partial class DeviceReport
    {
        Device dev;
        bool debug;

        public DeviceReport(Device device, bool debug)
        {
            this.dev = device;
            this.debug = debug;
        }
    }
}