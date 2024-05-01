// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Features.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MMC feature structures.
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


using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Helpers;
using Aaru.Localization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

namespace Aaru.Decoders.SCSI.MMC;

/// <summary>MMC Feature enumeration</summary>
public enum FeatureNumber : ushort
{
    /// <summary>Lists all profiles</summary>
    ProfileList = 0x0000,
    /// <summary>Mandatory behaviour</summary>
    Core = 0x0001,
    /// <summary>Operational changes</summary>
    Morphing = 0x0002,
    /// <summary>Removable medium</summary>
    Removable = 0x0003,
    /// <summary>Ability to control write protection status</summary>
    WriteProtect = 0x0004,
    /// <summary>Ability to read sectors with random addressing</summary>
    RandomRead = 0x0010,
    /// <summary>Reads on OSTA Multi-Read</summary>
    MultiRead = 0x001D,
    /// <summary>Able to read CD structures</summary>
    CDRead = 0x001E,
    /// <summary>Able to read DVD structures</summary>
    DVDRead = 0x001F,
    /// <summary>Ability to write sectors with random addressing</summary>
    RandomWrite = 0x0020,
    /// <summary>Ability to sequentially write</summary>
    IncrementalWrite = 0x0021,
    /// <summary>Support for media that requires erase before write</summary>
    SectorErasable = 0x0022,
    /// <summary>Supports formatting media</summary>
    Formattable = 0x0023,
    /// <summary>Ability to provide defect-free space</summary>
    HardwareDefectMgmt = 0x0024,
    /// <summary>Supports for write-once media in random order</summary>
    WriteOnce = 0x0025,
    /// <summary>Supports for media that shall be written from blocking boundaries</summary>
    RestrictedOverwrite = 0x0026,
    /// <summary>Supports high speed CD-RW</summary>
    CDRWCAV = 0x0027,
    /// <summary>Read and optionally write MRW</summary>
    MRW = 0x0028,
    /// <summary>Ability to control RECOVERED ERROR reporting</summary>
    EnDefectReport = 0x0029,
    /// <summary>Ability to recognize, read and optionally write DVD+RW</summary>
    DVDRWPlus = 0x002A,
    /// <summary>Ability to read DVD+R</summary>
    DVDRPlus = 0x002B,
    RigidOverWrite = 0x002C,
    /// <summary>Ability to write CD in Track-at-Once</summary>
    CDTAO = 0x002D,
    /// <summary>Ability to write CD in Session-at-Once or RAW</summary>
    CDMastering = 0x002E,
    /// <summary>Ability to write DVD structures</summary>
    DVDRWrite = 0x002F,
    /// <summary>Ability to read DDCD</summary>
    DDCD = 0x0030,
    /// <summary>Ability to write DDCD-R</summary>
    DDCDR = 0x0031,
    /// <summary>Ability to write DDCD-RW</summary>
    DDCDRW = 0x0032,
    /// <summary>Ability to record in layer jump mode</summary>
    LayerJump = 0x0033,
    /// <summary>Ability to perform Layer Jump recording on Rigid Restricted Overwrite</summary>
    LJRigid = 0x0034,
    /// <summary>Ability to stop the long immediate operation</summary>
    StopLong = 0x0035,
    /// <summary>Ability to report CD-RW media sub-types supported for write</summary>
    CDRWMediaWrite = 0x0037,
    /// <summary>Logical block overwrite service on BD-R formatted as SRM+POW</summary>
    BDRPOW = 0x0038,
    /// <summary>Ability to read DVD+RW DL</summary>
    DVDRWDLPlus = 0x003A,
    /// <summary>Ability to read DVD+R DL</summary>
    DVDRDLPlus = 0x003B,
    /// <summary>Ability to read BD discs</summary>
    BDRead = 0x0040,
    /// <summary>Ability to write BD discs</summary>
    BDWrite = 0x0041,
    /// <summary>Timely, Safe Recording</summary>
    TSR = 0x0042,
    /// <summary>Ability to read HD DVD</summary>
    HDDVDRead = 0x0050,
    /// <summary>Ability to write HD DVD</summary>
    HDDVDWrite = 0x0051,
    /// <summary>Ability to write HD DVD-RW fragmented</summary>
    HDDVDRWFragment = 0x0052,
    /// <summary>Supports some Hybrid Discs</summary>
    Hybrid = 0x0080,
    /// <summary>Host and device directed power management</summary>
    PowerMgmt = 0x0100,
    /// <summary>Supports S.M.A.R.T.</summary>
    SMART = 0x0101,
    /// <summary>Single machanism multiple disc changer</summary>
    Changer = 0x0102,
    /// <summary>Ability to play CD audio to an analogue output</summary>
    CDAudioExt = 0x0103,
    /// <summary>Ability to accept new microcode</summary>
    MicrocodeUpgrade = 0x0104,
    /// <summary>Ability to respond to all commands within a specific time</summary>
    Timeout = 0x0105,
    /// <summary>Supports DVD CSS/CPPM</summary>
    CSS = 0x0106,
    /// <summary>Ability to read and write using host requested performance parameters</summary>
    RTS = 0x0107,
    /// <summary>Drive has a unique identifier</summary>
    DriveSerial = 0x0108,
    /// <summary>Ability to return unique Media Serial Number</summary>
    MediaSerial = 0x0109,
    /// <summary>Ability to read and/or write DCBs</summary>
    DCBs = 0x010A,
    /// <summary>Supports DVD CPRM</summary>
    CPRM = 0x010B,
    /// <summary>Firmware creation date report</summary>
    FirmwareInfo = 0x010C,
    /// <summary>Ability to decode and optionally encode AACS</summary>
    AACS = 0x010D,
    /// <summary>Ability to perform DVD CSS managed recording</summary>
    CSSManagedRec = 0x010E,
    /// <summary>Ability to decode and optionally encode VCPS</summary>
    VCPS = 0x0110,
    /// <summary>Supports SecurDisc</summary>
    SecurDisc = 0x0113,
    /// <summary>TCG Optical Security Subsystem Class</summary>
    OSSC = 0x0142
}

/// <summary>MMC Profile enumeration</summary>
public enum ProfileNumber : ushort
{
    /// <summary>Not to use</summary>
    Reserved = 0x0000,
    /// <summary>Non-removable disk profile</summary>
    NonRemovable = 0x0001,
    /// <summary>Rewritable with removable media</summary>
    Removable = 0x0002,
    /// <summary>Magneto-Optical with sector erase</summary>
    MOErasable = 0x0003,
    /// <summary>Optical write once</summary>
    OpticalWORM = 0x0004,
    /// <summary>Advance Storage - Magneto-Optical</summary>
    ASMO = 0x0005,
    /// <summary>Read-only Compact Disc</summary>
    CDROM = 0x0008,
    /// <summary>Write-once Compact Disc</summary>
    CDR = 0x0009,
    /// <summary>Re-writable Compact Disc</summary>
    CDRW = 0x000A,
    /// <summary>Read-only DVD</summary>
    DVDROM = 0x0010,
    /// <summary>Write-once sequentially recorded DVD-R</summary>
    DVDRSeq = 0x0011,
    /// <summary>DVD-RAM</summary>
    DVDRAM = 0x0012,
    /// <summary>Restricted overwrite DVD-RW</summary>
    DVDRWRes = 0x0013,
    /// <summary>Sequential recording DVD-RW</summary>
    DVDRWSeq = 0x0014,
    /// <summary>Sequential recording DVD-R DL</summary>
    DVDRDLSeq = 0x0015,
    /// <summary>Layer jump recording DVD-R DL</summary>
    DVDRDLJump = 0x0016,
    /// <summary>DVD-RW DL</summary>
    DVDRWDL = 0x0017,
    /// <summary>DVD-Download</summary>
    DVDDownload = 0x0018,
    /// <summary>DVD+RW</summary>
    DVDRWPlus = 0x001A,
    /// <summary>DVD+R</summary>
    DVDRPlus = 0x001B,
    /// <summary>DDCD-ROM</summary>
    DDCDROM = 0x0020,
    /// <summary>DDCD-R</summary>
    DDCDR = 0x0021,
    /// <summary>DDCD-RW</summary>
    DDCDRW = 0x0022,
    /// <summary>DVD+RW DL</summary>
    DVDRWDLPlus = 0x002A,
    /// <summary>DVD+R DL</summary>
    DVDRDLPlus = 0x002B,
    /// <summary>BD-ROM</summary>
    BDROM = 0x0040,
    /// <summary>BD-R SRM</summary>
    BDRSeq = 0x0041,
    /// <summary>BD-R RRM</summary>
    BDRRdm = 0x0042,
    /// <summary>BD-RE</summary>
    BDRE = 0x0043,
    /// <summary>HD DVD-ROM</summary>
    HDDVDROM = 0x0050,
    /// <summary>HD DVD-R</summary>
    HDDVDR = 0x0051,
    /// <summary>HD DVD-RAM</summary>
    HDDVDRAM = 0x0052,
    /// <summary>HD DVD-RW</summary>
    HDDVDRW = 0x0053,
    /// <summary>HD DVD-R DL</summary>
    HDDVDRDL = 0x0058,
    /// <summary>HD DVD-RW DL</summary>
    HDDVDRWDL = 0x005A,
    /// <summary>HDBurn CD-ROM</summary>
    HDBURNROM = 0x0080,
    /// <summary>HDBurn CD-R</summary>
    HDBURNR = 0x0081,
    /// <summary>HDBurn CD-RW</summary>
    HDBURNRW = 0x0082,
    /// <summary>Drive does not conform to any profiles</summary>
    Unconforming = 0xFFFF
}

public struct Profile
{
    public ProfileNumber Number;
    public bool          Current;
}

/// <summary>Profile List Feature (0000h)</summary>
public struct Feature_0000
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>All supported profiles</summary>
    public Profile[] Profiles;
}

/// <summary>Core Feature (0001h)</summary>
public struct Feature_0001
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Currently in-use physical interface standard</summary>
    public PhysicalInterfaces PhysicalInterfaceStandard;
    /// <summary>Supports EVPD, Page Code and 16-bit Allocation Length as defined in SPC-3</summary>
    public bool INQ2;
    /// <summary>Supports Device Busy Event</summary>
    public bool DBE;
}

/// <summary>Morphing Feature (0002h)</summary>
public struct Feature_0002
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports Operational Change Request/Nofitication Class Events of GET EVENT/STATUS NOTIFICATION</summary>
    public bool OCEvent;
    /// <summary>Supports asynchronous GET EVENT/STATUS NOTIFICATION</summary>
    public bool Async;
}

/// <summary>Removable Medium Feature (0003h)</summary>
public struct Feature_0003
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Mechanism type</summary>
    public byte LoadingMechanismType;
    /// <summary>Drive is able to load the medium</summary>
    public bool Load;
    /// <summary>Device can eject medium</summary>
    public bool Eject;
    /// <summary>Device starts in medium ejection/insertion allow</summary>
    public bool PreventJumper;
    /// <summary>Reports Device Busy Class events during medium loading/unloading</summary>
    public bool DBML;
    /// <summary>Medium is currently locked</summary>
    public bool Lock;
}

/// <summary>Write Protect Feature (0004h)</summary>
public struct Feature_0004
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Drive can read/write Disc Write Protect PAC on BD-R/-RE media</summary>
    public bool DWP;
    /// <summary>Supports reading/writing Write Inhibit DCB on DVD+RW media.</summary>
    public bool WDCB;
    /// <summary>Supports PWP status</summary>
    public bool SPWP;
    /// <summary>Supports SWPP bit of mode page 1Dh</summary>
    public bool SSWPP;
}

/// <summary>Random Readable Feature (0010h)</summary>
public struct Feature_0010
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Bytes per logical block</summary>
    public uint LogicalBlockSize;
    /// <summary>Number of logical blocks per device readable unit</summary>
    public ushort Blocking;
    /// <summary>Read/Write Error Recovery page is present</summary>
    public bool PP;
}

/// <summary>Multi-Read Feature (001Dh)</summary>
public struct Feature_001D
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

/// <summary>CD Read Feature (001Eh)</summary>
public struct Feature_001E
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports DAP bit in READ CD and READ CD MSF</summary>
    public bool DAP;
    /// <summary>Supports C2 Error Pointers</summary>
    public bool C2;
    /// <summary>Can read CD-Text with READ TOC/PMA/ATIP</summary>
    public bool CDText;
}

/// <summary>DVD Read Feature (001Fh)</summary>
public struct Feature_001F
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Compliant with DVD Multi Drive Read-only specifications</summary>
    public bool MULTI110;
    /// <summary>Supports reading all DVD-RW DL</summary>
    public bool DualRW;
    /// <summary>Supports reading all DVD-R DL including remapping</summary>
    public bool DualR;
}

/// <summary>Random Writable Feature (0020h)</summary>
public struct Feature_0020
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Last logical block address</summary>
    public uint LastLBA;
    /// <summary>Bytes per logical block</summary>
    public uint LogicalBlockSize;
    /// <summary>Number of logical blocks per device readable unit</summary>
    public ushort Blocking;
    /// <summary>Read/Write Error Recovery page is present</summary>
    public bool PP;
}

/// <summary>Incremental Streaming Writable Feature (0021h)</summary>
public struct Feature_0021
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Bitmask of supported data types</summary>
    public ushort DataTypeSupported;
    /// <summary>Can report Track Resources Information of READ DISC INFORMATION</summary>
    public bool TRIO;
    /// <summary>Supports Address Mode in RESERVE TRACK</summary>
    public bool ARSV;
    /// <summary>Zero loss linking</summary>
    public bool BUF;
    /// <summary>Logical blocks per link</summary>
    public byte[] LinkSizes;
}

/// <summary>Sector Erasable Feature (0022h)</summary>
public struct Feature_0022
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

/// <summary>Formattable Feature (0023h)</summary>
public struct Feature_0023
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports formatting BD-RE without spare area</summary>
    public bool RENoSA;
    /// <summary>Supports expansion of the spare area on BD-RE</summary>
    public bool Expand;
    /// <summary>Supports FORMAT type 30h sub-type 11b</summary>
    public bool QCert;
    /// <summary>Supports FORMAT type 30h sub-type 10b</summary>
    public bool Cert;
    /// <summary>Supports FORMAT type 18h</summary>
    public bool FRF;
    /// <summary>Supports FORMAT type 00h/32h sub-type 10b on BD-R</summary>
    public bool RRM;
}

/// <summary>Hardware Defect Management Feature (0024h)</summary>
public struct Feature_0024
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports READ DISC STRUCTURE with Format Code 0Ah (Spare Area Information)</summary>
    public bool SSA;
}

/// <summary>Write Once Feature (0025h)</summary>
public struct Feature_0025
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Bytes per logical block</summary>
    public uint LogicalBlockSize;
    /// <summary>Number of logical blocks per device readable unit</summary>
    public ushort Blocking;
    /// <summary>Read/Write Error Recovery page is present</summary>
    public bool PP;
}

/// <summary>Restricted Overwrite Feature (0026h)</summary>
public struct Feature_0026
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

/// <summary>CD-RW CAV Write Feature (0027h)</summary>
public struct Feature_0027
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

/// <summary>MRW Feature (0028h)</summary>
public struct Feature_0028
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Can read DVD+MRW discs</summary>
    public bool DVDPRead;
    /// <summary>Can write DVD+MRW discs</summary>
    public bool DVDPWrite;
    /// <summary>Can format and write to CD-MRW discs</summary>
    public bool Write;
}

/// <summary>Enhanced Defect Reporting Feature (0029h)</summary>
public struct Feature_0029
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports DRT-DM</summary>
    public bool DRTDM;
    /// <summary>Maximum number of DBI cache zones device can handle separately</summary>
    public byte DBICacheZones;
    /// <summary>Number of entries in worst case to case DBI overflow</summary>
    public ushort Entries;
}

/// <summary>DVD+RW Feature (002Ah)</summary>
public struct Feature_002A
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Can format DVD+RW discs</summary>
    public bool Write;
    /// <summary>FORMAT UNIT supports quick start formatting</summary>
    public bool QuickStart;
    /// <summary>Drive only supports read compatibility stop</summary>
    public bool CloseOnly;
}

/// <summary>DVD+R Feature (002Bh)</summary>
public struct Feature_002B
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Can write DVD+R</summary>
    public bool Write;
}

/// <summary>Rigid Restricted Overwrite Feature (002Ch)</summary>
public struct Feature_002C
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Can generate Defect Status Data during formatting</summary>
    public bool DSDG;
    /// <summary>Can read Defect Status Data recorded on medium</summary>
    public bool DSDR;
    /// <summary>Supports writing on an intermediate state Session and quick formatting</summary>
    public bool Intermediate;
    /// <summary>Supports BLANK command types 00h and 01h</summary>
    public bool Blank;
}

/// <summary>CD Track at Once Feature (002Dh)</summary>
public struct Feature_002D
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports zero loss linking</summary>
    public bool BUF;
    /// <summary>Supports writing R-W subchannels in raw mode</summary>
    public bool RWRaw;
    /// <summary>Supports writing R-W subchannels in packed mode</summary>
    public bool RWPack;
    /// <summary>Can perform test writes</summary>
    public bool TestWrite;
    /// <summary>Supports overwriting a TAO track with another</summary>
    public bool CDRW;
    /// <summary>Can write R-W subchannels with user provided data</summary>
    public bool RWSubchannel;
    /// <summary>Bitmask of supported data types</summary>
    public ushort DataTypeSupported;
}

/// <summary>CD Mastering (Session at Once) Feature (002Eh)</summary>
public struct Feature_002E
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports zero loss linking</summary>
    public bool BUF;
    /// <summary>Can write in Session at Once</summary>
    public bool SAO;
    /// <summary>Can write multi-session in RAW</summary>
    public bool RAWMS;
    /// <summary>Can write in RAW</summary>
    public bool RAW;
    /// <summary>Can perform test writes</summary>
    public bool TestWrite;
    /// <summary>Can overwrite previously recorded data</summary>
    public bool CDRW;
    /// <summary>Can write R-W subchannels with user provided data</summary>
    public bool RW;
    /// <summary>Maximum length of a Cue Sheet for Session at Once</summary>
    public uint MaxCueSheet;
}

/// <summary>DVD-R/-RW Write Feature (002Fh)</summary>
public struct Feature_002F
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Buffer Under-run protection</summary>
    public bool BUF;
    /// <summary>Supports writing DVD-R DL</summary>
    public bool RDL;
    /// <summary>Test write</summary>
    public bool TestWrite;
    /// <summary>Can write and erase DVD-RW</summary>
    public bool DVDRW;
}

/// <summary>Double Density CD Read Feature (0030h)</summary>
public struct Feature_0030
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

/// <summary>Double Density CD-R Write Feature (0031h)</summary>
public struct Feature_0031
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Test write</summary>
    public bool TestWrite;
}

/// <summary>Double Density CD-RW Write Feature (0032h)</summary>
public struct Feature_0032
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports quick formatting</summary>
    public bool Intermediate;
    /// <summary>Supports BLANK command</summary>
    public bool Blank;
}

/// <summary>Layer Jump Recording Feature (0033h)</summary>
public struct Feature_0033
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    public byte[] LinkSizes;
}

/// <summary>Stop Long Operation Feature (0035h)</summary>
public struct Feature_0035
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

/// <summary>CD-RW Media Write Support Feature (0037h)</summary>
public struct Feature_0037
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Bitmask of supported CD-RW media sub-types</summary>
    public byte SubtypeSupport;
}

/// <summary>BD-R Pseudo-Overwrite (POW) Feature (0038h)</summary>
public struct Feature_0038
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

/// <summary>DVD+RW Dual Layer Feature (003Ah)</summary>
public struct Feature_003A
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Can format DVD+RW DL discs</summary>
    public bool Write;
    /// <summary>FORMAT UNIT supports quick start formatting</summary>
    public bool QuickStart;
    /// <summary>Drive only supports read compatibility stop</summary>
    public bool CloseOnly;
}

/// <summary>DVD+R Dual Layer Feature (003Bh)</summary>
public struct Feature_003B
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Can format DVD+R DL discs</summary>
    public bool Write;
    /// <summary>FORMAT UNIT supports quick start formatting</summary>
    public bool QuickStart;
    /// <summary>Drive only supports read compatibility stop</summary>
    public bool CloseOnly;
}

/// <summary>BD Read Feature (0040h)</summary>
public struct Feature_0040
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Can read BCA</summary>
    public bool BCA;
    /// <summary>Supports reading BD-RE Ver.2</summary>
    public bool RE2;
    /// <summary>Supports reading BD-RE Ver.1</summary>
    public bool RE1;
    /// <summary>Obsolete</summary>
    public bool OldRE;
    /// <summary>Supports reading BD-R Ver.1</summary>
    public bool R;
    /// <summary>Obsolete</summary>
    public bool OldR;
    /// <summary>Supports reading BD-ROM Ver.1</summary>
    public bool ROM;
    /// <summary>Obsolete</summary>
    public bool OldROM;
}

/// <summary>BD Write Feature (0041h)</summary>
public struct Feature_0041
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports verify not required</summary>
    public bool SVNR;
    /// <summary>Supports writing BD-RE Ver.2</summary>
    public bool RE2;
    /// <summary>Supports writing BD-RE Ver.1</summary>
    public bool RE1;
    /// <summary>Obsolete</summary>
    public bool OldRE;
    /// <summary>Supports writing BD-R Ver.1</summary>
    public bool R;
    /// <summary>Obsolete</summary>
    public bool OldR;
}

/// <summary>TSR Feature (0042h)</summary>
public struct Feature_0042
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

/// <summary>HD DVD Read Feature (0050h)</summary>
public struct Feature_0050
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Can read HD DVD-R</summary>
    public bool HDDVDR;
    /// <summary>Can read HD DVD-RAM</summary>
    public bool HDDVDRAM;
}

/// <summary>HD DVD Write Feature (0051h)</summary>
public struct Feature_0051
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Can write HD DVD-R</summary>
    public bool HDDVDR;
    /// <summary>Can write HD DVD-RAM</summary>
    public bool HDDVDRAM;
}

/// <summary>Hybrid Disc Feature (0080h)</summary>
public struct Feature_0080
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Reset immunity</summary>
    public bool RI;
}

/// <summary>Power Management Feature (0100h)</summary>
public struct Feature_0100
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

/// <summary>S.M.A.R.T. Feature (0101h)</summary>
public struct Feature_0101
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Mode Page 1Ch is present</summary>
    public bool PP;
}

/// <summary>Embedded Changer Feature (0102h)</summary>
public struct Feature_0102
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Side change capable</summary>
    public bool SCC;
    /// <summary>Supports Disc Present</summary>
    public bool SDP;
    /// <summary>Number of slots - 1</summary>
    public byte HighestSlotNumber;
}

/// <summary>CD Audio External Play Feature (0103h)</summary>
public struct Feature_0103
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports SCAN command</summary>
    public bool Scan;
    /// <summary>Separate Channel Mute</summary>
    public bool SCM;
    /// <summary>Separate Volume</summary>
    public bool SV;
    /// <summary>Number of volume levels</summary>
    public ushort VolumeLevels;
}

/// <summary>Microcode Upgrade Feature (0104h)</summary>
public struct Feature_0104
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports validating 5-bit mode field of READ BUFFER and WRITE BUFFER commands.</summary>
    public bool M5;
}

/// <summary>Time-Out Feature (0105h)</summary>
public struct Feature_0105
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports G3Enable bit and Group3 Timeout field in Mode Page 1Dh</summary>
    public bool Group3;
    /// <summary>Indicates a unit of block length, in sectors, corresponding to increase a unit of Group 3 time unit</summary>
    public ushort UnitLength;
}

/// <summary>DVD-CSS Feature (0106h)</summary>
public struct Feature_0106
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>CSS version</summary>
    public byte CSSVersion;
}

/// <summary>Real Time Streaming Feature (0107h)</summary>
public struct Feature_0107
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports Set Minimum Performance bit in SET STREAMING</summary>
    public bool SMP;
    /// <summary>Supports READ BUFFER CAPACITY with block bit set</summary>
    public bool RBCB;
    /// <summary>Supports SET CD SPEED</summary>
    public bool SCS;
    /// <summary>Has Mode Page 2Ah with Speed Performance Descriptors</summary>
    public bool MP2A;
    /// <summary>Supports type 03h of GET PERFORMANCE</summary>
    public bool WSPD;
    /// <summary>Supports stream recording</summary>
    public bool SW;
}

/// <summary>Drive serial number (0108h)</summary>
public struct Feature_0108
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Drive serial number</summary>
    public string Serial;
}

/// <summary>Media Serial Number Feature (0109h)</summary>
public struct Feature_0109
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

/// <summary>Disc Control Blocks Feature (010Ah)</summary>
public struct Feature_010A
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    public uint[] DCBs;
}

/// <summary>DVD CPRM Feature (010Bh)</summary>
public struct Feature_010B
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>CPRM version</summary>
    public byte CPRMVersion;
}

/// <summary>Firmware Information Feature (010Ch)</summary>
public struct Feature_010C
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    public ushort Century;
    public ushort Year;
    public ushort Month;
    public ushort Day;
    public ushort Hour;
    public ushort Minute;
    public ushort Second;
}

/// <summary>AACS Feature (010Dh)</summary>
public struct Feature_010D
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Drive supports reading drive certificate</summary>
    public bool RDC;
    /// <summary>Drive can read media key block of CPRM</summary>
    public bool RMC;
    /// <summary>Drive can write bus encrypted blocks</summary>
    public bool WBE;
    /// <summary>Drive supports bus encryption</summary>
    public bool BEC;
    /// <summary>Drive supports generating the binding nonce</summary>
    public bool BNG;
    /// <summary>Blocks required to store the binding nonce for the media</summary>
    public byte BindNonceBlocks;
    /// <summary>Maximum number of AGIDs supported concurrently</summary>
    public byte AGIDs;
    /// <summary>AACS version</summary>
    public byte AACSVersion;
}

/// <summary>DVD CSS Managed Recording Feature (010Eh)</summary>
public struct Feature_010E
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Maximum number of Scramble Extent information entries in a single SEND DISC STRUCTURE</summary>
    public byte MaxScrambleExtent;
}

/// <summary>SecurDisc Feature (0113h)</summary>
public struct Feature_0113
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

/// <summary>OSSC Feature (0142h)</summary>
public struct Feature_0142
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
    /// <summary>Supports PSA updates on write-once media</summary>
    public bool PSAU;
    /// <summary>Supports linked OSPBs</summary>
    public bool LOSPB;
    /// <summary>Restricted to recording only OSSC disc format</summary>
    public bool ME;
    public ushort[] Profiles;
}

/// <summary>VCPS Feature (0110h)</summary>
public struct Feature_0110
{
    /// <summary>Feature version</summary>
    public byte Version;
    /// <summary>Feature is persistent</summary>
    public bool Persistent;
    /// <summary>Feature is currently in use</summary>
    public bool Current;
}

public static class Features
{
    public static Feature_0000? Decode_0000(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0000) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0000();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        var           offset       = 4;
        List<Profile> listProfiles = new();

        while(offset < feature.Length)
        {
            var prof = new Profile
            {
                Number = (ProfileNumber)((feature[offset] << 8) + feature[offset + 1])
            };

            prof.Current |= (feature[offset + 2] & 0x01) == 0x01;
            listProfiles.Add(prof);
            offset += 4;
        }

        decoded.Profiles = listProfiles.ToArray();

        return decoded;
    }

    public static Feature_0001? Decode_0001(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0001) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0001();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.PhysicalInterfaceStandard =
            (PhysicalInterfaces)((feature[4] << 24) + (feature[5] << 16) + (feature[6] << 8) + feature[7]);

        if(decoded.Version >= 1 && feature.Length >= 12) decoded.DBE |= (feature[8] & 0x01) == 0x01;

        if(decoded.Version >= 2 && feature.Length >= 12) decoded.INQ2 |= (feature[8] & 0x02) == 0x02;

        return decoded;
    }

    public static Feature_0002? Decode_0002(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0002) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0002();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.Async |= (feature[4] & 0x01) == 0x01;

        if(decoded.Version >= 1) decoded.OCEvent |= (feature[4] & 0x02) == 0x02;

        return decoded;
    }

    public static Feature_0003? Decode_0003(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0003) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0003();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.LoadingMechanismType =  (byte)((feature[4] & 0xE0) >> 5);
        decoded.Eject                |= (feature[4] & 0x08) == 0x08;
        decoded.PreventJumper        |= (feature[4] & 0x04) == 0x04;
        decoded.Lock                 |= (feature[4] & 0x01) == 0x01;

        if(decoded.Version < 2) return decoded;

        decoded.Load |= (feature[4] & 0x10) == 0x10;
        decoded.DBML |= (feature[4] & 0x02) == 0x02;

        return decoded;
    }

    public static Feature_0004? Decode_0004(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0004) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0004();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.SPWP  |= (feature[4] & 0x02) == 0x02;
        decoded.SSWPP |= (feature[4] & 0x01) == 0x01;

        if(decoded.Version >= 1) decoded.WDCB |= (feature[4] & 0x04) == 0x04;

        if(decoded.Version >= 2) decoded.DWP |= (feature[4] & 0x08) == 0x08;

        return decoded;
    }

    public static Feature_0010? Decode_0010(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 12) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0010) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0010();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.LogicalBlockSize = (uint)((feature[4] << 24) + (feature[5] << 16) + (feature[6] << 8) + feature[7]);

        decoded.Blocking = (ushort)((feature[8] << 8) + feature[9]);

        decoded.PP |= (feature[10] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_001D? Decode_001D(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x001D) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_001D();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_001E? Decode_001E(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x001E) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_001E();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        if(decoded.Version >= 1)
        {
            decoded.C2     |= (feature[4] & 0x02) == 0x02;
            decoded.CDText |= (feature[4] & 0x01) == 0x01;
        }

        if(decoded.Version >= 2) decoded.DAP |= (feature[4] & 0x80) == 0x80;

        return decoded;
    }

    public static Feature_001F? Decode_001F(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x001F) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_001F();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        if(decoded.Version >= 2 && feature.Length >= 8)
        {
            decoded.MULTI110 |= (feature[4] & 0x01) == 0x01;
            decoded.DualR    |= (feature[6] & 0x01) == 0x01;
        }

        // TODO: Check this
        if(decoded.Version >= 2 && feature.Length >= 8) decoded.DualRW |= (feature[6] & 0x02) == 0x02;

        return decoded;
    }

    public static Feature_0020? Decode_0020(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 16) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0020) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0020();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        if(decoded.Version < 1) return decoded;

        decoded.LastLBA = (uint)((feature[4] << 24) + (feature[5] << 16) + (feature[6] << 8) + feature[7]);

        decoded.LogicalBlockSize = (uint)((feature[8] << 24) + (feature[9] << 16) + (feature[10] << 8) + feature[11]);

        decoded.Blocking =  (ushort)((feature[12] << 8) + feature[13]);
        decoded.PP       |= (feature[14] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_0021? Decode_0021(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0021) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0021();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        if(decoded.Version >= 1)
        {
            decoded.DataTypeSupported =  (ushort)((feature[4] << 8) + feature[5]);
            decoded.BUF               |= (feature[6] & 0x01) == 0x01;
            decoded.LinkSizes         =  new byte[feature[7]];

            if(feature.Length > feature[7] + 8) Array.Copy(feature, 8, decoded.LinkSizes, 0, feature[7]);
        }

        if(decoded.Version < 3) return decoded;

        decoded.TRIO |= (feature[6] & 0x04) == 0x04;
        decoded.ARSV |= (feature[6] & 0x02) == 0x02;

        return decoded;
    }

    public static Feature_0022? Decode_0022(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0022) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0022();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_0023? Decode_0023(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0023) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0023();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        if(decoded.Version >= 1 && feature.Length >= 12)
        {
            decoded.RENoSA |= (feature[4] & 0x08) == 0x08;
            decoded.Expand |= (feature[4] & 0x04) == 0x04;
            decoded.QCert  |= (feature[4] & 0x02) == 0x02;
            decoded.Cert   |= (feature[4] & 0x01) == 0x01;
            decoded.RRM    |= (feature[8] & 0x01) == 0x01;
        }

        if(decoded.Version >= 2 && feature.Length >= 12) decoded.FRF |= (feature[4] & 0x80) == 0x80;

        return decoded;
    }

    public static Feature_0024? Decode_0024(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0024) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0024();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        if(decoded.Version >= 1 && feature.Length >= 8) decoded.SSA |= (feature[4] & 0x80) == 0x80;

        return decoded;
    }

    public static Feature_0025? Decode_0025(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 12) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0025) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0025();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.LogicalBlockSize = (uint)((feature[4] << 24) + (feature[5] << 16) + (feature[6] << 8) + feature[7]);

        decoded.Blocking = (ushort)((feature[8] << 8) + feature[9]);

        decoded.PP |= (feature[10] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_0026? Decode_0026(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0026) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0026();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_0027? Decode_0027(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0027) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0027();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_0028? Decode_0028(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0028) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0028();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.Write |= (feature[4] & 0x01) == 0x01;

        if(decoded.Version < 1) return decoded;

        decoded.DVDPWrite |= (feature[4] & 0x04) == 0x04;
        decoded.DVDPRead  |= (feature[4] & 0x02) == 0x02;

        return decoded;
    }

    public static Feature_0029? Decode_0029(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0029) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0029();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.DRTDM         |= (feature[4] & 0x01) == 0x01;
        decoded.DBICacheZones =  feature[5];
        decoded.Entries       =  (ushort)((feature[6] << 8) + feature[7]);

        return decoded;
    }

    public static Feature_002A? Decode_002A(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x002A) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_002A();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.Write     |= (feature[4] & 0x01) == 0x01;
        decoded.CloseOnly |= (feature[5] & 0x01) == 0x01;

        if(decoded.Version >= 1) decoded.QuickStart |= (feature[5] & 0x02) == 0x02;

        return decoded;
    }

    public static Feature_002B? Decode_002B(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x002B) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_002B();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.Write |= (feature[4] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_002C? Decode_002C(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x002C) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_002C();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.DSDG         |= (feature[4] & 0x08) == 0x08;
        decoded.DSDR         |= (feature[4] & 0x04) == 0x04;
        decoded.Intermediate |= (feature[4] & 0x02) == 0x02;
        decoded.Blank        |= (feature[4] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_002D? Decode_002D(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x002D) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_002D();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.TestWrite         |= (feature[4] & 0x04) == 0x04;
        decoded.CDRW              |= (feature[4] & 0x02) == 0x02;
        decoded.RWSubchannel      |= (feature[4] & 0x01) == 0x01;
        decoded.DataTypeSupported =  (ushort)((feature[6] << 8) + feature[7]);

        if(decoded.Version < 2) return decoded;

        decoded.BUF    |= (feature[4] & 0x40) == 0x40;
        decoded.RWRaw  |= (feature[4] & 0x10) == 0x10;
        decoded.RWPack |= (feature[4] & 0x08) == 0x08;

        return decoded;
    }

    public static Feature_002E? Decode_002E(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x002E) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_002E();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.SAO         |= (feature[4] & 0x20) == 0x20;
        decoded.RAWMS       |= (feature[4] & 0x10) == 0x10;
        decoded.RAW         |= (feature[4] & 0x08) == 0x08;
        decoded.TestWrite   |= (feature[4] & 0x04) == 0x04;
        decoded.CDRW        |= (feature[4] & 0x02) == 0x02;
        decoded.RW          |= (feature[4] & 0x01) == 0x01;
        decoded.MaxCueSheet =  (uint)((feature[5] << 16) + (feature[6] << 8) + feature[7]);

        if(decoded.Version >= 1) decoded.BUF |= (feature[4] & 0x40) == 0x40;

        return decoded;
    }

    public static Feature_002F? Decode_002F(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x002F) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_002F();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.BUF       |= (feature[4] & 0x40) == 0x40;
        decoded.TestWrite |= (feature[4] & 0x04) == 0x04;

        if(decoded.Version >= 1) decoded.DVDRW |= (feature[4] & 0x02) == 0x02;

        if(decoded.Version >= 2) decoded.RDL |= (feature[4] & 0x08) == 0x08;

        return decoded;
    }

    public static Feature_0030? Decode_0030(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0030) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0030();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_0031? Decode_0031(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0031) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0031();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.TestWrite |= (feature[4] & 0x04) == 0x04;

        return decoded;
    }

    public static Feature_0032? Decode_0032(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0032) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0032();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.Intermediate |= (feature[4] & 0x02) == 0x02;
        decoded.Blank        |= (feature[4] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_0033? Decode_0033(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0033) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0033();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        if(feature[7] <= 0 || feature.Length <= feature[7] + 8) return decoded;

        decoded.LinkSizes = new byte[feature[7]];
        Array.Copy(feature, 8, decoded.LinkSizes, 0, feature[7]);

        return decoded;
    }

    public static Feature_0035? Decode_0035(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0035) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0035();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_0037? Decode_0037(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0037) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0037();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.SubtypeSupport = feature[5];

        return decoded;
    }

    public static Feature_0038? Decode_0038(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0038) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0038();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_003A? Decode_003A(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x003A) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_003A();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.Write      |= (feature[4] & 0x01) == 0x01;
        decoded.QuickStart |= (feature[5] & 0x02) == 0x02;
        decoded.CloseOnly  |= (feature[5] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_003B? Decode_003B(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x003B) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_003B();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.Write |= (feature[4] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_0040? Decode_0040(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 32) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0040) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0040();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.OldRE  |= (feature[9]  & 0x01) == 0x01;
        decoded.OldR   |= (feature[17] & 0x01) == 0x01;
        decoded.OldROM |= (feature[25] & 0x01) == 0x01;

        if(decoded.Version < 1) return decoded;

        decoded.BCA |= (feature[4]  & 0x01) == 0x01;
        decoded.RE2 |= (feature[9]  & 0x04) == 0x04;
        decoded.RE1 |= (feature[9]  & 0x02) == 0x02;
        decoded.R   |= (feature[17] & 0x02) == 0x02;
        decoded.ROM |= (feature[25] & 0x02) == 0x02;

        return decoded;
    }

    public static Feature_0041? Decode_0041(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 24) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0041) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0041();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.SVNR  |= (feature[4]  & 0x01) == 0x01;
        decoded.OldRE |= (feature[9]  & 0x01) == 0x01;
        decoded.OldR  |= (feature[17] & 0x01) == 0x01;

        if(decoded.Version < 1) return decoded;

        decoded.RE2 |= (feature[9]  & 0x04) == 0x04;
        decoded.RE1 |= (feature[9]  & 0x02) == 0x02;
        decoded.R   |= (feature[17] & 0x02) == 0x02;

        return decoded;
    }

    public static Feature_0042? Decode_0042(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0042) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0042();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_0050? Decode_0050(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0050) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0050();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.HDDVDR   |= (feature[4] & 0x01) == 0x01;
        decoded.HDDVDRAM |= (feature[6] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_0051? Decode_0051(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0051) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0051();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.HDDVDR   |= (feature[4] & 0x01) == 0x01;
        decoded.HDDVDRAM |= (feature[6] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_0080? Decode_0080(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0080) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0080();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.RI |= (feature[4] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_0100? Decode_0100(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0100) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0100();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_0101? Decode_0101(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0101) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0101();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.PP |= (feature[4] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_0102? Decode_0102(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0102) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0102();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.SCC               |= (feature[4]       & 0x10) == 0x10;
        decoded.SDP               |= (feature[4]       & 0x04) == 0x04;
        decoded.HighestSlotNumber =  (byte)(feature[7] & 0x1F);

        return decoded;
    }

    public static Feature_0103? Decode_0103(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0103) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0103();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.Scan         |= (feature[4] & 0x04) == 0x04;
        decoded.SCM          |= (feature[4] & 0x02) == 0x02;
        decoded.SV           |= (feature[4] & 0x01) == 0x01;
        decoded.VolumeLevels =  (ushort)((feature[6] << 8) + feature[7]);

        return decoded;
    }

    public static Feature_0104? Decode_0104(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0104) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0104();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        if(decoded.Version >= 1 && feature.Length >= 8) decoded.M5 |= (feature[4] & 0x01) == 0x01;

        return decoded;
    }

    public static Feature_0105? Decode_0105(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0105) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0105();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        if(decoded.Version < 1 || feature.Length < 8) return decoded;

        decoded.Group3     |= (feature[4] & 0x01) == 0x01;
        decoded.UnitLength =  (ushort)((feature[6] << 8) + feature[7]);

        return decoded;
    }

    public static Feature_0106? Decode_0106(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0106) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0106();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.CSSVersion = feature[7];

        return decoded;
    }

    public static Feature_0107? Decode_0107(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0107) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0107();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        if(decoded.Version >= 3 && feature.Length >= 8)
        {
            decoded.RBCB |= (feature[4] & 0x10) == 0x10;
            decoded.SCS  |= (feature[4] & 0x08) == 0x08;
            decoded.MP2A |= (feature[4] & 0x04) == 0x04;
            decoded.WSPD |= (feature[4] & 0x02) == 0x02;
            decoded.SW   |= (feature[4] & 0x01) == 0x01;
        }

        if(decoded.Version < 5 || feature.Length < 8) return decoded;

        decoded.SMP  |= (feature[4] & 0x20) == 0x20;
        decoded.RBCB |= (feature[4] & 0x10) == 0x10;

        return decoded;
    }

    public static Feature_0108? Decode_0108(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0108) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0108();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        var serial = new byte[feature.Length];
        Array.Copy(feature, 4, serial, 0, feature.Length - 4);
        decoded.Serial = StringHandlers.CToString(serial).Trim();

        return decoded;
    }

    public static Feature_0109? Decode_0109(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0109) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0109();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_010A? Decode_010A(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x010A) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_010A();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.DCBs = new uint[feature[3] / 4];

        for(var i = 0; i < decoded.DCBs.Length; i++)
        {
            decoded.DCBs[i] = (uint)((feature[0 + 4 + i * 4] << 24) +
                                     (feature[1 + 4 + i * 4] << 16) +
                                     (feature[2 + 4 + i * 4] << 8)  +
                                     feature[3 + 4 + i * 4]);
        }

        return decoded;
    }

    public static Feature_010B? Decode_010B(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x010B) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_010B();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.CPRMVersion = feature[7];

        return decoded;
    }

    public static Feature_010C? Decode_010C(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 20) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x010C) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_010C();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.Century = (ushort)((feature[4]  << 8) + feature[5]);
        decoded.Year    = (ushort)((feature[6]  << 8) + feature[7]);
        decoded.Month   = (ushort)((feature[8]  << 8) + feature[9]);
        decoded.Day     = (ushort)((feature[10] << 8) + feature[11]);
        decoded.Hour    = (ushort)((feature[12] << 8) + feature[13]);
        decoded.Minute  = (ushort)((feature[14] << 8) + feature[15]);
        decoded.Second  = (ushort)((feature[16] << 8) + feature[17]);

        return decoded;
    }

    public static Feature_010D? Decode_010D(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x010D) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_010D();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.BNG             |= (feature[4] & 0x01) == 0x01;
        decoded.BindNonceBlocks =  feature[5];
        decoded.AGIDs           =  (byte)(feature[6] & 0x0F);
        decoded.AACSVersion     =  feature[7];

        if(decoded.Version < 2) return decoded;

        decoded.RDC |= (feature[4] & 0x10) == 0x10;
        decoded.RMC |= (feature[4] & 0x08) == 0x08;
        decoded.WBE |= (feature[4] & 0x04) == 0x04;
        decoded.BEC |= (feature[4] & 0x02) == 0x02;

        return decoded;
    }

    public static Feature_010E? Decode_010E(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x010E) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_010E();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.MaxScrambleExtent = feature[4];

        return decoded;
    }

    public static Feature_0110? Decode_0110(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 8) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0110) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0110();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_0113? Decode_0113(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 4) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0113) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0113();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        return decoded;
    }

    public static Feature_0142? Decode_0142(byte[] feature)
    {
        if(feature == null) return null;

        if(feature.Length < 6) return null;

        var number = (ushort)((feature[0] << 8) + feature[1]);

        if(number != 0x0142) return null;

        if(feature[3] + 4 != feature.Length) return null;

        var decoded = new Feature_0142();

        decoded.Current    |= (feature[2] & 0x01) == 0x01;
        decoded.Persistent |= (feature[2] & 0x02) == 0x02;
        decoded.Version    =  (byte)((feature[2] & 0x3C) >> 2);

        decoded.PSAU     |= (feature[4] & 0x80) == 0x80;
        decoded.LOSPB    |= (feature[4] & 0x40) == 0x40;
        decoded.ME       |= (feature[4] & 0x01) == 0x01;
        decoded.Profiles =  new ushort[feature[5]];

        if(feature[5] * 2 + 6 != feature.Length) return decoded;

        for(var i = 0; i < feature[5]; i++)
            decoded.Profiles[i] = (ushort)((feature[0 + 6 + 2 * i] << 8) + feature[1 + 6 + 2 * i]);

        return decoded;
    }

    public static string Prettify_0000(Feature_0000? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0000 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.MMC_Supported_Profiles);

        if(ftr.Profiles == null) return sb.ToString();

        foreach(Profile prof in ftr.Profiles)
        {
            switch(prof.Number)
            {
                case ProfileNumber.Reserved:
                    sb.Append("\t" + Localization.Drive_reported_a_reserved_profile_number);

                    break;
                case ProfileNumber.NonRemovable:
                    sb.Append("\t" + Localization.Drive_supports_non_removable_changeable_media);

                    break;
                case ProfileNumber.Removable:
                    sb.Append("\t" + Localization.Drive_supports_rewritable_and_removable_media);

                    break;
                case ProfileNumber.MOErasable:
                    sb.Append("\t" + Localization.Drive_supports_Magneto_Optical_media);

                    break;
                case ProfileNumber.OpticalWORM:
                    sb.Append("\t" + Localization.Drive_supports_optical_write_once_media);

                    break;
                case ProfileNumber.ASMO:
                    sb.Append("\t" + Localization.Drive_supports_Advanced_Storage_Magneto_Optical);

                    break;
                case ProfileNumber.CDROM:
                    sb.Append("\t" + Localization.Drive_supports_CD_ROM);

                    break;
                case ProfileNumber.CDR:
                    sb.Append("\t" + Localization.Drive_supports_CD_R);

                    break;
                case ProfileNumber.CDRW:
                    sb.Append("\t" + Localization.Drive_supports_CD_RW);

                    break;
                case ProfileNumber.DVDROM:
                    sb.Append("\t" + Localization.Drive_supports_DVD_ROM);

                    break;
                case ProfileNumber.DVDRSeq:
                    sb.Append("\t" + Localization.Drive_supports_DVD_R);

                    break;
                case ProfileNumber.DVDRAM:
                    sb.Append("\t" + Localization.Drive_supports_DVD_RAM);

                    break;
                case ProfileNumber.DVDRWRes:
                    sb.Append("\t" + Localization.Drive_supports_restricted_overwrite_DVD_RW);

                    break;
                case ProfileNumber.DVDRWSeq:
                    sb.Append("\t" + Localization.Drive_supports_sequentially_recorded_DVD_RW);

                    break;
                case ProfileNumber.DVDRDLSeq:
                    sb.Append("\t" + Localization.Drive_supports_sequentially_recorded_DVD_R_DL);

                    break;
                case ProfileNumber.DVDRDLJump:
                    sb.Append("\t" + Localization.Drive_supports_layer_jump_recorded_DVD_R_DL);

                    break;
                case ProfileNumber.DVDRWDL:
                    sb.Append("\t" + Localization.Drive_supports_DVD_RW_DL);

                    break;
                case ProfileNumber.DVDDownload:
                    sb.Append("\t" + Localization.Drive_supports_DVD_Download);

                    break;
                case ProfileNumber.DVDRWPlus:
                    sb.Append("\t" + Localization.Drive_supports_DVD_Plus_RW);

                    break;
                case ProfileNumber.DVDRPlus:
                    sb.Append("\t" + Localization.Drive_supports_DVD_Plus_R);

                    break;
                case ProfileNumber.DDCDROM:
                    sb.Append("\t" + Localization.Drive_supports_DDCD_ROM);

                    break;
                case ProfileNumber.DDCDR:
                    sb.Append("\t" + Localization.Drive_supports_DDCD_R);

                    break;
                case ProfileNumber.DDCDRW:
                    sb.Append("\t" + Localization.Drive_supports_DDCD_RW);

                    break;
                case ProfileNumber.DVDRWDLPlus:
                    sb.Append("\t" + Localization.Drive_supports_DVD_Plus_RW_DL);

                    break;
                case ProfileNumber.DVDRDLPlus:
                    sb.Append("\t" + Localization.Drive_supports_DVD_Plus_R_DL);

                    break;
                case ProfileNumber.BDROM:
                    sb.Append("\t" + Localization.Drive_supports_BD_ROM);

                    break;
                case ProfileNumber.BDRSeq:
                    sb.Append("\t" + Localization.Drive_supports_BD_R_SRM);

                    break;
                case ProfileNumber.BDRRdm:
                    sb.Append("\t" + Localization.Drive_supports_BD_R_RRM);

                    break;
                case ProfileNumber.BDRE:
                    sb.Append("\t" + Localization.Drive_supports_BD_RE);

                    break;
                case ProfileNumber.HDDVDROM:
                    sb.Append("\t" + Localization.Drive_supports_HD_DVD_ROM);

                    break;
                case ProfileNumber.HDDVDR:
                    sb.Append("\t" + Localization.Drive_supports_HD_DVD_R);

                    break;
                case ProfileNumber.HDDVDRAM:
                    sb.Append("\t" + Localization.Drive_supports_HD_DVD_RAM);

                    break;
                case ProfileNumber.HDDVDRW:
                    sb.Append("\t" + Localization.Drive_supports_HD_DVD_RW);

                    break;
                case ProfileNumber.HDDVDRDL:
                    sb.Append("\t" + Localization.Drive_supports_HD_DVD_R_DL);

                    break;
                case ProfileNumber.HDDVDRWDL:
                    sb.Append("\t" + Localization.Drive_supports_HD_DVD_RW_DL);

                    break;
                case ProfileNumber.HDBURNROM:
                    sb.Append("\t" + Localization.Drive_supports_HDBurn_CD_ROM);

                    break;
                case ProfileNumber.HDBURNR:
                    sb.Append("\t" + Localization.Drive_supports_HDBurn_CD_R);

                    break;
                case ProfileNumber.HDBURNRW:
                    sb.Append("\t" + Localization.Drive_supports_HDBurn_CD_RW);

                    break;
                case ProfileNumber.Unconforming:
                    sb.Append("\t" + Localization.Drive_is_not_conforming_to_any_profile);

                    break;
                default:
                    sb.AppendFormat("\t" + Localization.Drive_informs_of_unknown_profile_0, (ushort)prof.Number);

                    break;
            }

            if(prof.Current)
                sb.AppendLine(Localization.current);
            else
                sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string Prettify_0001(Feature_0001? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0001 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.MMC_Core_Feature);
        sb.Append("\t");

        switch(ftr.PhysicalInterfaceStandard)
        {
            case PhysicalInterfaces.Unspecified:
                sb.AppendLine(Localization.Drive_uses_an_unspecified_physical_interface);

                break;
            case PhysicalInterfaces.SCSI:
                sb.AppendLine(Localization.Drive_uses_a_SCSI_interface);

                break;
            case PhysicalInterfaces.ATAPI:
                sb.AppendLine(Localization.Drive_uses_an_ATAPI_interface);

                break;
            case PhysicalInterfaces.IEEE1394:
                sb.AppendLine(Localization.Drive_uses_an_IEEE_1394_interface);

                break;
            case PhysicalInterfaces.IEEE1394A:
                sb.AppendLine(Localization.Drive_uses_an_IEEE_1394A_interface);

                break;
            case PhysicalInterfaces.FC:
                sb.AppendLine(Localization.Drive_uses_a_Fibre_Channel_interface);

                break;
            case PhysicalInterfaces.IEEE1394B:
                sb.AppendLine(Localization.Drive_uses_an_IEEE_1394B_interface);

                break;
            case PhysicalInterfaces.SerialATAPI:
                sb.AppendLine(Localization.Drive_uses_a_Serial_ATAPI_interface);

                break;
            case PhysicalInterfaces.USB:
                sb.AppendLine(Localization.Drive_uses_an_USB_interface);

                break;
            case PhysicalInterfaces.Vendor:
                sb.AppendLine(Localization.Drive_uses_a_vendor_unique_interface);

                break;
            default:
                sb.AppendFormat(Localization.Drive_uses_an_unknown_interface_with_code_0,
                                (uint)ftr.PhysicalInterfaceStandard)
                  .AppendLine();

                break;
        }

        if(ftr.DBE) sb.AppendLine("\t" + Localization.Drive_supports_Device_Busy_events);

        if(ftr.INQ2)
        {
            sb.AppendLine("\t" +
                          Localization
                             .Drive_supports_EVPD_Page_Code_and_16_bit_Allocation_Length_as_described_in_SPC_3);
        }

        return sb.ToString();
    }

    public static string Prettify_0002(Feature_0002? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0002 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.MMC_Morphing);

        sb.AppendLine(ftr.Async
                          ? "\t" + Localization.Drive_supports_polling_and_asynchronous_GET_EVENT_STATUS_NOTIFICATION
                          : "\t" + Localization.Drive_supports_only_polling_GET_EVENT_STATUS_NOTIFICATION);

        if(ftr.OCEvent)
            sb.AppendLine("\t" + Localization.Drive_supports_operational_change_request_notification_class_events);

        return sb.ToString();
    }

    public static string Prettify_0003(Feature_0003? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0003 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.Features_Prettify_0003_MMC_Removable_Medium);

        switch(ftr.LoadingMechanismType)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Features_Prettify_0003_Drive_uses_media_caddy);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Features_Prettify_0003_Drive_uses_a_tray);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Features_Prettify_0003_Drive_is_pop_up);

                break;
            case 4:
                sb.AppendLine("\t" +
                              Localization
                                 .Features_Prettify_0003_Drive_is_a_changer_with_individually_changeable_discs);

                break;
            case 5:
                sb.AppendLine("\t" + Localization.Features_Prettify_0003_Drive_is_a_changer_using_cartridges);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Features_Prettify_0003_Drive_uses_unknown_loading_mechanism_type_0,
                                ftr.LoadingMechanismType)
                  .AppendLine();

                break;
        }

        if(ftr.Lock) sb.AppendLine("\t" + Localization.Features_Prettify_0003_Drive_can_lock_media);

        if(ftr.PreventJumper) sb.AppendLine("\t" + Localization.Features_Prettify_0003_Drive_power_ups_locked);

        if(ftr.Eject) sb.AppendLine("\t" + Localization.Features_Prettify_0003_Drive_can_eject_media);

        if(ftr.Load) sb.AppendLine("\t" + Localization.Features_Prettify_0003_Drive_can_load_media);

        if(ftr.DBML)
        {
            sb.AppendLine("\t" +
                          Localization
                             .Features_Prettify_0003_Drive_reports_Device_Busy_Class_events_during_medium_loading_unloading);
        }

        return sb.ToString();
    }

    public static string Prettify_0004(Feature_0004? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0004 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.MMC_Write_Protect);

        if(ftr.DWP)
        {
            sb.AppendLine("\t" +
                          Localization.Drive_supports_reading_writing_the_Disc_Write_Protect_PAC_on_BD_R_RE_media);
        }

        if(ftr.WDCB) sb.AppendLine("\t" + Localization.Drive_supports_writing_the_Write_Inhibit_DCB_on_DVD_RW_media);

        if(ftr.SPWP) sb.AppendLine("\t" + Localization.Drive_supports_set_release_of_PWP_status);

        if(ftr.SSWPP)
            sb.AppendLine("\t" + Localization.Drive_supports_the_SWPP_bit_of_the_Timeout_and_Protect_mode_page);

        return sb.ToString();
    }

    public static string Prettify_0010(Feature_0010? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0010 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.Append(Localization.MMC_Random_Readable);

        if(ftr.Current) sb.Append(Localization.current);

        sb.AppendLine(":");

        if(ftr.PP) sb.AppendLine("\t" + Localization.Drive_shall_report_Read_Write_Error_Recovery_mode_page);

        if(ftr.LogicalBlockSize > 0)
            sb.AppendFormat("\t" + Localization._0_bytes_per_logical_block, ftr.LogicalBlockSize).AppendLine();

        if(ftr.Blocking > 1)
            sb.AppendFormat("\t" + Localization._0_logical_blocks_per_media_readable_unit, ftr.Blocking).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_001D(Feature_001D? feature) =>
        !feature.HasValue
            ? null
            : Localization.Drive_claims_capability_to_read_all_CD_formats_according_to_OSTA_Multi_Read_Specification +
              "\n";

    public static string Prettify_001E(Feature_001E? feature)
    {
        if(!feature.HasValue) return null;

        Feature_001E ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.Append(Localization.MMC_CD_Read);

        if(ftr.Current) sb.Append(Localization.current);

        sb.AppendLine(":");

        if(ftr.DAP)
            sb.AppendLine("\t" + Localization.Drive_supports_the_DAP_bit_in_the_READ_CD_and_READ_CD_MSF_commands);

        if(ftr.C2) sb.AppendLine("\t" + Localization.Drive_supports_C2_Error_Pointers);

        if(ftr.CDText) sb.AppendLine("\t" + Localization.Drive_can_return_CD_Text_from_Lead_In);

        return sb.ToString();
    }

    public static string Prettify_001F(Feature_001F? feature)
    {
        if(!feature.HasValue) return null;

        Feature_001F ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.Append(Localization.MMC_DVD_Read);

        if(ftr.Current) sb.Append(Localization.current);

        sb.AppendLine(":");
        sb.AppendLine("\t" + Localization.Drive_can_read_DVD_media);

        if(ftr.DualR) sb.AppendLine("\t" + Localization.Drive_can_read_DVD_R_DL_from_all_recording_modes);

        if(ftr.DualRW) sb.AppendLine("\t" + Localization.Drive_can_read_DVD_RW_DL_from_all_recording_modes);

        if(ftr.MULTI110) sb.AppendLine("\t" + Localization.Drive_conforms_to_DVD_Multi_Drive_Read_only_Specifications);

        return sb.ToString();
    }

    public static string Prettify_0020(Feature_0020? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0020 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.Append(Localization.MMC_Random_Writable);

        if(ftr.Current) sb.Append(Localization.current);

        sb.AppendLine(":");

        if(ftr.PP) sb.AppendLine("\t" + Localization.Drive_shall_report_Read_Write_Error_Recovery_mode_page);

        if(ftr.LogicalBlockSize > 0)
            sb.AppendFormat("\t" + Localization._0_bytes_per_logical_block, ftr.LogicalBlockSize).AppendLine();

        if(ftr.Blocking > 1)
            sb.AppendFormat("\t" + Localization._0_logical_blocks_per_media_writable_unit, ftr.Blocking).AppendLine();

        if(ftr.LastLBA > 0)
            sb.AppendFormat("\t" + Localization.Last_addressable_logical_block_is_0, ftr.LastLBA).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0021(Feature_0021? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0021 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.MMC_Incremental_Streaming_Writable);

        if(ftr.DataTypeSupported > 0)
        {
            sb.Append("\t" + Localization.Drive_supports_data_block_types);

            if((ftr.DataTypeSupported & 0x0001) == 0x0001) sb.Append(" 0");

            if((ftr.DataTypeSupported & 0x0002) == 0x0002) sb.Append(" 1");

            if((ftr.DataTypeSupported & 0x0004) == 0x0004) sb.Append(" 2");

            if((ftr.DataTypeSupported & 0x0008) == 0x0008) sb.Append(" 3");

            if((ftr.DataTypeSupported & 0x0010) == 0x0010) sb.Append(" 4");

            if((ftr.DataTypeSupported & 0x0020) == 0x0020) sb.Append(" 5");

            if((ftr.DataTypeSupported & 0x0040) == 0x0040) sb.Append(" 6");

            if((ftr.DataTypeSupported & 0x0080) == 0x0080) sb.Append(" 7");

            if((ftr.DataTypeSupported & 0x0100) == 0x0100) sb.Append(" 8");

            if((ftr.DataTypeSupported & 0x0200) == 0x0200) sb.Append(" 9");

            if((ftr.DataTypeSupported & 0x0400) == 0x0400) sb.Append(" 10");

            if((ftr.DataTypeSupported & 0x0800) == 0x0800) sb.Append(" 11");

            if((ftr.DataTypeSupported & 0x1000) == 0x1000) sb.Append(" 12");

            if((ftr.DataTypeSupported & 0x2000) == 0x2000) sb.Append(" 13");

            if((ftr.DataTypeSupported & 0x4000) == 0x4000) sb.Append(" 14");

            if((ftr.DataTypeSupported & 0x8000) == 0x8000) sb.Append(" 15");

            sb.AppendLine();
        }

        if(ftr.TRIO) sb.AppendLine("\t" + Localization.Drive_claims_support_to_report_Track_Resources_Information);

        if(ftr.ARSV)
            sb.AppendLine("\t" + Localization.Drive_supports_address_mode_reservation_on_the_RESERVE_TRACK_command);

        if(ftr.BUF) sb.AppendLine("\t" + Localization.Drive_is_capable_of_zero_loss_linking);

        return sb.ToString();
    }

    public static string Prettify_0022(Feature_0022? feature) =>
        !feature.HasValue ? null : Localization.Drive_supports_media_that_require_erasing_before_writing + "\n";

    public static string Prettify_0023(Feature_0023? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0023 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.MMC_Formattable);
        sb.AppendLine("\t" + Localization.Drive_can_format_media_into_logical_blocks);

        if(ftr.RENoSA) sb.AppendLine("\t" + Localization.Drive_can_format_BD_RE_with_no_spares_allocated);

        if(ftr.Expand) sb.AppendLine("\t" + Localization.Drive_can_expand_the_spare_area_on_a_formatted_BD_RE_disc);

        if(ftr.QCert) sb.AppendLine("\t" + Localization.Drive_can_format_BD_RE_discs_with_quick_certification);

        if(ftr.Cert) sb.AppendLine("\t" + Localization.Drive_can_format_BD_RE_discs_with_full_certification);

        if(ftr.FRF) sb.AppendLine("\t" + Localization.Drive_can_fast_re_format_BD_RE_discs);

        if(ftr.RRM) sb.AppendLine("\t" + Localization.Drive_can_format_BD_R_discs_with_RRM_format);

        return sb.ToString();
    }

    public static string Prettify_0024(Feature_0024? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0024 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.MMC_Hardware_Defect_Management);
        sb.AppendLine("\t" + Localization.Drive_shall_be_able_to_provide_a_defect_free_contiguous_address_space);

        if(ftr.SSA) sb.AppendLine("\t" + Localization.Drive_can_return_Spare_Area_Information);

        return sb.ToString();
    }

    public static string Prettify_0025(Feature_0025? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0025 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.Append(Localization.MMC_Write_Once);

        if(ftr.Current) sb.Append(Localization.current);

        sb.AppendLine(":");

        if(ftr.PP) sb.AppendLine("\t" + Localization.Drive_shall_report_Read_Write_Error_Recovery_mode_page);

        if(ftr.LogicalBlockSize > 0)
            sb.AppendFormat("\t" + Localization._0_bytes_per_logical_block, ftr.LogicalBlockSize).AppendLine();

        if(ftr.Blocking > 1)
            sb.AppendFormat("\t" + Localization._0_logical_blocks_per_media_writable_unit, ftr.Blocking).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0026(Feature_0026? feature) =>
        !feature.HasValue
            ? null
            : Localization.Drive_shall_have_the_ability_to_overwrite_logical_blocks_only_in_fixed_sets_at_a_time + "\n";

    public static string Prettify_0027(Feature_0027? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0027 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.Append(Localization.Drive_can_write_High_Speed_CD_RW);

        if(ftr.Current)
            sb.AppendLine(Localization.current);
        else
            sb.AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0028(Feature_0028? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0028 ftr = feature.Value;
        var          sb  = new StringBuilder();

        if(ftr.Write && ftr is { DVDPRead: true, DVDPWrite: true })
            sb.Append(Localization.Drive_can_read_and_write_CD_MRW_and_DVD_MRW);
        else if(ftr is { DVDPRead: true, DVDPWrite: true })
            sb.Append(Localization.Drive_can_read_and_write_DVD_MRW);
        else
        {
            switch(ftr.Write)
            {
                case true when ftr.DVDPRead:
                    sb.Append(Localization.Drive_and_read_DVD_MRW_and_read_and_write_CD_MRW);

                    break;
                case true:
                    sb.Append(Localization.Drive_can_read_and_write_CD_MRW);

                    break;
                default:
                {
                    sb.Append(ftr.DVDPRead
                                  ? Localization.Drive_can_read_CD_MRW_and_DVD_MRW
                                  : Localization.Drive_can_read_CD_MRW);

                    break;
                }
            }
        }

        if(ftr.Current)
            sb.AppendLine(Localization.current);
        else
            sb.AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0029(Feature_0029? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0029 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.MMC_Enhanced_Defect_Reporting_Feature);

        sb.AppendLine(ftr.DRTDM
                          ? "\t" + Localization.Drive_supports_DRT_DM_mode
                          : "\t" + Localization.Drive_supports_Persistent_DM_mode);

        if(ftr.DBICacheZones > 0)
            sb.AppendFormat("\t" + Localization.Drive_has_0_DBI_cache_zones, ftr.DBICacheZones).AppendLine();

        if(ftr.Entries > 0) sb.AppendFormat("\t" + Localization.Drive_has_0_DBI_entries, ftr.Entries).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_002A(Feature_002A? feature)
    {
        if(!feature.HasValue) return null;

        Feature_002A ftr = feature.Value;
        var          sb  = new StringBuilder();

        if(ftr.Write)
        {
            sb.Append(Localization.Drive_can_read_and_write_DVD_RW);

            if(ftr.Current)
                sb.AppendLine(Localization.current);
            else
                sb.AppendLine();

            sb.AppendLine(ftr.CloseOnly
                              ? "\t" + Localization.Drive_supports_only_the_read_compatibility_stop
                              : "\t" + Localization.Drive_supports_both_forms_of_background_format_stopping);

            if(ftr.QuickStart) sb.AppendLine("\t" + Localization.Drive_can_do_a_quick_start_formatting);
        }
        else
        {
            sb.Append(Localization.Drive_can_read_DVD_Plus_RW);

            if(ftr.Current)
                sb.AppendLine(Localization.current);
            else
                sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string Prettify_002B(Feature_002B? feature)
    {
        if(!feature.HasValue) return null;

        Feature_002B ftr = feature.Value;
        var          sb  = new StringBuilder();

        if(ftr.Write)
        {
            sb.Append(Localization.Drive_can_read_and_write_DVD_Plus_R);

            if(ftr.Current)
                sb.AppendLine(Localization.current);
            else
                sb.AppendLine();
        }
        else
        {
            sb.Append(Localization.Drive_can_read_DVD_Plus_R);

            if(ftr.Current)
                sb.AppendLine(Localization.current);
            else
                sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string Prettify_002C(Feature_002C? feature)
    {
        if(!feature.HasValue) return null;

        Feature_002C ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.Append(Localization.MMC_Rigid_Restricted_Overwrite);
        sb.AppendLine(ftr.Current ? $"{Localization.current}:" : ":");

        if(ftr.Blank) sb.AppendLine("\t" + Localization.Drive_supports_the_BLANK_command);

        if(ftr.Intermediate)
        {
            sb.AppendLine("\t" +
                          Localization.Drive_supports_writing_on_an_intermediate_state_session_and_quick_formatting);
        }

        if(ftr.DSDR) sb.AppendLine("\t" + Localization.Drive_can_read_Defect_Status_data_recorded_on_the_medium);

        if(ftr.DSDG) sb.AppendLine("\t" + Localization.Drive_can_generate_Defect_Status_data_during_formatting);

        return sb.ToString();
    }

    public static string Prettify_002D(Feature_002D? feature)
    {
        if(!feature.HasValue) return null;

        Feature_002D ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.Drive_can_write_CDs_in_Track_at_Once_Mode);

        if(ftr.RWSubchannel)
        {
            sb.AppendLine("\t" + Localization.Drive_can_write_user_provided_data_in_the_R_W_subchannels);

            if(ftr.RWRaw) sb.AppendLine("\t" + Localization.Drive_accepts_RAW_R_W_subchannel_data);

            if(ftr.RWPack) sb.AppendLine("\t" + Localization.Drive_accepts_Packed_R_W_subchannel_data);
        }

        if(ftr.CDRW) sb.AppendLine("\t" + Localization.Drive_can_overwrite_a_TAO_track_with_another_in_CD_RWs);

        if(ftr.TestWrite) sb.AppendLine("\t" + Localization.Drive_can_do_a_test_writing);

        if(ftr.BUF) sb.AppendLine("\t" + Localization.Drive_supports_zero_loss_linking);

        if(ftr.DataTypeSupported <= 0) return sb.ToString();

        sb.Append("\t" + Localization.Drive_supports_data_block_types);

        if((ftr.DataTypeSupported & 0x0001) == 0x0001) sb.Append(" 0");

        if((ftr.DataTypeSupported & 0x0002) == 0x0002) sb.Append(" 1");

        if((ftr.DataTypeSupported & 0x0004) == 0x0004) sb.Append(" 2");

        if((ftr.DataTypeSupported & 0x0008) == 0x0008) sb.Append(" 3");

        if((ftr.DataTypeSupported & 0x0010) == 0x0010) sb.Append(" 4");

        if((ftr.DataTypeSupported & 0x0020) == 0x0020) sb.Append(" 5");

        if((ftr.DataTypeSupported & 0x0040) == 0x0040) sb.Append(" 6");

        if((ftr.DataTypeSupported & 0x0080) == 0x0080) sb.Append(" 7");

        if((ftr.DataTypeSupported & 0x0100) == 0x0100) sb.Append(" 8");

        if((ftr.DataTypeSupported & 0x0200) == 0x0200) sb.Append(" 9");

        if((ftr.DataTypeSupported & 0x0400) == 0x0400) sb.Append(" 10");

        if((ftr.DataTypeSupported & 0x0800) == 0x0800) sb.Append(" 11");

        if((ftr.DataTypeSupported & 0x1000) == 0x1000) sb.Append(" 12");

        if((ftr.DataTypeSupported & 0x2000) == 0x2000) sb.Append(" 13");

        if((ftr.DataTypeSupported & 0x4000) == 0x4000) sb.Append(" 14");

        if((ftr.DataTypeSupported & 0x8000) == 0x8000) sb.Append(" 15");

        sb.AppendLine();

        return sb.ToString();
    }

    public static string Prettify_002E(Feature_002E? feature)
    {
        if(!feature.HasValue) return null;

        Feature_002E ftr = feature.Value;
        var          sb  = new StringBuilder();

        switch(ftr.SAO)
        {
            case true when !ftr.RAW:
                sb.AppendLine(Localization.Drive_can_write_CDs_in_Session_at_Once_Mode);

                break;
            case false when ftr.RAW:
                sb.AppendLine(Localization.Drive_can_write_CDs_in_raw_Mode);

                break;
            default:
                sb.AppendLine(Localization.Drive_can_write_CDs_in_Session_at_Once_and_in_Raw_Modes);

                break;
        }

        if(ftr is { RAW: true, RAWMS: true })
            sb.AppendLine("\t" + Localization.Drive_can_write_multi_session_CDs_in_raw_mode);

        if(ftr.RW) sb.AppendLine("\t" + Localization.Drive_can_write_user_provided_data_in_the_R_W_subchannels);

        if(ftr.CDRW) sb.AppendLine("\t" + Localization.Drive_can_write_CD_RW);

        if(ftr.TestWrite) sb.AppendLine("\t" + Localization.Drive_can_do_a_test_writing);

        if(ftr.BUF) sb.AppendLine("\t" + Localization.Drive_supports_zero_loss_linking);

        if(ftr.MaxCueSheet > 0)
        {
            sb.AppendFormat("\t" + Localization.Drive_supports_a_maximum_of_0_bytes_in_a_single_cue_sheet,
                            ftr.MaxCueSheet)
              .AppendLine();
        }

        return sb.ToString();
    }

    public static string Prettify_002F(Feature_002F? feature)
    {
        if(!feature.HasValue) return null;

        Feature_002F ftr = feature.Value;
        var          sb  = new StringBuilder();

        if(ftr is { DVDRW: true, RDL: true })
            sb.AppendLine(Localization.Drive_supports_writing_DVD_R_DVD_RW_and_DVD_R_DL);
        else if(ftr.RDL)
            sb.AppendLine(Localization.Drive_supports_writing_DVD_R_and_DVD_R_DL);
        else if(ftr.DVDRW)
            sb.AppendLine(Localization.Drive_supports_writing_DVD_R_and_DVD_RW);
        else
            sb.AppendLine(Localization.Drive_supports_writing_DVD_R);

        if(ftr.TestWrite) sb.AppendLine("\t" + Localization.Drive_can_do_a_test_writing);

        if(ftr.BUF) sb.AppendLine("\t" + Localization.Drive_supports_zero_loss_linking);

        return sb.ToString();
    }

    public static string Prettify_0030(Feature_0030? feature) =>
        !feature.HasValue ? null : Localization.Drive_can_read_DDCDs + "\n";

    public static string Prettify_0031(Feature_0031? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0031 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.Drive_supports_writing_DDCD_R);

        if(ftr.TestWrite) sb.AppendLine("\t" + Localization.Drive_can_do_a_test_writing);

        return sb.ToString();
    }

    public static string Prettify_0032(Feature_0032? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0032 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.Drive_supports_writing_DDCD_RW);

        if(ftr.Blank) sb.AppendLine("\t" + Localization.Drive_supports_the_BLANK_command);

        if(ftr.Intermediate) sb.AppendLine("\t" + Localization.Drive_supports_quick_formatting);

        return sb.ToString();
    }

    public static string Prettify_0033(Feature_0033? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0033 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.MMC_Layer_Jump_Recording);

        if(ftr.LinkSizes == null) return sb.ToString();

        foreach(byte link in ftr.LinkSizes)
            sb.AppendFormat("\t" + Localization.Current_media_has_a_0_bytes_link_available, link).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0035(Feature_0035? feature) =>
        !feature.HasValue ? null : Localization.Drive_can_stop_a_long_immediate_operation + "\n";

    public static string Prettify_0037(Feature_0037? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0037 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.Drive_can_write_CD_RW);

        if(ftr.SubtypeSupport <= 0) return sb.ToString();

        sb.Append("\t" + Localization.Drive_supports_CD_RW_subtypes);

        if((ftr.SubtypeSupport & 0x01) == 0x01) sb.Append(" 0");

        if((ftr.SubtypeSupport & 0x02) == 0x02) sb.Append(" 1");

        if((ftr.SubtypeSupport & 0x04) == 0x04) sb.Append(" 2");

        if((ftr.SubtypeSupport & 0x08) == 0x08) sb.Append(" 3");

        if((ftr.SubtypeSupport & 0x10) == 0x10) sb.Append(" 4");

        if((ftr.SubtypeSupport & 0x20) == 0x20) sb.Append(" 5");

        if((ftr.SubtypeSupport & 0x40) == 0x40) sb.Append(" 6");

        if((ftr.SubtypeSupport & 0x80) == 0x80) sb.Append(" 7");

        sb.AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0038(Feature_0038? feature) =>
        !feature.HasValue ? null : Localization.Drive_can_write_BD_R_on_Pseudo_OVerwrite_SRM_mode + "\n";

    public static string Prettify_003A(Feature_003A? feature)
    {
        if(!feature.HasValue) return null;

        Feature_003A ftr = feature.Value;
        var          sb  = new StringBuilder();

        if(ftr.Write)
        {
            sb.Append(Localization.Drive_can_read_and_write_DVD_Plus_RW_DL);

            if(ftr.Current)
                sb.AppendLine(Localization.current);
            else
                sb.AppendLine();

            sb.AppendLine(ftr.CloseOnly
                              ? "\t" + Localization.Drive_supports_only_the_read_compatibility_stop
                              : "\t" + Localization.Drive_supports_both_forms_of_background_format_stopping);

            if(ftr.QuickStart) sb.AppendLine("\t" + Localization.Drive_can_do_a_quick_start_formatting);
        }
        else
        {
            sb.Append(Localization.Drive_can_read_DVD_Plus_RW_DL);

            if(ftr.Current)
                sb.AppendLine(Localization.current);
            else
                sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string Prettify_003B(Feature_003B? feature)
    {
        if(!feature.HasValue) return null;

        Feature_003B ftr = feature.Value;
        var          sb  = new StringBuilder();

        if(ftr.Write)
        {
            sb.Append(Localization.Drive_can_read_and_write_DVD_Plus_R_DL);

            if(ftr.Current)
                sb.AppendLine(Localization.current);
            else
                sb.AppendLine();
        }
        else
        {
            sb.Append(Localization.Features_Prettify_003B_Drive_can_read_DVD_Plus_R_DL);

            if(ftr.Current)
                sb.AppendLine(Localization.current);
            else
                sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string Prettify_0040(Feature_0040? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0040 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.Append(Localization.MMC_BD_Read);
        sb.AppendLine(ftr.Current ? $"{Localization.current}:" : ":");

        if(ftr.OldROM) sb.AppendLine("\t" + Localization.Drive_can_read_BD_ROM_pre_1_0);

        if(ftr.ROM) sb.AppendLine("\t" + Localization.Drive_can_read_BD_ROM_Ver_1);

        if(ftr.OldR) sb.AppendLine("\t" + Localization.Drive_can_read_BD_R_pre_1_0);

        if(ftr.R) sb.AppendLine("\t" + Localization.Drive_can_read_BD_R_Ver_1);

        if(ftr.OldRE) sb.AppendLine("\t" + Localization.Drive_can_read_BD_RE_pre_1_0);

        if(ftr.RE1) sb.AppendLine("\t" + Localization.Drive_can_read_BD_RE_Ver_1);

        if(ftr.RE2) sb.AppendLine("\t" + Localization.Drive_can_read_BD_RE_Ver_2);

        if(ftr.BCA) sb.AppendLine("\t" + Localization.Drive_can_read_BDs_Burst_Cutting_Area);

        return sb.ToString();
    }

    public static string Prettify_0041(Feature_0041? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0041 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.Append(Localization.MMC_BD_Write);
        sb.AppendLine(ftr.Current ? $"{Localization.current}:" : ":");

        if(ftr.OldR) sb.AppendLine("\t" + Localization.Drive_can_write_BD_R_pre_1_0);

        if(ftr.R) sb.AppendLine("\t" + Localization.Drive_can_write_BD_R_Ver_1);

        if(ftr.OldRE) sb.AppendLine("\t" + Localization.Drive_can_write_BD_RE_pre_1_0);

        if(ftr.RE1) sb.AppendLine("\t" + Localization.Drive_can_write_BD_RE_Ver_1);

        if(ftr.RE2) sb.AppendLine("\t" + Localization.Drive_can_write_BD_RE_Ver_2);

        if(ftr.SVNR) sb.AppendLine("\t" + Localization.Drive_supports_write_without_verify_requirement);

        return sb.ToString();
    }

    public static string Prettify_0042(Feature_0042? feature) =>
        !feature.HasValue
            ? null
            : Localization
                 .Features_Prettify_0042_Drive_is_able_to_detect_and_report_defective_writable_unit_and_behave_accordingly +
              "\n";

    public static string Prettify_0050(Feature_0050? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0050 ftr = feature.Value;
        var          sb  = new StringBuilder();

        switch(ftr.HDDVDR)
        {
            case true when ftr.HDDVDRAM:
                sb.Append(Localization.Drive_can_read_HD_DVD_ROM_HD_DVD_RW_HD_DVD_R_and_HD_DVD_RAM);

                break;
            case true:
                sb.Append(Localization.Drive_can_read_HD_DVD_ROM_HD_DVD_RW_and_HD_DVD_R);

                break;
            default:
            {
                sb.Append(ftr.HDDVDRAM
                              ? Localization.Drive_can_read_HD_DVD_ROM_HD_DVD_RW_and_HD_DVD_RAM
                              : Localization.Drive_can_read_HD_DVD_ROM_and_HD_DVD_RW);

                break;
            }
        }

        if(ftr.Current)
            sb.AppendLine(Localization.current);
        else
            sb.AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0051(Feature_0051? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0051 ftr = feature.Value;
        var          sb  = new StringBuilder();

        switch(ftr.HDDVDR)
        {
            case true when ftr.HDDVDRAM:
                sb.Append(Localization.Drive_can_write_HD_DVD_RW_HD_DVD_R_and_HD_DVD_RAM);

                break;
            case true:
                sb.Append(Localization.Drive_can_write_HD_DVD_RW_and_HD_DVD_R);

                break;
            default:
            {
                sb.Append(ftr.HDDVDRAM
                              ? Localization.Drive_can_write_HD_DVD_RW_and_HD_DVD_RAM
                              : Localization.Drive_can_write_HD_DVD_RW);

                break;
            }
        }

        if(ftr.Current)
            sb.AppendLine(Localization.current);
        else
            sb.AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0080(Feature_0080? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0080 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.Append(Localization.Drive_is_able_to_access_Hybrid_discs);

        if(ftr.Current)
            sb.AppendLine(Localization.current);
        else
            sb.AppendLine();

        if(ftr.RI)
        {
            sb.AppendLine("\t" +
                          Localization
                             .Drive_is_able_to_maintain_the_online_format_layer_through_reset_and_power_cycling);
        }

        return sb.ToString();
    }

    public static string Prettify_0100(Feature_0100? feature) =>
        !feature.HasValue
            ? null
            : Localization.Drive_is_able_to_perform_host_and_drive_directed_power_management + "\n";

    public static string Prettify_0101(Feature_0101? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0101 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.Drive_supports_SMART);

        if(ftr.PP) sb.AppendLine("\t" + Localization.Drive_supports_the_Informational_Exceptions_Control_mode_page_1Ch);

        return sb.ToString();
    }

    public static string Prettify_0102(Feature_0102? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0102 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.MMC_Embedded_Changer);

        if(ftr.SCC) sb.AppendLine("\t" + Localization.Drive_can_change_disc_side);

        if(ftr.SDP) sb.AppendLine("\t" + Localization.Drive_is_able_to_report_slots_contents_after_a_reset_or_change);

        sb.AppendFormat("\t" + Localization.Drive_has_0_slots, ftr.HighestSlotNumber + 1).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0103(Feature_0103? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0103 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.Drive_has_an_analogue_audio_output);

        if(ftr.Scan) sb.AppendLine("\t" + Localization.Drive_supports_the_SCAN_command);

        if(ftr.SCM) sb.AppendLine("\t" + Localization.Drive_is_able_to_mute_channels_separately);

        if(ftr.SV) sb.AppendLine("\t" + Localization.Drive_supports_separate_volume_per_channel);

        sb.AppendFormat("\t" + Localization.Drive_has_0_volume_levels, ftr.VolumeLevels + 1).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0104(Feature_0104? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0104 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.Drive_supports_Microcode_Upgrade);

        if(ftr.M5)
        {
            sb.AppendLine(Localization
                             .Drive_supports_validating_the_5_bit_Mode_of_the_READ_BUFFER_and_WRITE_BUFFER_commands);
        }

        return sb.ToString();
    }

    public static string Prettify_0105(Feature_0105? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0105 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.Drive_supports_Timeout_Protect_mode_page_1Dh);

        if(!ftr.Group3) return sb.ToString();

        sb.AppendLine("\t" + Localization.Drive_supports_the_Group3_in_Timeout_Protect_mode_page_1Dh);

        if(ftr.UnitLength > 0)
            sb.AppendFormat("\t" + Localization.Drive_has_0_increase_of_Group_3_time_unit, ftr.UnitLength).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0106(Feature_0106? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0106 ftr = feature.Value;
        var          sb  = new StringBuilder();

        if(ftr.Current)
        {
            sb.AppendFormat(Localization.Drive_supports_DVD_CSS_CPPM_version_0_and_current_disc_is_encrypted,
                            ftr.CSSVersion)
              .AppendLine();
        }
        else
            sb.AppendFormat(Localization.Drive_supports_DVD_CSS_CPPM_version_0, ftr.CSSVersion).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0107(Feature_0107? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0107 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.MMC_Real_Time_Streaming_);

        if(ftr.SMP)
            sb.AppendLine("\t" + Localization.Drive_supports_Set_Minimum_Performance_with_the_SET_STREAMING_command);

        if(ftr.RBCB)
            sb.AppendLine("\t" + Localization.Drive_supports_the_block_bit_in_the_READ_BUFFER_CAPACITY_command);

        if(ftr.SCS) sb.AppendLine("\t" + Localization.Drive_supports_the_SET_CD_SPEED_command);

        if(ftr.MP2A)
        {
            sb.AppendLine("\t" +
                          Localization
                             .Drive_supports_the_Write_Speed_Performance_Descriptor_Blocks_in_the_MMC_mode_page_2Ah);
        }

        if(ftr.WSPD)
        {
            sb.AppendLine("\t" +
                          Localization
                             .Drive_supports_the_Write_Speed_data_of_GET_PERFORMANCE_and_the_WRC_field_of_SET_STREAMING);
        }

        if(ftr.SW) sb.AppendLine("\t" + Localization.Drive_supports_stream_recording);

        return sb.ToString();
    }

    public static string Prettify_0108(Feature_0108? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0108 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendFormat(Core.Drive_serial_number_0, ftr.Serial).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0109(Feature_0109? feature) =>
        !feature.HasValue ? null : Localization.Drive_is_able_to_read_media_serial_number + "\n";

    public static string Prettify_010A(Feature_010A? feature)
    {
        if(!feature.HasValue) return null;

        Feature_010A ftr = feature.Value;
        var          sb  = new StringBuilder();

        if(ftr.DCBs == null) return sb.ToString();

        foreach(uint dcb in ftr.DCBs) sb.AppendFormat(Localization.Drive_supports_DCB_0, dcb).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_010B(Feature_010B? feature)
    {
        if(!feature.HasValue) return null;

        Feature_010B ftr = feature.Value;
        var          sb  = new StringBuilder();

        if(ftr.Current)
        {
            sb.AppendFormat(Localization.Drive_supports_DVD_CPRM_version_0_and_current_disc_is_or_can_be_encrypted,
                            ftr.CPRMVersion)
              .AppendLine();
        }
        else
            sb.AppendFormat(Localization.Drive_supports_DVD_CPRM_version_0, ftr.CPRMVersion).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_010C(Feature_010C? feature)
    {
        if(!feature.HasValue) return null;

        Feature_010C ftr = feature.Value;
        var          sb  = new StringBuilder();

        var temp = new byte[4];
        temp[0] = (byte)((ftr.Century & 0xFF00) >> 8);
        temp[1] = (byte)(ftr.Century & 0xFF);
        temp[2] = (byte)((ftr.Year & 0xFF00) >> 8);
        temp[3] = (byte)(ftr.Year & 0xFF);
        string syear = Encoding.ASCII.GetString(temp);
        temp    = new byte[2];
        temp[0] = (byte)((ftr.Month & 0xFF00) >> 8);
        temp[1] = (byte)(ftr.Month & 0xFF);
        string smonth = Encoding.ASCII.GetString(temp);
        temp    = new byte[2];
        temp[0] = (byte)((ftr.Day & 0xFF00) >> 8);
        temp[1] = (byte)(ftr.Day & 0xFF);
        string sday = Encoding.ASCII.GetString(temp);
        temp    = new byte[2];
        temp[0] = (byte)((ftr.Hour & 0xFF00) >> 8);
        temp[1] = (byte)(ftr.Hour & 0xFF);
        string shour = Encoding.ASCII.GetString(temp);
        temp    = new byte[2];
        temp[0] = (byte)((ftr.Minute & 0xFF00) >> 8);
        temp[1] = (byte)(ftr.Minute & 0xFF);
        string sminute = Encoding.ASCII.GetString(temp);
        temp    = new byte[2];
        temp[0] = (byte)((ftr.Second & 0xFF00) >> 8);
        temp[1] = (byte)(ftr.Second & 0xFF);
        string ssecond = Encoding.ASCII.GetString(temp);

        try
        {
            var fwDate = new DateTime(int.Parse(syear),
                                      int.Parse(smonth),
                                      int.Parse(sday),
                                      int.Parse(shour),
                                      int.Parse(sminute),
                                      int.Parse(ssecond),
                                      DateTimeKind.Utc);

            sb.AppendFormat(Localization.Drive_firmware_is_dated_0, fwDate).AppendLine();
        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
        catch
        {
            // ignored
        }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body

        return sb.ToString();
    }

    public static string Prettify_010D(Feature_010D? feature)
    {
        if(!feature.HasValue) return null;

        Feature_010D ftr = feature.Value;
        var          sb  = new StringBuilder();

        if(ftr.Current)
        {
            sb.AppendFormat(Localization.Drive_supports_AACS_version_0_and_current_disc_is_encrypted, ftr.AACSVersion)
              .AppendLine();
        }
        else
            sb.AppendFormat(Localization.Drive_supports_AACS_version_0, ftr.AACSVersion).AppendLine();

        if(ftr.RDC) sb.AppendLine("\t" + Localization.Drive_supports_reading_the_Drive_Certificate);

        if(ftr.RMC) sb.AppendLine("\t" + Localization.Drive_supports_reading_Media_Key_Block_of_CPRM);

        if(ftr.WBE) sb.AppendLine("\t" + Localization.Drive_supports_writing_with_bus_encryption);

        if(ftr.BEC) sb.AppendLine("\t" + Localization.Drive_supports_bus_encryption);

        if(ftr.BNG)
        {
            sb.AppendLine("\t" + Localization.Drive_supports_generating_the_binding_nonce);

            if(ftr.BindNonceBlocks > 0)
            {
                sb.AppendFormat("\t" + Localization._0_media_blocks_are_required_for_the_binding_nonce,
                                ftr.BindNonceBlocks)
                  .AppendLine();
            }
        }

        if(ftr.AGIDs > 0)
            sb.AppendFormat("\t" + Localization.Drive_supports_0_AGIDs_concurrently, ftr.AGIDs).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_010E(Feature_010E? feature)
    {
        if(!feature.HasValue) return null;

        Feature_010E ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.Append(Localization.Drive_supports_DVD_Download);

        if(ftr.Current)
            sb.AppendLine(Localization.current);
        else
            sb.AppendLine();

        if(ftr.MaxScrambleExtent > 0)
        {
            sb.AppendFormat("\t" + Localization.Maximum_0_scramble_extent_information_entries, ftr.MaxScrambleExtent)
              .AppendLine();
        }

        return sb.ToString();
    }

    public static string Prettify_0110(Feature_0110? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0110 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(ftr.Current
                          ? Localization.Drive_and_currently_inserted_media_support_VCPS
                          : Localization.Drive_supports_VCPS);

        return sb.ToString();
    }

    public static string Prettify_0113(Feature_0113? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0113 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(ftr.Current
                          ? Localization.Drive_and_currently_inserted_media_support_SecurDisc
                          : Localization.Drive_supports_SecurDisc);

        return sb.ToString();
    }

    public static string Prettify_0142(Feature_0142? feature)
    {
        if(!feature.HasValue) return null;

        Feature_0142 ftr = feature.Value;
        var          sb  = new StringBuilder();

        sb.AppendLine(Localization.Drive_supports_the_Trusted_Computing_Group_Optical_Security_Subsystem_Class);

        if(ftr.Current) sb.AppendLine("\t" + Localization.Current_media_is_initialized_with_TCG_OSSC);

        if(ftr.PSAU) sb.AppendLine("\t" + Localization.Drive_supports_PSA_updates_on_write_once_media);

        if(ftr.LOSPB) sb.AppendLine("\t" + Localization.Drive_supports_linked_OSPBs);

        if(ftr.ME) sb.AppendLine("\t" + Localization.Drive_will_only_record_on_the_OSSC_Disc_Format);

        if(ftr.Profiles == null) return sb.ToString();

        for(var i = 0; i < ftr.Profiles.Length; i++)
            sb.AppendFormat("\t" + Localization.Profile_0_1, i, ftr.Profiles[i]).AppendLine();

        return sb.ToString();
    }

    public static string Prettify_0000(byte[] feature) => Prettify_0000(Decode_0000(feature));

    public static string Prettify_0001(byte[] feature) => Prettify_0001(Decode_0001(feature));

    public static string Prettify_0002(byte[] feature) => Prettify_0002(Decode_0002(feature));

    public static string Prettify_0003(byte[] feature) => Prettify_0003(Decode_0003(feature));

    public static string Prettify_0004(byte[] feature) => Prettify_0004(Decode_0004(feature));

    public static string Prettify_0010(byte[] feature) => Prettify_0010(Decode_0010(feature));

    public static string Prettify_001D(byte[] feature) => Prettify_001D(Decode_001D(feature));

    public static string Prettify_001E(byte[] feature) => Prettify_001E(Decode_001E(feature));

    public static string Prettify_001F(byte[] feature) => Prettify_001F(Decode_001F(feature));

    public static string Prettify_0020(byte[] feature) => Prettify_0020(Decode_0020(feature));

    public static string Prettify_0021(byte[] feature) => Prettify_0021(Decode_0021(feature));

    public static string Prettify_0022(byte[] feature) => Prettify_0022(Decode_0022(feature));

    public static string Prettify_0023(byte[] feature) => Prettify_0023(Decode_0023(feature));

    public static string Prettify_0024(byte[] feature) => Prettify_0024(Decode_0024(feature));

    public static string Prettify_0025(byte[] feature) => Prettify_0025(Decode_0025(feature));

    public static string Prettify_0026(byte[] feature) => Prettify_0026(Decode_0026(feature));

    public static string Prettify_0027(byte[] feature) => Prettify_0027(Decode_0027(feature));

    public static string Prettify_0028(byte[] feature) => Prettify_0028(Decode_0028(feature));

    public static string Prettify_0029(byte[] feature) => Prettify_0029(Decode_0029(feature));

    public static string Prettify_002A(byte[] feature) => Prettify_002A(Decode_002A(feature));

    public static string Prettify_002B(byte[] feature) => Prettify_002B(Decode_002B(feature));

    public static string Prettify_002C(byte[] feature) => Prettify_002C(Decode_002C(feature));

    public static string Prettify_002D(byte[] feature) => Prettify_002D(Decode_002D(feature));

    public static string Prettify_002E(byte[] feature) => Prettify_002E(Decode_002E(feature));

    public static string Prettify_002F(byte[] feature) => Prettify_002F(Decode_002F(feature));

    public static string Prettify_0030(byte[] feature) => Prettify_0030(Decode_0030(feature));

    public static string Prettify_0031(byte[] feature) => Prettify_0031(Decode_0031(feature));

    public static string Prettify_0032(byte[] feature) => Prettify_0032(Decode_0032(feature));

    public static string Prettify_0033(byte[] feature) => Prettify_0033(Decode_0033(feature));

    public static string Prettify_0035(byte[] feature) => Prettify_0035(Decode_0035(feature));

    public static string Prettify_0037(byte[] feature) => Prettify_0037(Decode_0037(feature));

    public static string Prettify_0038(byte[] feature) => Prettify_0038(Decode_0038(feature));

    public static string Prettify_003A(byte[] feature) => Prettify_003A(Decode_003A(feature));

    public static string Prettify_003B(byte[] feature) => Prettify_003B(Decode_003B(feature));

    public static string Prettify_0040(byte[] feature) => Prettify_0040(Decode_0040(feature));

    public static string Prettify_0041(byte[] feature) => Prettify_0041(Decode_0041(feature));

    public static string Prettify_0042(byte[] feature) => Prettify_0042(Decode_0042(feature));

    public static string Prettify_0050(byte[] feature) => Prettify_0050(Decode_0050(feature));

    public static string Prettify_0051(byte[] feature) => Prettify_0051(Decode_0051(feature));

    public static string Prettify_0080(byte[] feature) => Prettify_0080(Decode_0080(feature));

    public static string Prettify_0100(byte[] feature) => Prettify_0100(Decode_0100(feature));

    public static string Prettify_0101(byte[] feature) => Prettify_0101(Decode_0101(feature));

    public static string Prettify_0102(byte[] feature) => Prettify_0102(Decode_0102(feature));

    public static string Prettify_0103(byte[] feature) => Prettify_0103(Decode_0103(feature));

    public static string Prettify_0104(byte[] feature) => Prettify_0104(Decode_0104(feature));

    public static string Prettify_0105(byte[] feature) => Prettify_0105(Decode_0105(feature));

    public static string Prettify_0106(byte[] feature) => Prettify_0106(Decode_0106(feature));

    public static string Prettify_0107(byte[] feature) => Prettify_0107(Decode_0107(feature));

    public static string Prettify_0108(byte[] feature) => Prettify_0108(Decode_0108(feature));

    public static string Prettify_0109(byte[] feature) => Prettify_0109(Decode_0109(feature));

    public static string Prettify_010A(byte[] feature) => Prettify_010A(Decode_010A(feature));

    public static string Prettify_010B(byte[] feature) => Prettify_010B(Decode_010B(feature));

    public static string Prettify_010C(byte[] feature) => Prettify_010C(Decode_010C(feature));

    public static string Prettify_010D(byte[] feature) => Prettify_010D(Decode_010D(feature));

    public static string Prettify_010E(byte[] feature) => Prettify_010E(Decode_010E(feature));

    public static string Prettify_0110(byte[] feature) => Prettify_0110(Decode_0110(feature));

    public static string Prettify_0113(byte[] feature) => Prettify_0113(Decode_0113(feature));

    public static string Prettify_0142(byte[] feature) => Prettify_0142(Decode_0142(feature));

    public static SeparatedFeatures Separate(byte[] response)
    {
        var dec = new SeparatedFeatures
        {
            DataLength     = (uint)((response[0]   << 24) + (response[1] << 16) + (response[2] << 8) + response[4]),
            CurrentProfile = (ushort)((response[6] << 8)  + response[7])
        };

        uint                    offset  = 8;
        List<FeatureDescriptor> descLst = new();

        while(offset + 4 < response.Length)
        {
            var desc = new FeatureDescriptor
            {
                Code = (ushort)((response[offset + 0] << 8) + response[offset + 1]),
                Data = new byte[response[offset + 3] + 4]
            };

            if(desc.Data.Length + offset > response.Length) desc.Data = new byte[response.Length - offset];

            Array.Copy(response, offset, desc.Data, 0, desc.Data.Length);
            offset += (uint)desc.Data.Length;

            descLst.Add(desc);
        }

        dec.Descriptors = descLst.ToArray();

        return dec;
    }

#region Nested type: FeatureDescriptor

    public struct FeatureDescriptor
    {
        public ushort Code;
        public byte[] Data;
    }

#endregion

#region Nested type: SeparatedFeatures

    public struct SeparatedFeatures
    {
        public uint                DataLength;
        public ushort              CurrentProfile;
        public FeatureDescriptor[] Descriptors;
    }

#endregion
}