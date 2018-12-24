using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscImageChef.Server.Models
{
    public class UsbProduct
    {
        public UsbProduct() { }

        public UsbProduct(UsbVendor vendor, ushort id, string product)
        {
            ProductId = id;
            Product   = product;
            AddedWhen = ModifiedWhen = DateTime.UtcNow;
            Vendor    = vendor;
        }

        [Key]
        public int Id { get; set; }
        [Index]
        public int ProductId { get;      set; }
        public string   Product   { get; set; }
        public DateTime AddedWhen { get; set; }
        [Index]
        public DateTime ModifiedWhen { get; set; }
        [Index]
        public int VendorId { get;             set; }
        public virtual UsbVendor Vendor { get; set; }
    }
}