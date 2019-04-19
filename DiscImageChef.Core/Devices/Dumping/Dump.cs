namespace DiscImageChef.Core.Devices.Dumping
{
    public partial class Dump
    {
        public event UpdateStatusHandler   UpdateStatus;
        public event ErrorMessageHandler   ErrorMessage;
        public event ErrorMessageHandler   StoppingErrorMessage;
        public event UpdateProgressHandler UpdateProgress;
    }
}