// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Features.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;


namespace DiscImageChef.Decoders.SCSI.MMC
{

    /// <summary>
    /// MMC Feature enumeration
    /// </summary>
    public enum FeatureNumber : ushort
    {
        /// <summary>
        /// Lists all profiles
        /// </summary>
        ProfileList = 0x0000,
        /// <summary>
        /// Mandatory behaviour
        /// </summary>
        Core = 0x0001,
        /// <summary>
        /// Operational changes
        /// </summary>
        Morphing = 0x0002,
        /// <summary>
        /// Removable medium
        /// </summary>
        Removable = 0x0003,
        /// <summary>
        /// Ability to control write protection status
        /// </summary>
        WriteProtect = 0x0004,
        /// <summary>
        /// Ability to read sectors with random addressing
        /// </summary>
        RandomRead = 0x0010,
        /// <summary>
        /// Reads on OSTA Multi-Read
        /// </summary>
        MultiRead = 0x001D,
        /// <summary>
        /// Able to read CD structures
        /// </summary>
        CDRead = 0x001E,
        /// <summary>
        /// Able to read DVD structures
        /// </summary>
        DVDRead = 0x001F,
        /// <summary>
        /// Ability to write sectors with random addressing
        /// </summary>
        RandomWrite = 0x0020,
        /// <summary>
        /// Ability to sequentially write
        /// </summary>
        IncrementalWrite = 0x0021,
        /// <summary>
        /// Support for media that requires erase before write
        /// </summary>
        SectorErasable = 0x0022,
        /// <summary>
        /// Supports formatting media
        /// </summary>
        Formattable = 0x0023,
        /// <summary>
        /// Ability to provide defect-free space
        /// </summary>
        HardwareDefectMgmt = 0x0024,
        /// <summary>
        /// Supports for write-once media in random order
        /// </summary>
        WriteOnce = 0x0025,
        /// <summary>
        /// Supports for media that shall be written from blocking boundaries
        /// </summary>
        RestrictedOverwrite = 0x0026,
        /// <summary>
        /// Supports high speed CD-RW
        /// </summary>
        CDRWCAV = 0x0027,
        /// <summary>
        /// Read and optionally write MRW
        /// </summary>
        MRW = 0x0028,
        /// <summary>
        /// Ability to control RECOVERED ERROR reporting
        /// </summary>
        EnDefectReport = 0x0029,
        /// <summary>
        /// Ability to recognize, read and optionally write DVD+RW
        /// </summary>
        DVDRWPlus = 0x002A,
        /// <summary>
        /// Ability to read DVD+R
        /// </summary>
        DVDRPlus = 0x002B,
        /// <summary>
        /// Ability to write CD in Track-at-Once
        /// </summary>
        CDTAO = 0x002D,
        /// <summary>
        /// Ability to write CD in Session-at-Once or RAW
        /// </summary>
        CDMastering = 0x002E,
        /// <summary>
        /// Ability to write DVD structures
        /// </summary>
        DVDRWrite = 0x002F,
        /// <summary>
        /// Ability to read DDCD
        /// </summary>
        DDCD = 0x0030,
        /// <summary>
        /// Ability to write DDCD-R
        /// </summary>
        DDCDR = 0x0031,
        /// <summary>
        /// Ability to write DDCD-RW
        /// </summary>
        DDCDRW = 0x0032,
        /// <summary>
        /// Ability to record in layer jump mode
        /// </summary>
        LayerJump = 0x0033,
        /// <summary>
        /// Ability to perform Layer Jump recording on Rigid Restricted Overwrite
        /// </summary>
        LJRigid = 0x0034,
        /// <summary>
        /// Ability to stop the long immediate operation
        /// </summary>
        StopLong = 0x0035,
        /// <summary>
        /// Ability to report CD-RW media sub-types supported for write
        /// </summary>
        CDRWMediaWrite = 0x0037,
        /// <summary>
        /// Logical block overwrite service on BD-R formatted as SRM+POW
        /// </summary>
        BDRPOW = 0x0038,
        /// <summary>
        /// Ability to read DVD+RW DL
        /// </summary>
        DVDRWDLPlus = 0x003A,
        /// <summary>
        /// Ability to read DVD+R DL
        /// </summary>
        DVDRDLPlus = 0x003B,
        /// <summary>
        /// Ability to read BD discs
        /// </summary>
        BDRead = 0x0040,
        /// <summary>
        /// Ability to write BD discs
        /// </summary>
        BDWrite = 0x0041,
        /// <summary>
        /// Timely, Safe Recording
        /// </summary>
        TSR = 0x0042,
        /// <summary>
        /// Ability to read HD DVD
        /// </summary>
        HDDVDRead = 0x0050,
        /// <summary>
        /// Ability to write HD DVD
        /// </summary>
        HDDVDWrite = 0x0051,
        /// <summary>
        /// Ability to write HD DVD-RW fragmented
        /// </summary>
        HDDVDRWFragment = 0x0052,
        /// <summary>
        /// Supports some Hybrid Discs
        /// </summary>
        Hybrid = 0x0080,
        /// <summary>
        /// Host and device directed power management
        /// </summary>
        PowerMgmt = 0x0100,
        /// <summary>
        /// Supports S.M.A.R.T.
        /// </summary>
        SMART = 0x0101,
        /// <summary>
        /// Single machanism multiple disc changer
        /// </summary>
        Changer = 0x0102,
        /// <summary>
        /// Ability to play CD audio to an analogue output
        /// </summary>
        CDAudioExt = 0x0103,
        /// <summary>
        /// Ability to accept new microcode
        /// </summary>
        MicrocodeUpgrade = 0x0104,
        /// <summary>
        /// Ability to respond to all commands within a specific time
        /// </summary>
        Timeout = 0x0105,
        /// <summary>
        /// Supports DVD CSS/CPPM
        /// </summary>
        CSS = 0x0106,
        /// <summary>
        /// Ability to read and write using host requested performance parameters
        /// </summary>
        RTS = 0x0107,
        /// <summary>
        /// Drive has a unique identifier
        /// </summary>
        DriveSerial = 0x0108,
        /// <summary>
        /// Ability to return unique Media Serial Number
        /// </summary>
        MediaSerial = 0x0109,
        /// <summary>
        /// Ability to read and/or write DCBs
        /// </summary>
        DCBs = 0x010A,
        /// <summary>
        /// Supports DVD CPRM
        /// </summary>
        CPRM = 0x010B,
        /// <summary>
        /// Firmware creation date report
        /// </summary>
        FirmwareInfo = 0x010C,
        /// <summary>
        /// Ability to decode and optionally encode AACS
        /// </summary>
        AACS = 0x010D,
        /// <summary>
        /// Ability to perform DVD CSS managed recording
        /// </summary>
        CSSManagedRec = 0x010E,
        /// <summary>
        /// Ability to decode and optionally encode VCPS
        /// </summary>
        VCPS = 0x0110,
        /// <summary>
        /// Supports SecurDisc
        /// </summary>
        SecurDisc = 0x0113,
        /// <summary>
        /// TCG Optical Security Subsystem Class
        /// </summary>
        OSSC = 0x0142
    }

    /// <summary>
    /// MMC Profile enumeration
    /// </summary>
    public enum ProfileNumber : ushort
    {
        /// <summary>
        /// Not to use
        /// </summary>
        Reserved = 0x0000,
        /// <summary>
        /// Non-removable disk profile
        /// </summary>
        NonRemovable = 0x0001,
        /// <summary>
        /// Rewritable with removable media
        /// </summary>
        Removable = 0x0002,
        /// <summary>
        /// Magneto-Optical with sector erase
        /// </summary>
        MOErasable = 0x0003,
        /// <summary>
        /// Optical write once
        /// </summary>
        OpticalWORM = 0x0004,
        /// <summary>
        /// Advance Storage - Magneto-Optical
        /// </summary>
        ASMO = 0x0005,
        /// <summary>
        /// Read-only Compact Disc
        /// </summary>
        CDROM = 0x0008,
        /// <summary>
        /// Write-once Compact Disc
        /// </summary>
        CDR = 0x0009,
        /// <summary>
        /// Re-writable Compact Disc
        /// </summary>
        CDRW = 0x000A,
        /// <summary>
        /// Read-only DVD
        /// </summary>
        DVDROM = 0x0010,
        /// <summary>
        /// Write-once sequentially recorded DVD-R
        /// </summary>
        DVDRSeq = 0x0011,
        /// <summary>
        /// DVD-RAM
        /// </summary>
        DVDRAM = 0x0012,
        /// <summary>
        /// Restricted overwrite DVD-RW
        /// </summary>
        DVDRWRes = 0x0013,
        /// <summary>
        /// Sequential recording DVD-RW
        /// </summary>
        DVDRWSeq = 0x0014,
        /// <summary>
        /// Sequential recording DVD-R DL
        /// </summary>
        DVDRDLSeq = 0x0015,
        /// <summary>
        /// Layer jump recording DVD-R DL
        /// </summary>
        DVDRDLJump = 0x0016,
        /// <summary>
        /// DVD+RW DL
        /// </summary>
        DVDRWDL = 0x0017,
        /// <summary>
        /// DVD-Download
        /// </summary>
        DVDDownload = 0x0018,
        /// <summary>
        /// DVD+RW
        /// </summary>
        DVDRWPlus = 0x001A,
        /// <summary>
        /// DVD+R
        /// </summary>
        DVDRPlus = 0x001B,
        /// <summary>
        /// DDCD-ROM
        /// </summary>
        DDCDROM = 0x0020,
        /// <summary>
        /// DDCD-R
        /// </summary>
        DDCDR = 0x0021,
        /// <summary>
        /// DDCD-RW
        /// </summary>
        DDCDRW = 0x0022,
        /// <summary>
        /// DVD+RW DL
        /// </summary>
        DVDRWDLPlus = 0x002A,
        /// <summary>
        /// DVD+R DL
        /// </summary>
        DVDRDLPlus = 0x002B,
        /// <summary>
        /// BD-ROM
        /// </summary>
        BDROM = 0x0040,
        /// <summary>
        /// BD-R SRM
        /// </summary>
        BDRSeq = 0x0041,
        /// <summary>
        /// BD-R RRM
        /// </summary>
        BDRRdm = 0x0042,
        /// <summary>
        /// BD-RE
        /// </summary>
        BDRE = 0x0043,
        /// <summary>
        /// HD DVD-ROM
        /// </summary>
        HDDVDROM = 0x0050,
        /// <summary>
        /// HD DVD-R
        /// </summary>
        HDDVDR = 0x0051,
        /// <summary>
        /// HD DVD-RAM
        /// </summary>
        HDDVDRAM = 0x0052,
        /// <summary>
        /// HD DVD-RW
        /// </summary>
        HDDVDRW = 0x0053,
        /// <summary>
        /// HD DVD-R DL
        /// </summary>
        HDDVDRDL = 0x0058,
        /// <summary>
        /// HD DVD-RW DL
        /// </summary>
        HDDVDRWDL = 0x005A,
        /// <summary>
        /// Drive does not conform to any profiles
        /// </summary>
        Unconforming = 0xFFFF
    }

    public enum PhysicalInterfaces : uint
    {
        /// <summary>
        /// Unspecified physical interface
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// SCSI
        /// </summary>
        SCSI = 1,
        /// <summary>
        /// ATAPI
        /// </summary>
        ATAPI = 2,
        /// <summary>
        /// IEEE-1394/1995
        /// </summary>
        IEEE1394 = 3,
        /// <summary>
        /// IEEE-1394A
        /// </summary>
        IEEE1394A = 4,
        /// <summary>
        /// Fibre Channel
        /// </summary>
        FC = 5,
        /// <summary>
        /// IEEE-1394B
        /// </summary>
        IEEE1394B = 6,
        /// <summary>
        /// Serial ATAPI
        /// </summary>
        SerialATAPI = 7,
        /// <summary>
        /// USB
        /// </summary>
        USB = 8,
        /// <summary>
        /// Vendor unique
        /// </summary>
        Vendor = 0xFFFF
    }

    public struct Profile
    {
        public ProfileNumber Number;
        public bool Current;
    }

    /// <summary>
    /// Profile List Feature (0000h)
    /// </summary>
    public struct Feature_0000
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// All supported profiles
        /// </summary>
        public Profile[] Profiles;
    }

    /// <summary>
    /// Core Feature (0001h)
    /// </summary>
    public struct Feature_0001
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Currently in-use physical interface standard
        /// </summary>
        public PhysicalInterfaces PhysicalInterfaceStandard;
    }

    /// <summary>
    /// Morphing Feature (0002h)
    /// </summary>
    public struct Feature_0002
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Supports asynchronous GET EVENT/STATUS NOTIFICATION
        /// </summary>
        public bool Async;
    }

    /// <summary>
    /// Removable Medium Feature (0003h)
    /// </summary>
    public struct Feature_0003
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Mechanism type
        /// </summary>
        public byte LoadingMechanismType;
        /// <summary>
        /// Device can eject medium
        /// </summary>
        public bool Eject;
        /// <summary>
        /// Device starts in medium ejection/insertion allow
        /// </summary>
        public bool PreventJumper;
        /// <summary>
        /// Medium is currently locked
        /// </summary>
        public bool Lock;
    }

    /// <summary>
    /// Random Readable Feature (0010h)
    /// </summary>
    public struct Feature_0010
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Bytes per logical block
        /// </summary>
        public uint LogicalBlockSize;
        /// <summary>
        /// Number of logical blocks per device readable unit
        /// </summary>
        public ushort Blocking;
        /// <summary>
        /// Read/Write Error Recovery page is present
        /// </summary>
        public bool PP;
    }

    /// <summary>
    /// Multi-Read Feature (001Dh)
    /// </summary>
    public struct Feature_001D
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
    }

    /// <summary>
    /// CD Read Feature (001Eh)
    /// </summary>
    public struct Feature_001E
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Supports C2 Error Pointers
        /// </summary>
        public bool C2;
        /// <summary>
        /// Can read CD-Text with READ TOC/PMA/ATIP
        /// </summary>
        public bool CDText;
    }

    /// <summary>
    /// DVD Feature (001Fh)
    /// </summary>
    public struct Feature_001F
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
    }

    /// <summary>
    /// Random Writable Feature (0020h)
    /// </summary>
    public struct Feature_0020
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Last logical block address
        /// </summary>
        public uint LastLBA;
        /// <summary>
        /// Bytes per logical block
        /// </summary>
        public uint LogicalBlockSize;
        /// <summary>
        /// Number of logical blocks per device readable unit
        /// </summary>
        public ushort Blocking;
        /// <summary>
        /// Read/Write Error Recovery page is present
        /// </summary>
        public bool PP;
    }

    /// <summary>
    /// Incremental Streaming Writable Feature (0021h)
    /// </summary>
    public struct Feature_0021
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Bitmask of supported data types
        /// </summary>
        public ushort DataTypeSupported;
        /// <summary>
        /// Logical blocks per link
        /// </summary>
        public byte[] LinkSizes;
    }

    /// <summary>
    /// Sector Erasable Feature (0022h)
    /// </summary>
    public struct Feature_0022
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
    }

    /// <summary>
    /// Formattable Feature (0023h)
    /// </summary>
    public struct Feature_0023
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
    }

    /// <summary>
    /// Defect Management Feature (0024h)
    /// </summary>
    public struct Feature_0024
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
    }

    /// <summary>
    /// Write Once Feature (0025h)
    /// </summary>
    public struct Feature_0025
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Last logical block address
        /// </summary>
        public uint LastLBA;
        /// <summary>
        /// Number of logical blocks per device readable unit
        /// </summary>
        public ushort Blocking;
        /// <summary>
        /// Read/Write Error Recovery page is present
        /// </summary>
        public bool PP;
    }

    /// <summary>
    /// Restricted Overwrite Feature (0026h)
    /// </summary>
    public struct Feature_0026
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
    }

    /// <summary>
    /// CD Track at Once Feature (002Dh)
    /// </summary>
    public struct Feature_002D
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Can perform test writes
        /// </summary>
        public bool TestWrite;
        /// <summary>
        /// Supports overwriting a TAO track with another
        /// </summary>
        public bool CDRW;
        /// <summary>
        /// Can write R-W subchannels with user provided data
        /// </summary>
        public bool RWSubchannel;
        /// <summary>
        /// Bitmask of supported data types
        /// </summary>
        public ushort DataTypeSupported;
    }

    /// <summary>
    /// CD Mastering (Session at Once) Feature (002Eh)
    /// </summary>
    public struct Feature_002E
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Can write in Session at Once
        /// </summary>
        public bool SAO;
        /// <summary>
        /// Can write multi-session in RAW
        /// </summary>
        public bool RAWMS;
        /// <summary>
        /// Can write in RAW
        /// </summary>
        public bool RAW;
        /// <summary>
        /// Can perform test writes
        /// </summary>
        public bool TestWrite;
        /// <summary>
        /// Can overwrite previously recorded data
        /// </summary>
        public bool CDRW;
        /// <summary>
        /// Can write R-W subchannels with user provided data
        /// </summary>
        public bool RW;
        /// <summary>
        /// Maximum length of a Cue Sheet for Session at Once
        /// </summary>
        public uint MaxCueSheet;
    }

    /// <summary>
    /// DVD-R Write Feature (002Fh)
    /// </summary>
    public struct Feature_002F
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Buffer Under-run protection
        /// </summary>
        public bool BUF;
        /// <summary>
        /// Test write
        /// </summary>
        public bool TestWrite;
    }

    /// <summary>
    /// Power Management Feature (0100h)
    /// </summary>
    public struct Feature_0100
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
    }

    /// <summary>
    /// Embedded Changer Feature (0102h)
    /// </summary>
    public struct Feature_0102
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Side change capable
        /// </summary>
        public bool SCC;
        /// <summary>
        /// Supports Disc Present
        /// </summary>
        public bool SDP;
    }

    /// <summary>
    /// CD Audio External Play Feature (0103h)
    /// </summary>
    public struct Feature_0103
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// Supports SCAN command
        /// </summary>
        public bool Scan;
        /// <summary>
        /// Separate Channel Mute
        /// </summary>
        public bool SCM;
        /// <summary>
        /// Separate Volume
        /// </summary>
        public bool SV;
    }

    /// <summary>
    /// Microcode Upgrade Feature (0104h)
    /// </summary>
    public struct Feature_0104
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
    }

    /// <summary>
    /// Time-Out Feature (0105h)
    /// </summary>
    public struct Feature_0105
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
    }

    /// <summary>
    /// DVD-CSS Feature (0106h)
    /// </summary>
    public struct Feature_0106
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// CSS version
        /// </summary>
        public byte CSSVersion;
    }

    /// <summary>
    /// Real Time Streaming Feature (0107h)
    /// </summary>
    public struct Feature_0107
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
    }

    /// <summary>
    /// Logical Unit serial number (0108h)
    /// </summary>
    public struct Feature_0108
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        /// <summary>
        /// LUN serial number
        /// </summary>
        public string Serial;
    }

    /// <summary>
    /// Disc Control Blocks Feature (010Ah)
    /// </summary>
    public struct Feature_010A
    {
        /// <summary>
        /// Feature version
        /// </summary>
        public byte Version;
        /// <summary>
        /// Feature is persistent
        /// </summary>
        public bool Persistent;
        /// <summary>
        /// Feature is currently in use
        /// </summary>
        public bool Current;
        public uint[] DCBs;
    }

    public static class Features
    {
    }
}

