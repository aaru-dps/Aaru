namespace DiscImageChef.CommonTypes.Metadata
{
    public class CdOffset
    {
        public string Manufacturer { get; set; }
        public string Model        { get; set; }
        public short  Offset       { get; set; }
        public int    Submissions  { get; set; }
        public float  Agreement    { get; set; }
    }
}