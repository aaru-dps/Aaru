// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PFI.cs
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

namespace DiscImageChef.Decoders.DVD
{
    /// <summary>
    /// Information from the following standards:
    /// ANSI X3.304-1997
    /// T10/1048-D revision 9.0
    /// T10/1048-D revision 10a
    /// T10/1228-D revision 7.0c
    /// T10/1228-D revision 11a
    /// T10/1363-D revision 10g
    /// T10/1545-D revision 1d
    /// T10/1545-D revision 5
    /// T10/1545-D revision 5a
    /// T10/1675-D revision 2c
    /// T10/1675-D revision 4
    /// T10/1836-D revision 2g
    /// ECMA 267: 120 mm DVD - Read-Only Disk
    /// ECMA 268: 80 mm DVD - Read-Only Disk
    /// ECMA 272: 120 mm DVD Rewritable Disk (DVD-RAM)
    /// ECMA 274: Data Interchange on 120 mm Optical Disk using +RW Format - Capacity: 3,0 Gbytes and 6,0 Gbytes
    /// ECMA 279: 80 mm (1,23 Gbytes per side) and 120 mm (3,95 Gbytes per side) DVD-Recordable Disk (DVD-R)
    /// ECMA 330: 120 mm (4,7 Gbytes per side) and 80 mm (1,46 Gbytes per side) DVD Rewritable Disk (DVD-RAM)
    /// ECMA 337: Data Interchange on 120 mm and 80 mm Optical Disk using +RW Format - Capacity: 4,7 and 1,46 Gbytes per Side
    /// ECMA 338: 80 mm (1,46 Gbytes per side) and 120 mm (4,70 Gbytes per side) DVD Re-recordable Disk (DVD-RW)
    /// ECMA 349: Data Interchange on 120 mm and 80 mm Optical Disk using +R Format - Capacity: 4,7 and 1,46 Gbytes per Side
    /// ECMA 359: 80 mm (1,46 Gbytes per side) and 120 mm (4,70 Gbytes per side) DVD Recordable Disk (DVD-R)
    /// ECMA 364: Data Interchange on 120 mm and 80 mm Optical Disk using +R DL Format - Capacity 8,55 and 2,66 Gbytes per Side
    /// ECMA 365: Data Interchange on 60 mm Read-Only ODC - Capacity: 1,8 Gbytes (UMD™)
    /// ECMA 371: Data Interchange on 120 mm and 80 mm Optical Disk using +RW HS Format - Capacity 4,7 and 1,46 Gbytes per side
    /// ECMA 374: Data Interchange on 120 mm and 80 mm Optical Disk using +RW DL Format - Capacity 8,55 and 2,66 Gbytes per side
    /// ECMA 382: 120 mm (8,54 Gbytes per side) and 80 mm (2,66 Gbytes per side) DVD Recordable Disk for Dual Layer (DVD-R for DL)
    /// ECMA 384: 120 mm (8,54 Gbytes per side) and 80 mm (2,66 Gbytes per side) DVD Re-recordable Disk for Dual Layer (DVD-RW for DL)
    /// </summary>
    public static class PFI
    {
        public struct PhysicalFormatInformation
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;

            #region PFI common to all
            /// <summary>
            /// Byte 4, bits 7 to 4
            /// Disk category field
            /// </summary>
            public DVDCategory DiskCategory;
            /// <summary>
            /// Byte 4, bits 3 to 0
            /// Media version
            /// </summary>
            public byte PartVersion;
            /// <summary>
            /// Byte 5, bits 7 to 4
            /// 120mm if 0, 80mm if 1. If UMD (60mm) 0 also. Reserved rest of values
            /// </summary>
            public DVDSize DiscSize;
            /// <summary>
            /// Byte 5, bits 3 to 0
            /// Maximum data rate
            /// </summary>
            public DVDMaxTransfer MaximumRate;
            /// <summary>
            /// Byte 6, bit 7
            /// Reserved
            /// </summary>
            public bool Reserved3;
            /// <summary>
            /// Byte 6, bits 6 to 5
            /// Number of layers
            /// </summary>
            public byte Layers;
            /// <summary>
            /// Byte 6, bit 4
            /// Track path
            /// </summary>
            public bool TrackPath;
            /// <summary>
            /// Byte 6, bits 3 to 0
            /// Layer type
            /// </summary>
            public DVDLayerTypes LayerType;
            /// <summary>
            /// Byte 7, bits 7 to 4
            /// Linear density field
            /// </summary>
            public DVDLinearDensity LinearDensity;
            /// <summary>
            /// Byte 7, bits 3 to 0
            /// Track density field
            /// </summary>
            public DVDTrackDensity TrackDensity;
            /// <summary>
            /// Bytes 8 to 11
            /// PSN where Data Area starts
            /// </summary>
            public UInt32 DataAreaStartPSN;
            /// <summary>
            /// Bytes 12 to 15
            /// PSN where Data Area ends
            /// </summary>
            public UInt32 DataAreaEndPSN;
            /// <summary>
            /// Bytes 16 to 19
            /// PSN where Data Area ends in Layer 0
            /// </summary>
            public UInt32 Layer0EndPSN;
            /// <summary>
            /// Byte 20, bit 7
            /// True if BCA exists. GC/Wii discs do not have this bit set, but there is a BCA, making it unreadable in normal DVD drives
            /// </summary>
            public bool BCA;
            /// <summary>
            /// Byte 20, bits 6 to 0
            /// Reserved
            /// </summary>
            public byte Reserved4;
            #endregion PFI common to all

            #region UMD PFI
            /// <summary>
            /// Bytes 21 to 22
            /// UMD only, media attribute, application-defined, part of media specific in rest of discs
            /// </summary>
            public UInt16 MediaAttribute;
            #endregion UMD PFI

            #region DVD-RAM PFI
            /// <summary>
            /// Byte 36
            /// Disc type, respecting case recordability
            /// </summary>
            public DVDRAMDiscType DiscType;
            #region DVD-RAM PFI

            #region DVD-RAM PFI, Version 0001b
            /// <summary>
            /// Byte 52
            /// Byte 504 in Version 0110b
            /// Linear velocity, in tenths of m/s
            /// </summary>
            public byte Velocity;
            /// <summary>
            /// Byte 53
            /// Byte 505 in Version 0110b
            /// Read power on disk surface, tenths of mW
            /// </summary>
            public byte ReadPower;
            /// <summary>
            /// Byte 54
            /// Byte 507 in Version 0110b
            /// Peak power on disk surface for recording land tracks
            /// </summary>
            public byte PeakPower;
            /// <summary>
            /// Byte 55
            /// Bias power on disk surface for recording land tracks
            /// </summary>
            public byte BiasPower;
            /// <summary>
            /// Byte 56
            /// First pulse starting time for recording on land tracks, ns
            /// </summary>
            public byte FirstPulseStart;
            /// <summary>
            /// Byte 57
            /// Byte 515 in Version 0110b
            /// First pulse ending time for recording on land tracks
            /// </summary>
            public byte FirstPulseEnd;
            /// <summary>
            /// Byte 58
            /// Byte 518 in Version 0110b
            /// Multiple-pulse duration time for recording on land tracks
            /// </summary>
            public byte MultiplePulseDuration;
            /// <summary>
            /// Byte 59
            /// Byte 519 in Version 0110b
            /// Last pulse starting time for recording on land tracks
            /// </summary>
            public byte LastPulseStart;
            /// <summary>
            /// Byte 60
            /// Las pulse ending time for recording on land tracks
            /// </summary>
            public byte LastPulseEnd;
            /// <summary>
            /// Byte 61
            /// Bias power duration for recording on land tracks
            /// </summary>
            public byte BiasPowerDuration;
            /// <summary>
            /// Byte 62
            /// Byte 511 on Version 0110b
            /// Peak power for recording on groove tracks
            /// </summary>
            public byte PeakPowerGroove;
            /// <summary>
            /// Byte 63
            /// Bias power for recording on groove tracks
            /// </summary>
            public byte BiasPowerGroove;
            /// <summary>
            /// Byte 64
            /// First pulse starting time on groove tracks
            /// </summary>
            public byte FirstPulseStartGroove;
            /// <summary>
            /// Byte 65
            /// First pulse ending time on groove tracks
            /// </summary>
            public byte FirstPulseEndGroove;
            /// <summary>
            /// Byte 66
            /// Multiple-pulse duration time on groove tracks
            /// </summary>
            public byte MultiplePulseDurationGroove;
            /// <summary>
            /// Byte 67
            /// Last pulse starting time on groove tracks
            /// </summary>
            public byte LastPulseStartGroove;
            /// <summary>
            /// Byte 68
            /// Last pulse ending time on groove tracks
            /// </summary>
            public byte LastPulseEndGroove;
            /// <summary>
            /// Byte 69
            /// Bias power duration for recording on groove tracks
            /// </summary>
            public byte BiasPowerDurationGroove;
            #endregion DVD-RAM PFI, Version 0001b

            #region DVD-R PFI, DVD-RW PFI
            /// <summary>
            /// Bytes 36 to 39
            /// Sector number of the first sector of the current Border Out
            /// </summary>
            public UInt32 CurrentBorderOutSector;
            /// <summary>
            /// Bytes 40 to 43
            /// Sector number of the first sector of the next Border In
            /// </summary>
            public UInt32 NextBorderInSector;
            #endregion DVD-R PFI, DVD-RW PFI

            #region DVD+RW PFI
            /// <summary>
            /// Byte 36
            /// Linear velocities
            /// 0 = CLV from 4,90 m/s to 6,25 m/s
            /// 1 = CAV from 3,02 m/s to 7,35 m/s
            /// </summary>
            public UInt32 RecordingVelocity;
            /// <summary>
            /// Byte 37
            /// Maximum read power in milliwatts at maximum velocity
            /// mW = 20 * (value - 1)
            /// </summary>
            public UInt32 ReadPowerMaxVelocity;
            /// <summary>
            /// Byte 38
            /// Indicative value of Ptarget in mW at maximum velocity
            /// </summary>
            public UInt32 PIndMaxVelocity;
            /// <summary>
            /// Byte 39
            /// Peak power multiplication factor at maximum velocity
            /// </summary>
            public UInt32 PMaxVelocity;
            /// <summary>
            /// Byte 40
            /// Bias1/write power ration at maximum velocity
            /// </summary>
            public UInt32 E1MaxVelocity;
            /// <summary>
            /// Byte 41
            /// Bias2/write power ration at maximum velocity
            /// </summary>
            public UInt32 E2MaxVelocity;
            /// <summary>
            /// Byte 42
            /// Target value for γ, γtarget at the maximum velocity
            /// </summary>
            public UInt32 YTargetMaxVelocity;
            /// <summary>
            /// Byte 43
            /// Maximum read power in milliwatts at reference velocity (4,90 m/s)
            /// mW = 20 * (value - 1)
            /// </summary>
            public UInt32 ReadPowerRefVelocity;
            /// <summary>
            /// Byte 44
            /// Indicative value of Ptarget in mW at reference velocity (4,90 m/s)
            /// </summary>
            public UInt32 PIndRefVelocity;
            /// <summary>
            /// Byte 45
            /// Peak power multiplication factor at reference velocity (4,90 m/s)
            /// </summary>
            public UInt32 PRefVelocity;
            /// <summary>
            /// Byte 46
            /// Bias1/write power ration at reference velocity (4,90 m/s)
            /// </summary>
            public UInt32 E1RefVelocity;
            /// <summary>
            /// Byte 47
            /// Bias2/write power ration at reference velocity (4,90 m/s)
            /// </summary>
            public UInt32 E2RefVelocity;
            /// <summary>
            /// Byte 48
            /// Target value for γ, γtarget at the reference velocity (4,90 m/s)
            /// </summary>
            public UInt32 YTargetRefVelocity;
            /// <summary>
            /// Byte 49
            /// Maximum read power in milliwatts at minimum velocity
            /// mW = 20 * (value - 1)
            /// </summary>
            public UInt32 ReadPowerMinVelocity;
            /// <summary>
            /// Byte 50
            /// Indicative value of Ptarget in mW at minimum velocity
            /// </summary>
            public UInt32 PIndMinVelocity;
            /// <summary>
            /// Byte 51
            /// Peak power multiplication factor at minimum velocity
            /// </summary>
            public UInt32 PMinVelocity;
            /// <summary>
            /// Byte 52
            /// Bias1/write power ration at minimum velocity
            /// </summary>
            public UInt32 E1MinVelocity;
            /// <summary>
            /// Byte 53
            /// Bias2/write power ration at minimum velocity
            /// </summary>
            public UInt32 E2MinVelocity;
            /// <summary>
            /// Byte 54
            /// Target value for γ, γtarget at the minimum velocity
            /// </summary>
            public UInt32 YTargetMinVelocity;
            #endregion DVD+RW PFI

            #region DVD-RAM PFI, version 0110b
            /// <summary>
            /// Byte 506, bit 7
            /// Mode of adaptative write pulse control
            /// </summary>
            public bool AdaptativeWritePulseControlFlag;
            /// <summary>
            /// Byte 508
            /// Bias power 1 on disk surface for recording land tracks
            /// </summary>
            public byte BiasPower1;
            /// <summary>
            /// Byte 509
            /// Bias power 2 on disk surface for recording land tracks
            /// </summary>
            public byte BiasPower2;
            /// <summary>
            /// Byte 510
            /// Bias power 3 on disk surface for recording land tracks
            /// </summary>
            public byte BiasPower3;
            /// <summary>
            /// Byte 512
            /// Bias power 1 on disk surface for recording groove tracks
            /// </summary>
            public byte BiasPower1Groove;
            /// <summary>
            /// Byte 513
            /// Bias power 2 on disk surface for recording groove tracks
            /// </summary>
            public byte BiasPower2Groove;
            /// <summary>
            /// Byte 514
            /// Bias power 3 on disk surface for recording groove tracks
            /// </summary>
            public byte BiasPower3Groove;
            /// <summary>
            /// Byte 516
            /// First pulse duration
            /// </summary>
            public byte FirstPulseDuration;
            /// <summary>
            /// Byte 520
            /// Bias power 2 duration on land tracks at Velocity 1
            /// </summary>
            public byte BiasPower2Duration;
            /// <summary>
            /// Byte 521
            /// First pulse start time, at Mark 3T and Leading Space 3T
            /// </summary>
            public byte FirstPulseStart3TSpace3T;
            /// <summary>
            /// Byte 522
            /// First pulse start time, at Mark 4T and Leading Space 3T
            /// </summary>
            public byte FirstPulseStart4TSpace3T;
            /// <summary>
            /// Byte 523
            /// First pulse start time, at Mark 5T and Leading Space 3T
            /// </summary>
            public byte FirstPulseStart5TSpace3T;
            /// <summary>
            /// Byte 524
            /// First pulse start time, at Mark >5T and Leading Space 3T
            /// </summary>
            public byte FirstPulseStartSpace3T;
            /// <summary>
            /// Byte 525
            /// First pulse start time, at Mark 3T and Leading Space 4T
            /// </summary>
            public byte FirstPulseStart3TSpace4T;
            /// <summary>
            /// Byte 526
            /// First pulse start time, at Mark 4T and Leading Space 4T
            /// </summary>
            public byte FirstPulseStart4TSpace4T;
            /// <summary>
            /// Byte 527
            /// First pulse start time, at Mark 5T and Leading Space 4T
            /// </summary>
            public byte FirstPulseStart5TSpace4T;
            /// <summary>
            /// Byte 528
            /// First pulse start time, at Mark >5T and Leading Space 4T
            /// </summary>
            public byte FirstPulseStartSpace4T;
            /// <summary>
            /// Byte 529
            /// First pulse start time, at Mark 3T and Leading Space 5T
            /// </summary>
            public byte FirstPulseStart3TSpace5T;
            /// <summary>
            /// Byte 530
            /// First pulse start time, at Mark 4T and Leading Space 5T
            /// </summary>
            public byte FirstPulseStart4TSpace5T;
            /// <summary>
            /// Byte 531
            /// First pulse start time, at Mark 5T and Leading Space 5T
            /// </summary>
            public byte FirstPulseStart5TSpace5T;
            /// <summary>
            /// Byte 532
            /// First pulse start time, at Mark >5T and Leading Space 5T
            /// </summary>
            public byte FirstPulseStartSpace5T;
            /// <summary>
            /// Byte 533
            /// First pulse start time, at Mark 3T and Leading Space >5T
            /// </summary>
            public byte FirstPulseStart3TSpace;
            /// <summary>
            /// Byte 534
            /// First pulse start time, at Mark 4T and Leading Space >5T
            /// </summary>
            public byte FirstPulseStart4TSpace;
            /// <summary>
            /// Byte 535
            /// First pulse start time, at Mark 5T and Leading Space >5T
            /// </summary>
            public byte FirstPulseStart5TSpace;
            /// <summary>
            /// Byte 536
            /// First pulse start time, at Mark >5T and Leading Space >5T
            /// </summary>
            public byte FirstPulseStartSpace;
            /// <summary>
            /// Byte 537
            /// First pulse start time, at Mark 3T and Trailing Space 3T
            /// </summary>
            public byte FirstPulse3TStartTSpace3T;
            /// <summary>
            /// Byte 538
            /// First pulse start time, at Mark 4T and Trailing Space 3T
            /// </summary>
            public byte FirstPulse4TStartTSpace3T;
            /// <summary>
            /// Byte 539
            /// First pulse start time, at Mark 5T and Trailing Space 3T
            /// </summary>
            public byte FirstPulse5TStartTSpace3T;
            /// <summary>
            /// Byte 540
            /// First pulse start time, at Mark >5T and Trailing Space 3T
            /// </summary>
            public byte FirstPulseStartTSpace3T;
            /// <summary>
            /// Byte 541
            /// First pulse start time, at Mark 3T and Trailing Space 4T
            /// </summary>
            public byte FirstPulse3TStartTSpace4T;
            /// <summary>
            /// Byte 542
            /// First pulse start time, at Mark 4T and Trailing Space 4T
            /// </summary>
            public byte FirstPulse4TStartTSpace4T;
            /// <summary>
            /// Byte 543
            /// First pulse start time, at Mark 5T and Trailing Space 4T
            /// </summary>
            public byte FirstPulse5TStartTSpace4T;
            /// <summary>
            /// Byte 544
            /// First pulse start time, at Mark >5T and Trailing Space 4T
            /// </summary>
            public byte FirstPulseStartTSpace4T;
            /// <summary>
            /// Byte 545
            /// First pulse start time, at Mark 3T and Trailing Space 5T
            /// </summary>
            public byte FirstPulse3TStartTSpace5T;
            /// <summary>
            /// Byte 546
            /// First pulse start time, at Mark 4T and Trailing Space 5T
            /// </summary>
            public byte FirstPulse4TStartTSpace5T;
            /// <summary>
            /// Byte 547
            /// First pulse start time, at Mark 5T and Trailing Space 5T
            /// </summary>
            public byte FirstPulse5TStartTSpace5T;
            /// <summary>
            /// Byte 548
            /// First pulse start time, at Mark >5T and Trailing Space 5T
            /// </summary>
            public byte FirstPulseStartTSpace5T;
            /// <summary>
            /// Byte 549
            /// First pulse start time, at Mark 3T and Trailing Space >5T
            /// </summary>
            public byte FirstPulse3TStartTSpace;
            /// <summary>
            /// Byte 550
            /// First pulse start time, at Mark 4T and Trailing Space >5T
            /// </summary>
            public byte FirstPulse4TStartTSpace;
            /// <summary>
            /// Byte 551
            /// First pulse start time, at Mark 5T and Trailing Space >5T
            /// </summary>
            public byte FirstPulse5TStartTSpace;
            /// <summary>
            /// Byte 552
            /// First pulse start time, at Mark >5T and Trailing Space >5T
            /// </summary>
            public byte FirstPulseStartTSpace;
            /// <summary>
            /// Bytes 553 to 600
            /// Disk manufacturer's name, space-padded
            /// </summary>
            public string DiskManufacturer;
            /// <summary>
            /// Bytes 601 to 616
            /// Disk manufacturer's supplementary information
            /// </summary>
            public string DiskManufacturerSupplementary;
            /// <summary>
            /// Bytes 617 to 627
            /// Write power control parameters
            /// </summary>
            public byte[] WritePowerControlParams;
            /// <summary>
            /// Byte 619
            /// Ratio of peak power for land tracks to threshold peak power for land tracks
            /// </summary>
            public byte PowerRatioLandThreshold;
            /// <summary>
            /// Byte 620
            /// Target asymmetry
            /// </summary>
            public byte TargetAsymmetry;
            /// <summary>
            /// Byte 621
            /// Temporary peak power
            /// </summary>
            public byte TemporaryPeakPower;
            /// <summary>
            /// Byte 622
            /// Temporary bias power 1
            /// </summary>
            public byte TemporaryBiasPower1;
            /// <summary>
            /// Byte 623
            /// Temporary bias power 2
            /// </summary>
            public byte TemporaryBiasPower2;
            /// <summary>
            /// Byte 624
            /// Temporary bias power 3
            /// </summary>
            public byte TemporaryBiasPower3;
            /// <summary>
            /// Byte 625
            /// Ratio of peak power for groove tracks to threshold peak power for groove tracks
            /// </summary>
            public byte PowerRatioGrooveThreshold;
            /// <summary>
            /// Byte 626
            /// Ratio of peak power for land tracks to threshold 6T peak power for land tracks
            /// </summary>
            public byte PowerRatioLandThreshold6T;
            /// <summary>
            /// Byte 627
            /// Ratio of peak power for groove tracks to threshold 6T peak power for groove tracks
            /// </summary>
            public byte PowerRatioGrooveThreshold6T;
            #endregion DVD-RAM PFI, version 0110b

            #region DVD+RW PFI and DVD+R PFI
            /// <summary>
            /// Byte 20, bit 6
            /// If set indicates data zone contains extended information for VCPS
            /// </summary>
            public bool VCPS;
            /// <summary>
            /// Byte 21
            /// Indicates restricted usage disk
            /// </summary>
            public byte ApplicationCode;
            /// <summary>
            /// Byte 22
            /// Bitmap of extended information block presence
            /// </summary>
            public byte ExtendedInformation;
            /// <summary>
            /// Bytes 23 to 30
            /// Disk manufacturer ID, null-padded
            /// </summary>
            public string DiskManufacturerID;
            /// <summary>
            /// Bytes 31 to 33
            /// Media type ID, null-padded
            /// </summary>
            public string MediaTypeID;
            /// <summary>
            /// Byte 34
            /// Product revision number
            /// </summary>
            public byte ProductRevision;
            /// <summary>
            /// Byte 35
            /// Indicates how many bytes, up to 63, are used in ADIP's PFI
            /// </summary>
            public byte PFIUsedInADIP;
            #region DVD+RW PFI and DVD+R PFI

            #region DVD+RW PFI, version 0010b
            /// <summary>
            /// Byte 55
            /// Ttop first pulse duration
            /// </summary>
            public byte FirstPulseDuration;
            /// <summary>
            /// Byte 56
            /// Tmp multi pulse duration
            /// </summary>
            public byte MultiPulseDuration;
            /// <summary>
            /// Byte 57
            /// dTtop first pulse lead time
            /// </summary>
            public byte FirstPulseLeadTime;
            /// <summary>
            /// Byte 58
            /// dTera erase lead time at reference velocity
            /// </summary>
            public byte EraseLeadTimeRefVelocity;
            /// <summary>
            /// Byte 59
            /// dTera erase lead time at upper velocity
            /// </summary>
            public byte EraseLeadTimeUppVelocity;
            #endregion DVD+RW PFI, version 0010b

            #region DVD+R PFI version 0001b and DVD+R DL PFI version 0001b
            /// <summary>
            /// Byte 36
            /// Primary recording velocity for the basic write strategy
            /// </summary>
            public byte PrimaryVelocity;
            /// <summary>
            /// Byte 37
            /// Upper recording velocity for the basic write strategy
            /// </summary>
            public byte UpperVelocity;
            /// <summary>
            /// Byte 38
            /// Wavelength λIND
            /// </summary>
            public byte Wavelength;
            /// <summary>
            /// Byte 39
            /// Normalized write power dependency on wavelength (dP/dλ)/(PIND/λIND)
            /// </summary>
            public byte NormalizedPowerDependency;
            /// <summary>
            /// Byte 40
            /// Maximum read power at primary velocity
            /// </summary>
            public byte MaximumPowerAtPrimaryVelocity;
            /// <summary>
            /// Byte 41
            /// Pind at primary velocity
            /// </summary>
            public byte PindAtPrimaryVelocity;
            /// <summary>
            /// Byte 42
            /// βtarget at primary velocity
            /// </summary>
            public byte BtargetAtPrimaryVelocity;
            /// <summary>
            /// Byte 43
            /// Maximum read power at upper velocity
            /// </summary>
            public byte MaximumPowerAtUpperVelocity;
            /// <summary>
            /// Byte 44
            /// Pind at primary velocity
            /// </summary>
            public byte PindAtUpperVelocity;
            /// <summary>
            /// Byte 45
            /// βtarget at upper velocity
            /// </summary>
            public byte BtargetAtUpperVelocity;
            /// <summary>
            /// Byte 46
            /// Ttop (≥4T) first pulse duration for cm∗ ≥4T at Primary velocity
            /// </summary>
            public byte FirstPulseDuration4TPrimaryVelocity;
            /// <summary>
            /// Byte 47
            /// Ttop (=3T) first pulse duration for cm∗ =3T at Primary velocity
            /// </summary>
            public byte FirstPulseDuration3TPrimaryVelocity;
            /// <summary>
            /// Byte 48
            /// Tmp multi pulse duration at Primary velocity
            /// </summary>
            public byte MultiPulseDurationPrimaryVelocity;
            /// <summary>
            /// Byte 49
            /// Tlp last pulse duration at Primary velocity
            /// </summary>
            public byte LastPulseDurationPrimaryVelocity;
            /// <summary>
            /// Byte 50
            /// dTtop (≥4T) first pulse lead time for cm∗ ≥4T at Primary velocity
            /// </summary>
            public byte FirstPulseLeadTime4TPrimaryVelocity;
            /// <summary>
            /// Byte 51
            /// dTtop (=3T) first pulse lead time for cm∗ =3T at Primary velocity
            /// </summary>
            public byte FirstPulseLeadTime3TPrimaryVelocity;
            /// <summary>
            /// Byte 52
            /// dTle first pulse leading edge shift for ps∗ =3T at Primary velocity
            /// </summary>
            public byte FirstPulseLeadingEdgePrimaryVelocity;
            /// <summary>
            /// Byte 53
            /// Ttop (≥4T) first pulse duration for cm∗ ≥4T at Upper velocity
            /// </summary>
            public byte FirstPulseDuration4TUpperVelocity;
            /// <summary>
            /// Byte 54
            /// Ttop (=3T) first pulse duration for cm∗ =3T at Upper velocity
            /// </summary>
            public byte FirstPulseDuration3TUpperVelocity;
            /// <summary>
            /// Byte 55
            /// Tmp multi pulse duration at Upper velocity
            /// </summary>
            public byte MultiPulseDurationUpperVelocity;
            /// <summary>
            /// Byte 56
            /// Tlp last pulse duration at Upper velocity
            /// </summary>
            public byte LastPulseDurationUpperVelocity;
            /// <summary>
            /// Byte 57
            /// dTtop (≥4T) first pulse lead time for cm∗ ≥4T at Upper velocity
            /// </summary>
            public byte FirstPulseLeadTime4TUpperVelocity;
            /// <summary>
            /// Byte 58
            /// dTtop (=3T) first pulse lead time for cm∗ =3T at Upper velocity
            /// </summary>
            public byte FirstPulseLeadTime3TUpperVelocity;
            /// <summary>
            /// Byte 59
            /// dTle first pulse leading edge shift for ps∗ =3T at Upper velocity
            /// </summary>
            public byte FirstPulseLeadingEdgeUpperVelocity;
            #endregion DVD+R PFI version 0001b and DVD+R DL PFI version 0001b

            #region DVD+R DL PFI version 0001b
            /// <summary>
            /// Byte 34, bits 7 to 6
            /// </summary>
            public DVDLayerStructure LayerStructure;
            #endregion DVD+R DL PFI version 0001b

            #region DVD+RW DL PFI
            /// <summary>
            /// Byte 36
            /// Primary recording velocity for the basic write strategy
            /// </summary>
            public byte PrimaryVelocity;
            /// <summary>
            /// Byte 37
            /// Maximum read power at Primary velocity
            /// </summary>
            public byte MaxReadPowerPrimaryVelocity;
            /// <summary>
            /// Byte 38
            /// PIND at Primary velocity
            /// </summary>
            public byte PindPrimaryVelocity;
            /// <summary>
            /// Byte 39
            /// ρ at Primary velocity
            /// </summary>
            public byte PPrimaryVelocity;
            /// <summary>
            /// Byte 40
            /// ε1 at Primary velocity
            /// </summary>
            public byte E1PrimaryVelocity;
            /// <summary>
            /// Byte 41
            /// ε2 at Primary velocity
            /// </summary>
            public byte E2PrimaryVelocity;
            /// <summary>
            /// Byte 42
            /// γtarget at Primary velocity
            /// </summary>
            public byte YtargetPrimaryVelocity;
            /// <summary>
            /// Byte 43
            /// β optimum at Primary velocity
            /// </summary>
            public byte BOptimumPrimaryVelocity;
            /// <summary>
            /// Byte 46
            /// Ttop first pulse duration
            /// </summary>
            public byte TFirstPulseDuration;
            /// <summary>
            /// Byte 47
            /// Tmp multi pulse duration
            /// </summary>
            public byte TMultiPulseDuration;
            /// <summary>
            /// Byte 48
            /// dTtop first pulse lead/lag time for any runlength ≥ 4T
            /// </summary>
            public byte FirstPulseLeadTimeAnyRun;
            /// <summary>
            /// Byte 49
            /// dTtop,3 first pulse lead/lag time for runlengths = 3T
            /// </summary>
            public byte FirstPulseLeadTimeRun3T;
            /// <summary>
            /// Byte 50
            /// dTlp last pulse lead/lag time for any runlength ≥ 5T
            /// </summary>
            public byte LastPulseLeadTimeAnyRun;
            /// <summary>
            /// Byte 51
            /// dTlp,3 last pulse lead/lag time for runlengths = 3T
            /// </summary>
            public byte LastPulseLeadTime3T;
            /// <summary>
            /// Byte 52
            /// dTlp,4 last pulse lead/lag time for runlengths = 4T
            /// </summary>
            public byte LastPulseLeadTime4T;
            /// <summary>
            /// Byte 53
            /// dTera erase lead/lag time when preceding mark length ≥ 5T
            /// </summary>
            public byte ErasePulseLeadTimeAny;
            /// <summary>
            /// Byte 54
            /// dTera,3 erase lead/lag time when preceding mark length = 3T
            /// </summary>
            public byte ErasePulseLeadTime3T;
            /// <summary>
            /// Byte 55
            /// dTera,4 erase lead/lag time when preceding mark length = 4T
            /// </summary>
            public byte ErasePulseLeadTime4T;
            #endregion DVD+RW DL PFI

            #region DVD-R DL PFI and DVD-RW DL PFI
            /// <summary>
            /// Byte 21
            /// Maximum recording speed
            /// </summary>
            public DVDRecordingSpeed MaxRecordingSpeed;
            /// <summary>
            /// Byte 22
            /// Minimum recording speed
            /// </summary>
            public DVDRecordingSpeed MinRecordingSpeed;
            /// <summary>
            /// Byte 23
            /// Another recording speed
            /// </summary>
            public DVDRecordingSpeed RecordingSpeed1;
            /// <summary>
            /// Byte 24
            /// Another recording speed
            /// </summary>
            public DVDRecordingSpeed RecordingSpeed2;
            /// <summary>
            /// Byte 25
            /// Another recording speed
            /// </summary>
            public DVDRecordingSpeed RecordingSpeed3;
            /// <summary>
            /// Byte 26
            /// Another recording speed
            /// </summary>
            public DVDRecordingSpeed RecordingSpeed4;
            /// <summary>
            /// Byte 27
            /// Another recording speed
            /// </summary>
            public DVDRecordingSpeed RecordingSpeed5;
            /// <summary>
            /// Byte 28
            /// Another recording speed
            /// </summary>
            public DVDRecordingSpeed RecordingSpeed6;
            /// <summary>
            /// Byte 29
            /// Another recording speed
            /// </summary>
            public DVDRecordingSpeed RecordingSpeed7;
            /// <summary>
            /// Byte 30
            /// Class
            /// </summary>
            public byte Class;
            /// <summary>
            /// Byte 31
            /// Extended version. 0x30 = ECMA-382, 0x20 = ECMA-384
            /// </summary>
            public byte ExtendedVersion;
            /// <summary>
            /// Byte 36
            /// Start sector number of current RMD in Extra Border Zone
            /// </summary>
            public UInt32 CurrentRMDExtraBorderPSN;
            /// <summary>
            /// Byte 40
            /// Start sector number of Physical Format Information blocks in Extra Border Zone
            /// </summary>
            public UInt32 PFIExtraBorderPSN;
            /// <summary>
            /// Byte 44, bit 0
            /// If NOT set, Control Data Zone is pre-recorded
            /// </summary>
            public bool PreRecordedControlDataInv;
            /// <summary>
            /// Byte 44 bit 1
            /// Lead-in Zone is pre-recorded
            /// </summary>
            public bool PreRecordedLeadIn;
            /// <summary>
            /// Byte 44 bit 3
            /// Lead-out Zone is pre-recorded
            /// </summary>
            public bool PreRecordedLeadOut;
            /// <summary>
            /// Byte 45 bits 0 to 3
            /// AR characteristic of LPP on Layer 1
            /// </summary>
            public byte ARCharLayer1;
            /// <summary>
            /// Byte 45 bits 4 to 7
            /// Tracking polarity on Layer 1
            /// </summary>
            public byte TrackPolarityLayer1;
            #endregion DVD-R DL PFI and DVD-RW DL PFI
        }
    }

    public enum DVDCategory
    {
        /// <summary>
        /// DVD-ROM. Version 1 is ECMA-267 and ECMA-268.
        /// </summary>
        DVDROM = 0,
        /// <summary>
        /// DVD-RAM. Version 1 is ECMA-272. Version 6 is ECMA-330.
        /// </summary>
        DVDRAM = 1,
        /// <summary>
        /// DVD-R. Version 1 is ECMA-279. Version 5 is ECMA-359. Version 6 is ECMA-832.
        /// </summary>
        DVDR = 2,
        /// <summary>
        /// DVD-RW. Version 2 is ECMA-338. Version 3 is ECMA-384.
        /// </summary>
        DVDRW = 3,
        /// <summary>
        /// UMD. Version 0 is ECMA-365.
        /// </summary>
        UMD = 8,
        /// <summary>
        /// DVD+RW. Version 1 is ECMA-274. Version 2 is ECMA-337. Version 3 is ECMA-371.
        /// </summary>
        DVDPRW = 9,
        /// <summary>
        /// DVD+R. Version 1 is ECMA-349.
        /// </summary>
        DVDPR = 10,
        /// <summary>
        /// DVD+RW DL. Version 1 is ECMA-374.
        /// </summary>
        DVDPRWDL = 13,
        /// <summary>
        /// DVD+R DL. Version 1 is ECMA-364.
        /// </summary>
        DVDPRDL = 14
    }

    public enum DVDSize
    {
        /// <summary>
        /// 120 mm
        /// </summary>
        OneTwenty = 0,
        /// <summary>
        /// 80 mm
        /// </summary>
        Eighty = 1
    }

    public enum DVDMaxTransfer
    {
        /// <summary>
        /// 2,52 Mbit/s
        /// </summary>
        Two = 0,
        /// <summary>
        /// 5,04 Mbit/s
        /// </summary>
        Five = 1,
        /// <summary>
        /// 10,08 Mbit/s
        /// </summary>
        Ten = 2,
        /// <summary>
        /// No maximum transfer rate
        /// </summary>
        Unspecified = 15
    }

    public enum DVDLayerTypes
    {
        ReadOnly = 1,
        Recordable = 2,
        Rewritable = 4
    }

    public enum DVDLinearDensity
    {
        /// <summary>
        /// 0,133 μm
        /// </summary>
        One33 = 0,
        /// <summary>
        /// 0,147 μm
        /// </summary>
        One47 = 1,
        /// <summary>
        /// 0,205 μm to 0,218 μm
        /// </summary>
        Twenty5 = 2,
        /// <summary>
        /// 0,140 μm to 0,148 μm
        /// </summary>
        One40 = 4,
        /// <summary>
        /// 0,176 μm
        /// </summary>
        One76 = 8
    }

    public enum DVDTrackDensity
    {
        /// <summary>
        /// 0,74 μm
        /// </summary>
        Seven4 = 0,
        /// <summary>
        /// 0,80 μm
        /// </summary>
        Eighty = 1,
        /// <summary>
        /// 0,615 μm
        /// </summary>
        Six15 = 2,
    }

    public enum DVDRAMDiscType
    {
        /// <summary>
        /// Shall not be recorded without a case
        /// </summary>
        Cased = 0,
        /// <summary>
        /// May be recorded without a case or within one
        /// </summary>
        Uncased = 1
    }

    public enum DVDLayerStructure
    {
        Unspecified = 0,
        InvertedStack = 1,
        TwoP = 2,
        Reserved = 3
    }

    public enum DVDRecordingSpeed
    {
        None = 0
        Two = 0,
        Four = 0x10,
        Six = 0x20,
        Eight = 0x30,
        Ten = 0x40,
        Twelve = 0x50
    }
}

