namespace DiscImageChef.Devices.Remote
{
    public enum DicPacketType : sbyte
    {
        Nop = -1,
        Hello = 1,
        CommandListDevices = 2,
        ResponseListDevices = 3,
        CommandOpen = 4
    }

    public enum DicNopReason : byte
    {
        OutOfOrder = 0,
        NotImplemented = 1,
        NotRecognized = 2,
        ErrorListDevices = 3,
        OpenOk = 4,
        OpenError = 5
    }
}