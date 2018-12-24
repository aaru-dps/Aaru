using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscImageChef.Database.Models
{
    public class UsbProduct
    {
        public UsbProduct() { }

        public UsbProduct(ushort vendorId, ushort id, string product)
        {
            ProductId = id;
            Product   = product;
            AddedWhen = ModifiedWhen = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }
        [Index]
        public ushort ProductId { get;   set; }
        public string   Product   { get; set; }
        public DateTime AddedWhen { get; set; }
        [Index]
        public DateTime ModifiedWhen { get; set; }
        [Index]
        public ushort VendorId { get;          set; }
        public virtual UsbVendor Vendor { get; set; }
    }
}