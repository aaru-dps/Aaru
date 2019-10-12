namespace DiscImageChef.Devices.Remote
{
    public enum DicPacketType : byte
    {
        Hello = 1,
        CommandListDevices = 2,
        ResponseListDevices = 3
    }
}