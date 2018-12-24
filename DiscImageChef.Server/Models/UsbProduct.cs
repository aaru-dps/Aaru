using System;
using System.ComponentModel.DataAnnotations;

namespace DiscImageChef.Server.Models
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
        public int Id { get;        set; }
        public int ProductId { get; set; }

        public string   Product      { get; set; }
        public DateTime AddedWhen    { get; set; }
        public DateTime ModifiedWhen { get; set; }

        public         int       VendorId { get; set; }
        public virtual UsbVendor Vendor   { get; set; }
    }
}