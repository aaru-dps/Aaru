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
}