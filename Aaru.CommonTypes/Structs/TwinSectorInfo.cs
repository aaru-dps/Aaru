using System.Runtime.InteropServices;

namespace Aaru.CommonTypes.Structs;

public struct TwinSectorInfo
{
    /// <summary>
    /// Sector address as passed to the read/write functions.
    /// </summary>
    public ulong sectorAddress;
    /// <summary>
    /// Number of physical instances (>= 2).
    /// </summary>
    public uint  count;
    /// <summary>
    /// True if <see cref="sectorAddress"/> is a negative sector.
    /// </summary>
    public bool  negative;
}