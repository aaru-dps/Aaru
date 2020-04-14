using Aaru.CommonTypes.Enums;

namespace Aaru.Gui.Models
{
    public class MediaTagModel
    {
        public MediaTagType Tag     { get; set; }
        public byte[]       Data    { get; set; }
        public string       Decoded { get; set; }
        public string       Name    => Tag.ToString();
    }
}