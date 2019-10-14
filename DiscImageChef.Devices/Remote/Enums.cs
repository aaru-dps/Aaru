namespace DiscImageChef.Devices.Remote
{
    public enum DicPacketType : sbyte
    {
        Nop = -1,
        Hello = 1,
        CommandListDevices = 2,
        ResponseListDevices = 3,
        CommandOpen = 4,
        CommandScsi = 5,
        ResponseScsi = 6,
        CommandAtaChs = 7,
        ResponseAtaChs = 8,
        CommandAtaLba28 = 9,
        ResponseAtaLba28 = 10,
        CommandAtaLba48 = 11,
        ResponseAtaLba48 = 12,
        CommandSdhci = 13,
        ResponseSdhci = 14,
        CommandGetType = 15,
        ResponseGetType = 16,
        CommandGetSdhciRegisters = 17,
        ResponseGetSdhciRegisters = 18,
        CommandGetUsbData = 19,
        ResponseGetUsbData = 20,
        CommandGetFireWireData = 21,
        ResponseGetFireWireData = 22,
        CommandGetPcmciaData = 23,
        ResponseGetPcmciaData = 24
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