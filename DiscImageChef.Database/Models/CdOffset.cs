using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscImageChef.Database.Models
{
    public class CdOffset : CommonTypes.Metadata.CdOffset
    {
        public CdOffset() { }

        public CdOffset(string manufacturer, string model, short offset, int submissions, float agreement)
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
        public DateTime ModifiedWhen { get; set; }
    }
}