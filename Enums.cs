namespace Aaru.Decryption;

public enum DvdCssKeyClass : byte
{
    DvdCssCppmOrCprm            = 0,
    RewritableSecurityServicesA = 1
}

public enum DvdCssKeyType
{
    Key1   = 0,
    Key2   = 1,
    BusKey = 2
}