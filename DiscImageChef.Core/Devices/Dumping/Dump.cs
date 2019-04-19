namespace DiscImageChef.Core.Devices.Dumping
{
    public partial class Dump
    {
        public event EndProgressHandler    EndProgress;
        public event InitProgressHandler   InitProgress;
        public event UpdateStatusHandler   UpdateStatus;
        public event ErrorMessageHandler   ErrorMessage;
        public event ErrorMessageHandler   StoppingErrorMessage;
        public event UpdateProgressHandler UpdateProgress;
        public event PulseProgressHandler  PulseProgress;
    }
}