using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Server.Models
{
    public class CompactDiscOffset : CdOffset
    {
        public CompactDiscOffset() { }

        public CompactDiscOffset(string manufacturer, string model, short offset, int submissions, float agreement)
        {
            Manufacturer = manufacturer;
            Model        = model;
            Offset       = offset;
            Submissions  = submissions;
            Agreement    = agreement;
            AddedWhen    = ModifiedWhen = DateTime.UtcNow;
        }

        public int      Id        { get; set; }
        public DateTime AddedWhen { get; set; }
        [Index]
        public DateTime ModifiedWhen { get;               set; }
        public virtual ICollection<Device> Devices { get; set; }
    }
}