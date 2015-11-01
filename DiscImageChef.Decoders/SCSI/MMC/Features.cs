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
using System.Collections.Generic;


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
        /// <summary>
        /// Supports EVPD, Page Code and 16-bit Allocation Length as defined in SPC-3
        /// </summary>
        public bool INQ2;
        /// <summary>
        /// Supports Device Busy Event
        /// </summary>
        public bool DBE;
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
        /// Supports Operational Change Request/Nofitication Class Events
        /// of GET EVENT/STATUS NOTIFICATION
        /// </summary>
        public bool OCEvent;
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
        /// Drive is able to load the medium
        /// </summary>
        public bool Load;
        /// <summary>
        /// Device can eject medium
        /// </summary>
        public bool Eject;
        /// <summary>
        /// Device starts in medium ejection/insertion allow
        /// </summary>
        public bool PreventJumper;
        /// <summary>
        /// Reports Device Busy Class events during medium loading/unloading
        /// </summary>
        public bool DBML;
        /// <summary>
        /// Medium is currently locked
        /// </summary>
        public bool Lock;
    }

    /// <summary>
    /// Write Protect Feature (0004h)
    /// </summary>
    public struct Feature_0004
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
        /// Drive can read/write Disc Write Protect PAC on BD-R/-RE media
        /// </summary>
        public bool DWP;
        /// <summary>
        /// Supports reading/writing Write Inhibit DCB on DVD+RW media.
        /// </summary>
        public bool WDCB;
        /// <summary>
        /// Supports PWP status
        /// </summary>
        public bool SPWP;
        /// <summary>
        /// Supports SWPP bit of mode page 1Dh
        /// </summary>
        public bool SSWPP;
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
        /// Supports DAP bit in READ CD and READ CD MSF
        /// </summary>
        public bool DAP;
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
    /// DVD Read Feature (001Fh)
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
        /// <summary>
        /// Compliant with DVD Multi Drive Read-only specifications
        /// </summary>
        public bool MULTI110;
        /// <summary>
        /// Supports reading all DVD-RW DL
        /// </summary>
        public bool DualRW;
        /// <summary>
        /// Supports reading all DVD-R DL including remapping
        /// </summary>
        public bool DualR;
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
        /// Can report Track Resources Information of READ DISC INFORMATION
        /// </summary>
        public bool TRIO;
        /// <summary>
        /// Supports Address Mode in RESERVE TRACK
        /// </summary>
        public bool ARSV;
        /// <summary>
        /// Zero loss linking
        /// </summary>
        public bool BUF;
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
        /// <summary>
        /// Supports formatting BD-RE without spare area
        /// </summary>
        public bool RENoSA;
        /// <summary>
        /// Supports expansion of the spare area on BD-RE
        /// </summary>
        public bool Expand;
        /// <summary>
        /// Supports FORMAT type 30h sub-type 11b
        /// </summary>
        public bool QCert;
        /// <summary>
        /// Supports FORMAT type 30h sub-type 10b
        /// </summary>
        public bool Cert;
        /// <summary>
        /// Supports FORMAT type 18h
        /// </summary>
        public bool FRF;
        /// <summary>
        /// Supports FORMAT type 00h/32h sub-type 10b on BD-R
        /// </summary>
        public bool RRM;
    }

    /// <summary>
    /// Hardware Defect Management Feature (0024h)
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
        /// <summary>
        /// Supports READ DISC STRUCTURE with Format Code 0Ah (Spare Area Information)
        /// </summary>
        public bool SSA;
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
    /// CD-RW CAV Write Feature (0027h)
    /// </summary>
    public struct Feature_0027
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
    /// MRW Feature (0028h)
    /// </summary>
    public struct Feature_0028
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
        /// Can read DVD+MRW discs
        /// </summary>
        public bool DVDPRead;
        /// <summary>
        /// Can write DVD+MRW discs
        /// </summary>
        public bool DVDPWrite;
        /// <summary>
        /// Can format and write to CD-MRW discs
        /// </summary>
        public bool Write;
    }

    /// <summary>
    /// Enhanced Defect Reporting Feature (0029h)
    /// </summary>
    public struct Feature_0029
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
        /// Supports DRT-DM
        /// </summary>
        public bool DRTDM;
        /// <summary>
        /// Maximum number of DBI cache zones device can handle separately
        /// </summary>
        public byte DBICacheZones;
        /// <summary>
        /// Number of entries in worst case to case DBI overflow
        /// </summary>
        public ushort Entries;
    }

    /// <summary>
    /// DVD+RW Feature (002Ah)
    /// </summary>
    public struct Feature_002A
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
        /// Can format DVD+RW discs
        /// </summary>
        public bool Write;
        /// <summary>
        /// FORMAT UNIT supports quick start formatting
        /// </summary>
        public bool QuickStart;
        /// <summary>
        /// Drive only supports read compatibility stop
        /// </summary>
        public bool CloseOnly;
    }

    /// <summary>
    /// DVD+R Feature (002Bh)
    /// </summary>
    public struct Feature_002B
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
        /// Can write DVD+R
        /// </summary>
        public bool Write;
    }

    /// <summary>
    /// Rigid Restricted Overwrite Feature (002Ch)
    /// </summary>
    public struct Feature_002C
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
        /// Can generate Defect Status Data during formatting
        /// </summary>
        public bool DSDG;
        /// <summary>
        /// Can read Defect Status Data recorded on medium
        /// </summary>
        public bool DSDR;
        /// <summary>
        /// Supports writing on an intermediate state Session and quick formatting
        /// </summary>
        public bool Intermediate;
        /// <summary>
        /// Supports BLANK command types 00h and 01h
        /// </summary>
        public bool Blank;
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
        /// Supports zero loss linking
        /// </summary>
        public bool BUF;
        /// <summary>
        /// Supports writing R-W subchannels in raw mode
        /// </summary>
        public bool RWRaw;
        /// <summary>
        /// Supports writing R-W subchannels in packed mode
        /// </summary>
        public bool RWPack;
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
        /// Supports zero loss linking
        /// </summary>
        public bool BUF;
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
    /// DVD-R/-RW Write Feature (002Fh)
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
        /// Supports writing DVD-R DL
        /// </summary>
        public bool RDL;
        /// <summary>
        /// Test write
        /// </summary>
        public bool TestWrite;
        /// <summary>
        /// Can write and erase DVD-RW
        /// </summary>
        public bool DVDRW;
    }

    /// <summary>
    /// Double Density CD Read Feature (0030h)
    /// </summary>
    public struct Feature_0030
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
    /// Double Density CD-R Write Feature (0031h)
    /// </summary>
    public struct Feature_0031
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
        /// Test write
        /// </summary>
        public bool TestWrite;
    }

    /// <summary>
    /// Double Density CD-RW Write Feature (0032h)
    /// </summary>
    public struct Feature_0032
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
        /// Supports quick formatting
        /// </summary>
        public bool Intermediate;
        /// <summary>
        /// Supports BLANK command
        /// </summary>
        public bool Blank;
    }

    /// <summary>
    /// Layer Jump Recording Feature (0033h)
    /// </summary>
    public struct Feature_0033
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
        public byte[] LinkSizes;
    }

    /// <summary>
    /// Stop Long Operation Feature (0035h)
    /// </summary>
    public struct Feature_0035
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
    /// CD-RW Media Write Support Feature (0037h)
    /// </summary>
    public struct Feature_0037
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
        /// Bitmask of supported CD-RW media sub-types
        /// </summary>
        public byte SubtypeSupport;
    }

    /// <summary>
    /// BD-R Pseudo-Overwrite (POW) Feature (0038h)
    /// </summary>
    public struct Feature_0038
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
    /// DVD+RW Dual Layer Feature (003Ah)
    /// </summary>
    public struct Feature_003A
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
        /// Can format DVD+RW DL discs
        /// </summary>
        public bool Write;
        /// <summary>
        /// FORMAT UNIT supports quick start formatting
        /// </summary>
        public bool QuickStart;
        /// <summary>
        /// Drive only supports read compatibility stop
        /// </summary>
        public bool CloseOnly;
    }

    /// <summary>
    /// DVD+R Dual Layer Feature (003Bh)
    /// </summary>
    public struct Feature_003B
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
        /// Can format DVD+R DL discs
        /// </summary>
        public bool Write;
        /// <summary>
        /// FORMAT UNIT supports quick start formatting
        /// </summary>
        public bool QuickStart;
        /// <summary>
        /// Drive only supports read compatibility stop
        /// </summary>
        public bool CloseOnly;
    }

    /// <summary>
    /// BD Read Feature (0040h)
    /// </summary>
    public struct Feature_0040
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
        /// Can read BCA
        /// </summary>
        public bool BCA;
        /// <summary>
        /// Supports reading BD-RE Ver.2
        /// </summary>
        public bool RE2;
        /// <summary>
        /// Supports reading BD-RE Ver.1
        /// </summary>
        public bool RE1;
        /// <summary>
        /// Obsolete
        /// </summary>
        public bool OldRE;
        /// <summary>
        /// Supports reading BD-R Ver.1
        /// </summary>
        public bool R;
        /// <summary>
        /// Obsolete
        /// </summary>
        public bool OldR;
        /// <summary>
        /// Supports reading BD-ROM Ver.1
        /// </summary>
        public bool ROM;
        /// <summary>
        /// Obsolete
        /// </summary>
        public bool OldROM;
    }

    /// <summary>
    /// BD Write Feature (0041h)
    /// </summary>
    public struct Feature_0041
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
        /// Supports verify not required
        /// </summary>
        public bool SVNR;
        /// <summary>
        /// Supports writing BD-RE Ver.2
        /// </summary>
        public bool RE2;
        /// <summary>
        /// Supports writing BD-RE Ver.1
        /// </summary>
        public bool RE1;
        /// <summary>
        /// Obsolete
        /// </summary>
        public bool OldRE;
        /// <summary>
        /// Supports writing BD-R Ver.1
        /// </summary>
        public bool R;
        /// <summary>
        /// Obsolete
        /// </summary>
        public bool OldR;
    }

    /// <summary>
    /// TSR Feature (0042h)
    /// </summary>
    public struct Feature_0042
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
    /// HD DVD Read Feature (0050h)
    /// </summary>
    public struct Feature_0050
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
        /// Can read HD DVD-R
        /// </summary>
        public bool HDDVDR;
        /// <summary>
        /// Can read HD DVD-RAM
        /// </summary>
        public bool HDDVDRAM;
    }


    /// <summary>
    /// HD DVD Write Feature (0051h)
    /// </summary>
    public struct Feature_0051
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
        /// Can write HD DVD-R
        /// </summary>
        public bool HDDVDR;
        /// <summary>
        /// Can write HD DVD-RAM
        /// </summary>
        public bool HDDVDRAM;
    }

    /// <summary>
    /// Hybrid Disc Feature (0080h)
    /// </summary>
    public struct Feature_0080
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
        /// Reset immunity
        /// </summary>
        public bool RI;
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
    /// S.M.A.R.T. Feature (0101h)
    /// </summary>
    public struct Feature_0101
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
        /// Mode Page 1Ch is present
        /// </summary>
        public bool PP;
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
        /// <summary>
        /// Number of slots - 1
        /// </summary>
        public byte HighestSlotNumber;
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
        /// <summary>
        /// Number of volume levels
        /// </summary>
        public ushort VolumeLevels;
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
        /// <summary>
        /// Supports validating 5-bit mode field of READ BUFFER and WRITE BUFFER commands.
        /// </summary>
        public bool M5;
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
        /// <summary>
        /// Supports G3Enable bit and Group3 Timeout field in Mode Page 1Dh
        /// </summary>
        public bool Group3;
        /// <summary>
        /// Indicates a unit of block length, in sectors, corresponding to increase a unit of Group 3 time unit
        /// </summary>
        public ushort UnitLength;
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
        /// <summary>
        /// Supports READ BUFFER CAPACITY with block bit set
        /// </summary>
        public bool RBCB;
        /// <summary>
        /// Supports SET CD SPEED
        /// </summary>
        public bool SCS;
        /// <summary>
        /// Has Mode Page 2Ah with Speed Performance Descriptors
        /// </summary>
        public bool MP2A;
        /// <summary>
        /// Supports type 03h of GET PERFORMANCE
        /// </summary>
        public bool WSPD;
        /// <summary>
        /// Supports stream recording
        /// </summary>
        public bool SW;
    }

    /// <summary>
    /// Drive serial number (0108h)
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
        /// Drive serial number
        /// </summary>
        public string Serial;
    }

    /// <summary>
    /// Media Serial Number Feature (0109h)
    /// </summary>
    public struct Feature_0109
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

    /// <summary>
    /// DVD CPRM Feature (010Bh)
    /// </summary>
    public struct Feature_010B
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
        /// CPRM version
        /// </summary>
        public byte CPRMVersion;
    }

    /// <summary>
    /// Firmware Information Feature (010Ch)
    /// </summary>
    public struct Feature_010C
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
        public ushort Century;
        public ushort Year;
        public ushort Month;
        public ushort Day;
        public ushort Hour;
        public ushort Minute;
        public ushort Second;
    }

    /// <summary>
    /// AACS Feature (010Dh)
    /// </summary>
    public struct Feature_010D
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
        /// Drive supports reading drive certificate
        /// </summary>
        public bool RDC;
        /// <summary>
        /// Drive can read media key block of CPRM
        /// </summary>
        public bool RMC;
        /// <summary>
        /// Drive can write bus encrypted blocks
        /// </summary>
        public bool WBE;
        /// <summary>
        /// Drive supports bus encryption
        /// </summary>
        public bool BEC;
        /// <summary>
        /// Drive supports generating the binding nonce
        /// </summary>
        public bool BNG;
        /// <summary>
        /// Blocks required to store the binding nonce for the media
        /// </summary>
        public byte BindNonceBlocks;
        /// <summary>
        /// Maximum number of AGIDs supported concurrently
        /// </summary>
        public byte AGIDs;
        /// <summary>
        /// AACS version
        /// </summary>
        public byte AACSVersion;
    }

    /// <summary>
    /// DVD CSS Managed Recording Feature (010Eh)
    /// </summary>
    public struct Feature_010E
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
        /// Maximum number of Scramble Extent information entries in a single SEND DISC STRUCTURE
        /// </summary>
        public byte MaxScrambleExtent;
    }

    /// <summary>
    /// SecurDisc Feature (0113h)
    /// </summary>
    public struct Feature_0113
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
    /// OSSC Feature (0142h)
    /// </summary>
    public struct Feature_0142
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
        /// Supports PSA updates on write-once media
        /// </summary>
        public bool PSAU;
        /// <summary>
        /// Supports linked OSPBs
        /// </summary>
        public bool LOSPB;
        /// <summary>
        /// Restricted to recording only OSSC disc format
        /// </summary>
        public bool ME;
        public ushort[] Profiles;
    }

    /// <summary>
    /// VCPS Feature (0110h)
    /// </summary>
    public struct Feature_0110
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

    public static class Features
    {
        public static Feature_0000? Decode_0000(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0000)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0000 decoded = new Feature_0000();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            int offset = 4;
            List<Profile> listProfiles = new List<Profile>();
            while (offset < feature.Length)
            {
                Profile prof = new Profile();
                prof.Number = (ProfileNumber)((feature[offset] << 8) + feature[offset + 1]);
                prof.Current |= (feature[offset + 2] & 0x01) == 0x01;
                listProfiles.Add(prof);
                offset += 4;
            }
            decoded.Profiles = listProfiles.ToArray();

            return decoded;
        }

        public static Feature_0001? Decode_0001(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0001)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0001 decoded = new Feature_0001();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.PhysicalInterfaceStandard = (PhysicalInterfaces)(feature[4] << 24 + feature[5] << 16 + feature[6] << 8 + feature[7]);

            if (decoded.Version >= 1 && feature.Length >= 12)
                decoded.DBE |= (feature[8] & 0x01) == 0x01;

            if (decoded.Version >= 2 && feature.Length >= 12)
                decoded.INQ2 |= (feature[8] & 0x02) == 0x02;

            return decoded;
        }

        public static Feature_0002? Decode_0002(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0002)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0002 decoded = new Feature_0002();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.Async |= (feature[4] & 0x01) == 0x01;

            if (decoded.Version >= 1)
                decoded.OCEvent |= (feature[4] & 0x02) == 0x02;

            return decoded;
        }

        public static Feature_0003? Decode_0003(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0003)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0003 decoded = new Feature_0003();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.LoadingMechanismType = (byte)((feature[4] & 0xE0) >> 5);
                decoded.Eject |= (feature[4] & 0x08) == 0x08;
                decoded.PreventJumper |= (feature[4] & 0x04) == 0x04;
                decoded.Lock |= (feature[4] & 0x01) == 0x01;
            }

            if (decoded.Version >= 2)
            {
                decoded.Load |= (feature[4] & 0x10) == 0x10;
                decoded.DBML |= (feature[4] & 0x02) == 0x02;
            }

            return decoded;
        }

        public static Feature_0004? Decode_0004(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0004)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0004 decoded = new Feature_0004();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.SPWP |= (feature[4] & 0x02) == 0x02;
                decoded.SSWPP |= (feature[4] & 0x01) == 0x01;
            }

            if (decoded.Version >= 1)
                decoded.WDCB |= (feature[4] & 0x04) == 0x04;

            if (decoded.Version >= 2)
                decoded.DWP |= (feature[4] & 0x08) == 0x08;

            return decoded;
        }

        public static Feature_0010? Decode_0010(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 12)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0010)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0010 decoded = new Feature_0010();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.LogicalBlockSize = (uint)(feature[4] << 24 + feature[5] << 16 + feature[6] << 8 + feature[7]);
                decoded.Blocking = (ushort)(feature[8] << 8 + feature[9]);
                decoded.PP |= (feature[10] & 0x01) == 0x01;
            }

            return decoded;
        }

        public static Feature_001D? Decode_001D(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x001D)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_001D decoded = new Feature_001D();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_001E? Decode_001E(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x001E)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_001E decoded = new Feature_001E();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 1)
            {
                decoded.C2 |= (feature[4] & 0x02) == 0x02;
                decoded.CDText |= (feature[4] & 0x01) == 0x01;
            }

            if (decoded.Version >= 2)
                decoded.DAP |= (feature[4] & 0x80) == 0x80;

            return decoded;
        }

        public static Feature_001F? Decode_001F(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x001F)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_001F decoded = new Feature_001F();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 2 && feature.Length >= 8)
            {
                decoded.MULTI110 |= (feature[4] & 0x01) == 0x01;
                decoded.DualR |= (feature[6] & 0x01) == 0x01;
            }

            if (decoded.Version >= 2 && feature.Length >= 8)
                decoded.DualRW |= (feature[6] & 0x02) == 0x02;

            return decoded;
        }

        public static Feature_0020? Decode_0020(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 16)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0020)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0020 decoded = new Feature_0020();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 1)
            {
                decoded.LastLBA = (uint)(feature[4] << 24 + feature[5] << 16 + feature[6] << 8 + feature[7]);
                decoded.LogicalBlockSize = (uint)(feature[8] << 24 + feature[9] << 16 + feature[10] << 8 + feature[11]);
                decoded.Blocking = (ushort)(feature[12] << 8 + feature[13]);
                decoded.PP |= (feature[14] & 0x01) == 0x01;
            }

            return decoded;
        }

        public static Feature_0021? Decode_0021(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0021)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0021 decoded = new Feature_0021();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 1)
            {
                decoded.DataTypeSupported = (ushort)(feature[4] << 8 + feature[5]);
                decoded.BUF |= (feature[6] & 0x01) == 0x01;
                decoded.LinkSizes = new byte[feature[7]];
                if (feature.Length > (feature[7] + 8))
                    Array.Copy(feature, 8, decoded.LinkSizes, 0, feature[7]);
            }

            if (decoded.Version >= 3)
            {
                decoded.TRIO |= (feature[6] & 0x04) == 0x04;
                decoded.ARSV |= (feature[6] & 0x02) == 0x02;
            }

            return decoded;
        }

        public static Feature_0022? Decode_0022(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0022)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0022 decoded = new Feature_0022();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_0023? Decode_0023(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0023)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0023 decoded = new Feature_0023();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 1 && feature.Length >= 12)
            {
                decoded.RENoSA |= (feature[4] & 0x08) == 0x08;
                decoded.Expand |= (feature[4] & 0x04) == 0x04;
                decoded.QCert |= (feature[4] & 0x02) == 0x02;
                decoded.Cert |= (feature[4] & 0x01) == 0x01;
                decoded.RRM |= (feature[8] & 0x01) == 0x01;
            }

            if (decoded.Version >= 2 && feature.Length >= 12)
                decoded.FRF |= (feature[4] & 0x80) == 0x80;

            return decoded;
        }

        public static Feature_0024? Decode_0024(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0024)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0024 decoded = new Feature_0024();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 1 && feature.Length >= 8)
                decoded.SSA |= (feature[4] & 0x80) == 0x80;

            return decoded;
        }

        public static Feature_0025? Decode_0025(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 12)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0025)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0025 decoded = new Feature_0025();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.LogicalBlockSize = (uint)(feature[4] << 24 + feature[5] << 16 + feature[6] << 8 + feature[7]);
                decoded.Blocking = (ushort)(feature[8] << 8 + feature[9]);
                decoded.PP |= (feature[10] & 0x01) == 0x01;
            }

            return decoded;
        }

        public static Feature_0026? Decode_0026(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0026)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0026 decoded = new Feature_0026();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_0027? Decode_0027(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0027)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0027 decoded = new Feature_0027();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_0028? Decode_0028(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0028)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0028 decoded = new Feature_0028();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.Write |= (feature[4] & 0x01) == 0x01;

            if (decoded.Version >= 1)
            {
                decoded.DVDPWrite |= (feature[4] & 0x04) == 0x04;
                decoded.DVDPRead |= (feature[4] & 0x02) == 0x02;
            }

            return decoded;
        }

        public static Feature_0029? Decode_0029(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0029)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0029 decoded = new Feature_0029();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.DRTDM |= (feature[4] & 0x01) == 0x01;
                decoded.DBICacheZones = feature[5];
                decoded.Entries = (ushort)(feature[6] << 8 + feature[7]);
            }

            return decoded;
        }

        public static Feature_002A? Decode_002A(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x002A)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_002A decoded = new Feature_002A();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.Write |= (feature[4] & 0x01) == 0x01;
                decoded.CloseOnly |= (feature[5] & 0x01) == 0x01;
            }

            if (decoded.Version >= 1)
                decoded.QuickStart |= (feature[5] & 0x02) == 0x02;

            return decoded;
        }

        public static Feature_002B? Decode_002B(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x002B)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_002B decoded = new Feature_002B();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.Write |= (feature[4] & 0x01) == 0x01;

            return decoded;
        }

        public static Feature_002C? Decode_002C(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x002C)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_002C decoded = new Feature_002C();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.DSDG |= (feature[4] & 0x08) == 0x08;
                decoded.DSDR |= (feature[4] & 0x04) == 0x04;
                decoded.Intermediate |= (feature[4] & 0x02) == 0x02;
                decoded.Blank |= (feature[4] & 0x01) == 0x01;
            }

            return decoded;
        }

        public static Feature_002D? Decode_002D(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x002D)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_002D decoded = new Feature_002D();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.TestWrite |= (feature[4] & 0x04) == 0x04;
                decoded.CDRW |= (feature[4] & 0x02) == 0x02;
                decoded.RWSubchannel |= (feature[4] & 0x01) == 0x01;
                decoded.DataTypeSupported = (ushort)(feature[6] << 8 + feature[7]);
            }

            if (decoded.Version >= 2)
            {
                decoded.BUF |= (feature[4] & 0x40) == 0x40;
                decoded.RWRaw |= (feature[4] & 0x10) == 0x10;
                decoded.RWPack |= (feature[4] & 0x08) == 0x08;
            }

            return decoded;
        }

        public static Feature_002E? Decode_002E(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x002E)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_002E decoded = new Feature_002E();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.SAO |= (feature[4] & 0x20) == 0x20;
                decoded.RAWMS |= (feature[4] & 0x10) == 0x10;
                decoded.RAW |= (feature[4] & 0x08) == 0x08;
                decoded.TestWrite |= (feature[4] & 0x04) == 0x04;
                decoded.CDRW |= (feature[4] & 0x02) == 0x02;
                decoded.RW |= (feature[4] & 0x01) == 0x01;
                decoded.MaxCueSheet = (uint)(feature[5] << 16 + feature[6] << 8 + feature[7]);
            }

            if (decoded.Version >= 1)
                decoded.BUF |= (feature[4] & 0x40) == 0x40;

            return decoded;
        }

        public static Feature_002F? Decode_002F(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x002F)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_002F decoded = new Feature_002F();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.BUF |= (feature[4] & 0x40) == 0x40;
                decoded.TestWrite |= (feature[4] & 0x04) == 0x04;
            }

            if (decoded.Version >= 1)
                decoded.DVDRW |= (feature[4] & 0x02) == 0x02;

            if (decoded.Version >= 2)
                decoded.RDL |= (feature[4] & 0x08) == 0x08;

            return decoded;
        }

        public static Feature_0030? Decode_0030(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0030)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0030 decoded = new Feature_0030();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_0031? Decode_0031(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0031)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0031 decoded = new Feature_0031();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.TestWrite |= (feature[4] & 0x04) == 0x04;

            return decoded;
        }

        public static Feature_0032? Decode_0032(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0032)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0032 decoded = new Feature_0032();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.Intermediate |= (feature[4] & 0x02) == 0x02;
                decoded.Blank |= (feature[4] & 0x01) == 0x01;
            }

            return decoded;
        }

        public static Feature_0033? Decode_0033(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0033)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0033 decoded = new Feature_0033();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (feature[7] > 0 && feature.Length > (feature[7] + 8))
            {
                decoded.LinkSizes = new byte[feature[7]];
                Array.Copy(feature, 8, decoded.LinkSizes, 0, feature[7]);
            }

            return decoded;
        }

        public static Feature_0035? Decode_0035(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0035)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0035 decoded = new Feature_0035();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_0037? Decode_0037(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0037)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0037 decoded = new Feature_0037();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.SubtypeSupport = feature[5];

            return decoded;
        }

        public static Feature_0038? Decode_0038(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0038)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0038 decoded = new Feature_0038();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_003A? Decode_003A(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x003A)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_003A decoded = new Feature_003A();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.Write |= (feature[4] & 0x01) == 0x01;
                decoded.QuickStart |= (feature[5] & 0x02) == 0x02;
                decoded.CloseOnly |= (feature[5] & 0x01) == 0x01;
            }

            return decoded;
        }

        public static Feature_003B? Decode_003B(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x003B)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_003B decoded = new Feature_003B();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.Write |= (feature[4] & 0x01) == 0x01;

            return decoded;
        }

        public static Feature_0040? Decode_0040(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 32)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0040)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0040 decoded = new Feature_0040();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.OldRE |= (feature[9] & 0x01) == 0x01;
                decoded.OldR |= (feature[17] & 0x01) == 0x01;
                decoded.OldROM |= (feature[25] & 0x01) == 0x01;
            }

            if (decoded.Version >= 1)
            {
                decoded.BCA |= (feature[4] & 0x01) == 0x01;
                decoded.RE2 |= (feature[9] & 0x04) == 0x04;
                decoded.RE1 |= (feature[9] & 0x02) == 0x02;
                decoded.R |= (feature[17] & 0x02) == 0x02;
                decoded.ROM |= (feature[25] & 0x02) == 0x02;
            }

            return decoded;
        }

        public static Feature_0041? Decode_0041(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 24)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0041)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0041 decoded = new Feature_0041();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.SVNR |= (feature[4] & 0x01) == 0x01;
                decoded.OldRE |= (feature[9] & 0x01) == 0x01;
                decoded.OldR |= (feature[17] & 0x01) == 0x01;
            }

            if (decoded.Version >= 1)
            {
                decoded.RE2 |= (feature[9] & 0x04) == 0x04;
                decoded.RE1 |= (feature[9] & 0x02) == 0x02;
                decoded.R |= (feature[17] & 0x02) == 0x02;
            }

            return decoded;
        }

        public static Feature_0042? Decode_0042(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0042)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0042 decoded = new Feature_0042();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_0050? Decode_0050(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0050)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0050 decoded = new Feature_0050();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.HDDVDR |= (feature[4] & 0x01) == 0x01;
                decoded.HDDVDRAM |= (feature[6] & 0x01) == 0x01;
            }

            return decoded;
        }

        public static Feature_0051? Decode_0051(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0051)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0051 decoded = new Feature_0051();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.HDDVDR |= (feature[4] & 0x01) == 0x01;
                decoded.HDDVDRAM |= (feature[6] & 0x01) == 0x01;
            }

            return decoded;
        }

        public static Feature_0080? Decode_0080(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0080)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0080 decoded = new Feature_0080();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.RI |= (feature[4] & 0x01) == 0x01;

            return decoded;
        }

        public static Feature_0100? Decode_0100(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0100)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0100 decoded = new Feature_0100();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_0101? Decode_0101(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0101)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0101 decoded = new Feature_0101();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.PP |= (feature[4] & 0x01) == 0x01;

            return decoded;
        }

        public static Feature_0102? Decode_0102(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0102)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0102 decoded = new Feature_0102();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.SCC |= (feature[4] & 0x10) == 0x10;
                decoded.SDP |= (feature[4] & 0x04) == 0x04;
                decoded.HighestSlotNumber = (byte)(feature[7] & 0x1F);
            }

            return decoded;
        }

        public static Feature_0103? Decode_0103(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0103)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0103 decoded = new Feature_0103();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.Scan |= (feature[4] & 0x04) == 0x04;
                decoded.SCM |= (feature[4] & 0x02) == 0x02;
                decoded.SV |= (feature[4] & 0x01) == 0x01;
                decoded.VolumeLevels = (ushort)(feature[6] << 8 + feature[7]);
            }

            return decoded;
        }

        public static Feature_0104? Decode_0104(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0104)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0104 decoded = new Feature_0104();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 1 && feature.Length >= 8)
                decoded.M5 |= (feature[4] & 0x01) == 0x01;

            return decoded;
        }

        public static Feature_0105? Decode_0105(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0105)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0105 decoded = new Feature_0105();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 1 && feature.Length >= 8)
            {
                decoded.Group3 |= (feature[4] & 0x01) == 0x01;
                decoded.UnitLength = (ushort)(feature[6] << 8 + feature[7]);
            }

            return decoded;
        }

        public static Feature_0106? Decode_0106(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0106)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0106 decoded = new Feature_0106();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.CSSVersion = feature[7];

            return decoded;
        }

        public static Feature_0107? Decode_0107(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0107)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0107 decoded = new Feature_0107();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 3 && feature.Length >= 8)
            {
                decoded.RBCB |= (feature[4] & 0x10) == 0x10;
                decoded.SCS |= (feature[4] & 0x08) == 0x08;
                decoded.MP2A |= (feature[4] & 0x04) == 0x04;
                decoded.WSPD |= (feature[4] & 0x02) == 0x02;
                decoded.SW |= (feature[4] & 0x01) == 0x01;
            }

            if (decoded.Version >= 5 && feature.Length >= 8)
                decoded.RBCB |= (feature[4] & 0x20) == 0x20;

            return decoded;
        }

        public static Feature_0108? Decode_0108(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0108)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0108 decoded = new Feature_0108();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                byte[] serial = new byte[feature.Length];
                Array.Copy(feature, 4, serial, 0, feature.Length - 4);
                decoded.Serial = StringHandlers.CToString(serial).Trim();
            }

            return decoded;
        }

        public static Feature_0109? Decode_0109(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0109)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0109 decoded = new Feature_0109();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_010A? Decode_010A(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x010A)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_010A decoded = new Feature_010A();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.DCBs = new uint[feature[3] / 4];
                for (int i = 0; i < decoded.DCBs.Length; i++)
                    decoded.DCBs[i] = (uint)(feature[0 + 4 + i * 4] << 24 + feature[1 + 4 + i * 4] << 16 + feature[2 + 4 + i * 4] << 8 + feature[3 + 4 + i * 4]);
            }

            return decoded;
        }

        public static Feature_010B? Decode_010B(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x010B)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_010B decoded = new Feature_010B();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.CPRMVersion = feature[7];

            return decoded;
        }

        public static Feature_010C? Decode_010C(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 20)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x010C)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_010C decoded = new Feature_010C();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.Century = (ushort)(feature[4] << 8 + feature[5]);
                decoded.Year = (ushort)(feature[6] << 8 + feature[7]);
                decoded.Month = (ushort)(feature[8] << 8 + feature[9]);
                decoded.Day = (ushort)(feature[10] << 8 + feature[11]);
                decoded.Hour = (ushort)(feature[12] << 8 + feature[13]);
                decoded.Minute = (ushort)(feature[14] << 8 + feature[15]);
                decoded.Second = (ushort)(feature[16] << 8 + feature[17]);
            }

            return decoded;
        }

        public static Feature_010D? Decode_010D(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x010D)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_010D decoded = new Feature_010D();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.BNG |= (feature[4] & 0x01) == 0x01;
                decoded.BindNonceBlocks = feature[5];
                decoded.AGIDs = (byte)(feature[6] & 0x0F);
                decoded.AACSVersion = feature[7];
            }

            if (decoded.Version >= 2)
            {
                decoded.RDC |= (feature[4] & 0x10) == 0x10;
                decoded.RMC |= (feature[4] & 0x08) == 0x08;
                decoded.WBE |= (feature[4] & 0x04) == 0x04;
                decoded.BEC |= (feature[4] & 0x02) == 0x02;
            }

            return decoded;
        }

        public static Feature_010E? Decode_010E(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x010E)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_010E decoded = new Feature_010E();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
                decoded.MaxScrambleExtent = feature[4];

            return decoded;
        }

        public static Feature_0110? Decode_0110(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 8)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0110)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0110 decoded = new Feature_0110();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_0113? Decode_0113(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 4)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0113)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0113 decoded = new Feature_0113();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            return decoded;
        }

        public static Feature_0142? Decode_0142(byte[] feature)
        {
            if (feature == null)
                return null;

            if (feature.Length < 6)
                return null;

            ushort number = (ushort)((feature[0] << 8) + feature[1]);

            if (number != 0x0142)
                return null;

            if ((feature[3] + 4) != feature.Length)
                return null;

            Feature_0142 decoded = new Feature_0142();

            decoded.Current |= (feature[2] & 0x01) == 0x01;
            decoded.Persistent |= (feature[2] & 0x02) == 0x02;
            decoded.Version = (byte)((feature[2] & 0x3C) >> 2);

            if (decoded.Version >= 0)
            {
                decoded.PSAU |= (feature[4] & 0x80) == 0x80;
                decoded.LOSPB |= (feature[4] & 0x40) == 0x40;
                decoded.ME |= (feature[4] & 0x01) == 0x01;
                decoded.Profiles = new ushort[feature[5]];
                if (feature[5] * 2 + 6 == feature.Length)
                {
                    for (int i = 0; i < feature[5]; i++)
                        decoded.Profiles[i] = (ushort)(feature[0 + 6 + 2 * i] << 8 + feature[1 + 6 + 2 * i]);
                }
            }

            return decoded;
        }
    }
}

