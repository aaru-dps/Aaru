using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscImageChef.Server.Models
{
    public class UsbVendor
    {
        public UsbVendor() { }

        public UsbVendor(ushort id, string vendor)
        {
            VendorId  = id;
            Vendor    = vendor;
            AddedWhen = ModifiedWhen = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }
        [Index(IsUnique = true)]
        public int VendorId { get;       set; }
        public string   Vendor    { get; set; }
        public DateTime AddedWhen { get; set; }
        [Index]
        public DateTime ModifiedWhen { get; set; }

        public virtual ICollection<UsbProduct> Products { get; set; }
    }
}