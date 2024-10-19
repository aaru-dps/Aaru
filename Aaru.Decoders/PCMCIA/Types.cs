// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Types.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains PCMCIA tuple structures.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable MemberCanBeInternal

namespace Aaru.Decoders.PCMCIA;

/// <summary>Basic classure of a PCMCIA tuple</summary>
public class Tuple
{
    public TupleCodes Code;
    public byte[]     Data;
    public byte       Link;
}

/// <summary>Checksum tuple</summary>
public class ChecksumTuple
{
    /// <summary>Modulo-256 sum of region</summary>
    public byte Checksum;
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_CHECKSUM" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Length of region to be checksummed</summary>
    public ushort Length;
    /// <summary>Link to next tuple</summary>
    public byte Link;
    /// <summary>Offset to region to be checksummed</summary>
    public short Offset;
}

/// <summary>Indirect Access PC Card Memory</summary>
public class IndirectTuple
{
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_INDIRECT" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Link to next tuple</summary>
    public byte Link;
}

/// <summary>Link target tuple</summary>
public class LinkTargetTuple
{
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_LINKTARGET" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Link to next tuple</summary>
    public byte Link;
    /// <summary>'C''I''S' in ASCII</summary>
    public byte[] Tag;
}

/// <summary>16-bit PC Card Long Link Tuple</summary>
public class LongLinkTuple
{
    /// <summary>Target address</summary>
    public uint Address;
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_LONGLINK_A" /> or <see cref="TupleCodes.CISTPL_LONGLINK_C" /> or
    ///     <see cref="TupleCodes.CISTPL_LONGLINK_CB" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Link to next tuple</summary>
    public byte Link;
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ConfigurationAddress
{
    /// <summary>Target address</summary>
    public uint Address;
    /// <summary>Target address space, 0 = attribute, 1 = common</summary>
    public byte TargetAddressSpace;
}

/// <summary>Multiple function link tuple</summary>
public class MultipleFunctionLinkTuple
{
    /// <summary>Link to more configuration registers</summary>
    public ConfigurationAddress[] Addresses;
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_LONGLINK_MFC" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Link to next tuple</summary>
    public byte Link;
    /// <summary>How many functions follow</summary>
    public byte NumberFunctions;
}

public class NoLinkTuple
{
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_NO_LINK" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Link to next tuple</summary>
    public byte Link;
}

public class AlternateStringTuple
{
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_ALTSTR" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Link to next tuple</summary>
    public byte Link;
    /// <summary>
    ///     Array of strings. On memory they're preceded by an ISO Escape Code indicating codepage. Here they're stored as
    ///     Unicode, so no need for it.
    /// </summary>
    public string[] Strings;
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ExtendedDeviceSpeed
{
    /// <summary>Speed exponent</summary>
    public byte Exponent;
    /// <summary>Another extended follows</summary>
    public bool Extended;
    /// <summary>Speed mantisa</summary>
    public byte Mantissa;
}

public struct DeviceInfo
{
    /// <summary>Device type code</summary>
    public DeviceTypeCodes Type;
    /// <summary>Write protected</summary>
    public bool WPS;
    /// <summary>Speed code</summary>
    public DeviceSpeedCodes Speed;
    /// <summary>Extended speeds</summary>
    public ExtendedDeviceSpeed[] ExtendedSpeeds;
    /// <summary>Extended types</summary>
    public byte[] ExtendedTypes;
    /// <summary>Size in units - 1</summary>
    public byte Units;
    /// <summary>Code to define units unit</summary>
    public byte SizeCode;
}

public class DeviceTuple
{
    /// <summary><see cref="TupleCodes.CISTPL_DEVICE" /> or <see cref="TupleCodes.CISTPL_DEVICE_A" /></summary>
    public TupleCodes Code;
    /// <summary>Array of device information bytes</summary>
    public DeviceInfo[] Infos;
    /// <summary>Link to next tuple</summary>
    public byte Link;
}

public struct OtherConditionInfo
{
    /// <summary>True if another other condition info follows</summary>
    public bool Extended;
    /// <summary>Vcc used</summary>
    public byte VccUsed;
    /// <summary>Supports WAIT# signal</summary>
    public bool MWAIT;
}

public class OtherConditionTuple
{
    /// <summary><see cref="TupleCodes.CISTPL_DEVICE_OC" /> or <see cref="TupleCodes.CISTPL_DEVICE_OA" /></summary>
    public TupleCodes Code;
    /// <summary>Array of device information bytes</summary>
    public DeviceInfo[] Infos;
    /// <summary>Link to next tuple</summary>
    public byte Link;
    /// <summary>Array of other condition information bytes</summary>
    public OtherConditionInfo[] OtherConditionInfos;
}

public struct DeviceGeometry
{
    /// <summary>1 &lt;&lt; n-1 bytes, 2 = 16-bit PC Card, 3 = CardBus PC Card</summary>
    public byte CardInterface;
    /// <summary>
    ///     Erase block size in 1 &lt;&lt; n-1 increments of <see cref="CardInterface" /> wide accesses. If n == 4, and
    ///     <see cref="CardInterface" /> == 16, erase block size = 32 * 4 = 128 bytes
    /// </summary>
    public byte EraseBlockSize;
    /// <summary>
    ///     Read block size in 1 &lt;&lt; n-1 increments of <see cref="CardInterface" /> wide accesses. If n == 4, and
    ///     <see cref="CardInterface" /> == 16, read block size = 32 * 4 = 128 bytes
    /// </summary>
    public byte ReadBlockSize;
    /// <summary>
    ///     Write block size in 1 &lt;&lt; n-1 increments of <see cref="CardInterface" /> wide accesses. If n == 4, and
    ///     <see cref="CardInterface" /> == 16, write block size = 32 * 4 = 128 bytes
    /// </summary>
    public byte WriteBlockSize;
    /// <summary>
    ///     Device partitioning in granularity of 1 &lt;&lt; n-1 erase blocks If n == 4, and erase block is 128 bytes,
    ///     partitions must be aligned to 32 erase block, or 4096 bytes
    /// </summary>
    public byte Partitions;
    /// <summary>Card employs a multiple of 1 &lt;&lt; n-1 times interleaving the entire memory arrays</summary>
    public byte Interleaving;
}

[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public class DeviceGeometryTuple
{
    /// <summary><see cref="TupleCodes.CISTPL_DEVICEGEO" /> or <see cref="TupleCodes.CISTPL_DEVICEGEO_A" /></summary>
    public TupleCodes Code;
    /// <summary>Array of device geometries</summary>
    public DeviceGeometry[] Geometries;
    /// <summary>Link to next tuple</summary>
    public byte Link;
}

public class FunctionIdentificationTuple
{
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_FUNCID" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Function code</summary>
    public FunctionCodes Function;
    /// <summary>Link to next tuple</summary>
    public byte Link;
    /// <summary>Device wants to be part of power-on-self-test</summary>
    public bool POST;
    /// <summary>Device contains boot ROM</summary>
    public bool ROM;
}

[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public class ManufacturerIdentificationTuple
{
    /// <summary>Card ID</summary>
    public ushort CardID;
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_MANFID" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Link to next tuple</summary>
    public byte Link;
    /// <summary>Manufacturer ID</summary>
    public ushort ManufacturerID;
}

[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public class Level1VersionTuple
{
    /// <summary>Additional information strings</summary>
    public string[] AdditionalInformation;
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_VERS_1" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Link to next tuple</summary>
    public byte Link;
    /// <summary>Major version of standard compliance</summary>
    public byte MajorVersion;
    /// <summary>Manufacturer string</summary>
    public string Manufacturer;
    /// <summary>Minor version of standard compliance</summary>
    public byte MinorVersion;
    /// <summary>Product string</summary>
    public string Product;
}

public class Level2VersionTuple
{
    /// <summary>Address of first data byte</summary>
    public ushort Address;
    /// <summary>Number of copies of CIS present</summary>
    public byte CISCopies;
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_VERS_2" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Level of compliance</summary>
    public byte Compliance;
    /// <summary>Informational message about the card</summary>
    public string Information;
    /// <summary>Link to next tuple</summary>
    public byte Link;
    /// <summary>Vendor of software that formatted the card</summary>
    public string OEM;
    /// <summary>Version of this classure</summary>
    public byte StructureVersion;
    /// <summary>Vendor-specific byte</summary>
    public byte VendorSpecific1;
    /// <summary>Vendor-specific byte</summary>
    public byte VendorSpecific2;
}

public class GeometryTuple
{
    /// <summary>
    ///     <see cref="TupleCodes.CISTPL_GEOMETRY" />
    /// </summary>
    public TupleCodes Code;
    /// <summary>Cylinders</summary>
    public ushort Cylinders;
    /// <summary>Link to next tuple</summary>
    public byte Link;
    /// <summary>Sectors per track</summary>
    public byte SectorsPerTrack;
    /// <summary>Tracks per cylinder</summary>
    public byte TracksPerCylinder;
}