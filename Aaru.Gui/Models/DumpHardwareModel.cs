namespace Aaru.Gui.Models
{
    public class DumpHardwareModel
    {
        public string Manufacturer { get; set; }

        public string Model { get; set; }

        public string Revision { get; set; }

        public string Serial { get; set; }

        public string SoftwareName    { get; set; }
        public string SoftwareVersion { get; set; }
        public string OperatingSystem { get; set; }
        public ulong  Start           { get; set; }
        public ulong  End             { get; set; }
    }
}