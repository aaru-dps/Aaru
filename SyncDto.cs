using System.Collections.Generic;

namespace Aaru.Dto
{
    public class SyncDto
    {
        public List<UsbVendorDto>  UsbVendors  { get; set; }
        public List<UsbProductDto> UsbProducts { get; set; }
        public List<CdOffsetDto>   Offsets     { get; set; }
        public List<DeviceDto>     Devices     { get; set; }
    }
}