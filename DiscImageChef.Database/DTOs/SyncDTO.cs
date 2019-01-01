using System.Collections.Generic;
using DiscImageChef.Database.Models;

namespace DiscImageChef.Database.DTOs
{
    public class SyncDTO
    {
        List<UsbProduct> UsbProducts;
        List<UsbVendor>  UsbVendors;
    }
}