using Aaru.CommonTypes.Interfaces;

namespace Aaru.Gui.Models
{
    public class ImagePluginModel
    {
        public string         Name   => Plugin.Name;
        public IWritableImage Plugin { get; set; }
    }
}