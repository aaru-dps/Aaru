using System;

namespace Aaru.Gui.Models
{
    public class PluginModel
    {
        public string Name    { get; set; }
        public Guid   Uuid    { get; set; }
        public string Version { get; set; }
        public string Author  { get; set; }
    }
}