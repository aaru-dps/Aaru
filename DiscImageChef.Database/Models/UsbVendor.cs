using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiscImageChef.Database.Models
{
    public class UsbVendor
    {
        public UsbVendor() { }

        public UsbVendor(ushort id, string vendor)
        {
            Id        = id;
            Vendor    = vendor;
            AddedWhen = ModifiedWhen = DateTime.UtcNow;
        }

        [Key]
        public ushort Id { get;             set; }
        public string   Vendor       { get; set; }
        public DateTime AddedWhen    { get; set; }
        public DateTime ModifiedWhen { get; set; }

        public virtual ICollection<UsbProduct> Products { get; set; }
    }
}