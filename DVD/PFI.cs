// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PFI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Records DVD Physical Format Information.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes;
using Aaru.Helpers;

namespace Aaru.Decoders.DVD;

// Information from the following standards:
// ANSI X3.304-1997
// T10/1048-D revision 9.0
// T10/1048-D revision 10a
// T10/1228-D revision 7.0c
// T10/1228-D revision 11a
// T10/1363-D revision 10g
// T10/1545-D revision 1d
// T10/1545-D revision 5
// T10/1545-D revision 5a
// T10/1675-D revision 2c
// T10/1675-D revision 4
// T10/1836-D revision 2g
// ECMA 267: 120 mm DVD - Read-Only Disk
// ECMA 268: 80 mm DVD - Read-Only Disk
// ECMA 272: 120 mm DVD Rewritable Disk (DVD-RAM)
// ECMA 274: Data Interchange on 120 mm Optical Disk using +RW Format - Capacity: 3,0 Gbytes and 6,0 Gbytes
// ECMA 279: 80 mm (1,23 Gbytes per side) and 120 mm (3,95 Gbytes per side) DVD-Recordable Disk (DVD-R)
// ECMA 330: 120 mm (4,7 Gbytes per side) and 80 mm (1,46 Gbytes per side) DVD Rewritable Disk (DVD-RAM)
// ECMA 337: Data Interchange on 120 mm and 80 mm Optical Disk using +RW Format - Capacity: 4,7 and 1,46 Gbytes per Side
// ECMA 338: 80 mm (1,46 Gbytes per side) and 120 mm (4,70 Gbytes per side) DVD Re-recordable Disk (DVD-RW)
// ECMA 349: Data Interchange on 120 mm and 80 mm Optical Disk using +R Format - Capacity: 4,7 and 1,46 Gbytes per Side
// ECMA 359: 80 mm (1,46 Gbytes per side) and 120 mm (4,70 Gbytes per side) DVD Recordable Disk (DVD-R)
// ECMA 364: Data Interchange on 120 mm and 80 mm Optical Disk using +R DL Format - Capacity 8,55 and 2,66 Gbytes per Side
// ECMA 365: Data Interchange on 60 mm Read-Only ODC - Capacity: 1,8 Gbytes (UMD™)
// ECMA 371: Data Interchange on 120 mm and 80 mm Optical Disk using +RW HS Format - Capacity 4,7 and 1,46 Gbytes per side
// ECMA 374: Data Interchange on 120 mm and 80 mm Optical Disk using +RW DL Format - Capacity 8,55 and 2,66 Gbytes per side
// ECMA 382: 120 mm (8,54 Gbytes per side) and 80 mm (2,66 Gbytes per side) DVD Recordable Disk for Dual Layer (DVD-R for DL)
// ECMA 384: 120 mm (8,54 Gbytes per side) and 80 mm (2,66 Gbytes per side) DVD Re-recordable Disk for Dual Layer (DVD-RW for DL)
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static class PFI
{
    public static PhysicalFormatInformation? Decode(byte[] response, MediaType mediaType)
    {
        if(response == null)
            return null;

        if(response.Length == 2048)
        {
            byte[] tmp2 = new byte[2052];
            Array.Copy(response, 0, tmp2, 4, 2048);
            response = tmp2;
        }

        if(response.Length < 2052)
            return null;

        var    pfi = new PhysicalFormatInformation();
        byte[] tmp;

        pfi.DataLength = (ushort)((response[0] << 8) + response[1]);
        pfi.Reserved1  = response[2];
        pfi.Reserved2  = response[3];

        // Common
        pfi.DiskCategory  =  (DiskCategory)((response[4] & 0xF0) >> 4);
        pfi.PartVersion   =  (byte)(response[4] & 0x0F);
        pfi.DiscSize      =  (DVDSize)((response[5] & 0xF0) >> 4);
        pfi.MaximumRate   =  (MaximumRateField)(response[5] & 0x0F);
        pfi.Reserved3     |= (response[6]                   & 0x80) == 0x80;
        pfi.Layers        =  (byte)((response[6] & 0x60) >> 5);
        pfi.TrackPath     |= (response[6]                     & 0x08) == 0x08;
        pfi.LayerType     =  (LayerTypeFieldMask)(response[6] & 0x07);
        pfi.LinearDensity =  (LinearDensityField)((response[7] & 0xF0) >> 4);
        pfi.TrackDensity  =  (TrackDensityField)(response[7] & 0x0F);

        pfi.DataAreaStartPSN = (uint)((response[8] << 24) + (response[9] << 16) + (response[10] << 8) + response[11]);

        pfi.DataAreaEndPSN = (uint)((response[12] << 24) + (response[13] << 16) + (response[14] << 8) + response[15]);

        pfi.Layer0EndPSN = (uint)((response[16] << 24) + (response[17] << 16) + (response[18] << 8) + response[19]);

        pfi.BCA |= (response[20] & 0x80) == 0x80;

        pfi.RecordedBookType = pfi.DiskCategory;

        if(mediaType != MediaType.DVDROM)
            switch(mediaType)
            {
                case MediaType.DVDPR:
                    pfi.DiskCategory = DiskCategory.DVDPR;

                    break;
                case MediaType.DVDPRDL:
                    pfi.DiskCategory = DiskCategory.DVDPRDL;

                    break;
                case MediaType.DVDPRW:
                    pfi.DiskCategory = DiskCategory.DVDPRW;

                    break;
                case MediaType.DVDPRWDL:
                    pfi.DiskCategory = DiskCategory.DVDPRWDL;

                    break;
                case MediaType.DVDRDL:
                    pfi.DiskCategory = DiskCategory.DVDR;

                    if(pfi.PartVersion < 6)
                        pfi.PartVersion = 6;

                    break;
                case MediaType.DVDR:
                    pfi.DiskCategory = DiskCategory.DVDR;

                    if(pfi.PartVersion > 5)
                        pfi.PartVersion = 5;

                    break;
                case MediaType.DVDRAM:
                    pfi.DiskCategory = DiskCategory.DVDRAM;

                    break;
                case MediaType.DVDRWDL:
                    pfi.DiskCategory = DiskCategory.DVDRW;

                    if(pfi.PartVersion < 15)
                        pfi.PartVersion = 15;

                    break;
                case MediaType.DVDRW:
                    pfi.DiskCategory = DiskCategory.DVDRW;

                    if(pfi.PartVersion > 14)
                        pfi.PartVersion = 14;

                    break;

                case MediaType.HDDVDR:
                    pfi.DiskCategory = DiskCategory.HDDVDR;

                    break;
                case MediaType.HDDVDRAM:
                    pfi.DiskCategory = DiskCategory.HDDVDRAM;

                    break;
                case MediaType.HDDVDROM:
                    pfi.DiskCategory = DiskCategory.HDDVDROM;

                    break;
                case MediaType.HDDVDRW:
                    pfi.DiskCategory = DiskCategory.HDDVDRW;

                    break;
                case MediaType.GOD:
                    pfi.DiscSize     = DVDSize.Eighty;
                    pfi.DiskCategory = DiskCategory.Nintendo;

                    break;
                case MediaType.WOD:
                    pfi.DiscSize     = DVDSize.OneTwenty;
                    pfi.DiskCategory = DiskCategory.Nintendo;

                    break;
                case MediaType.UMD:
                    pfi.DiskCategory = DiskCategory.UMD;

                    break;
            }

        switch(pfi.DiskCategory)
        {
            // UMD
            case DiskCategory.UMD:
                pfi.MediaAttribute = (ushort)((response[21] << 8) + response[22]);

                break;

            // DVD-RAM
            case DiskCategory.DVDRAM:
                pfi.DiscType = (DVDRAMDiscType)response[36];

                switch(pfi.PartVersion)
                {
                    case 1:
                        pfi.Velocity                    = response[52];
                        pfi.ReadPower                   = response[53];
                        pfi.PeakPower                   = response[54];
                        pfi.BiasPower                   = response[55];
                        pfi.FirstPulseStart             = response[56];
                        pfi.FirstPulseEnd               = response[57];
                        pfi.MultiPulseDuration          = response[58];
                        pfi.LastPulseStart              = response[59];
                        pfi.LastPulseEnd                = response[60];
                        pfi.BiasPowerDuration           = response[61];
                        pfi.PeakPowerGroove             = response[62];
                        pfi.BiasPowerGroove             = response[63];
                        pfi.FirstPulseStartGroove       = response[64];
                        pfi.FirstPulseEndGroove         = response[65];
                        pfi.MultiplePulseDurationGroove = response[66];
                        pfi.LastPulseStartGroove        = response[67];
                        pfi.LastPulseEndGroove          = response[68];
                        pfi.BiasPowerDurationGroove     = response[69];

                        break;
                    case >= 6:
                        pfi.Velocity                        =  response[504];
                        pfi.ReadPower                       =  response[505];
                        pfi.AdaptativeWritePulseControlFlag |= (response[506] & 0x80) == 0x80;
                        pfi.PeakPower                       =  response[507];
                        pfi.BiasPower1                      =  response[508];
                        pfi.BiasPower2                      =  response[509];
                        pfi.BiasPower3                      =  response[510];
                        pfi.PeakPowerGroove                 =  response[511];
                        pfi.BiasPower1Groove                =  response[512];
                        pfi.BiasPower2Groove                =  response[513];
                        pfi.BiasPower3Groove                =  response[514];
                        pfi.FirstPulseEnd                   =  response[515];
                        pfi.FirstPulseDuration              =  response[516];
                        pfi.MultiPulseDuration              =  response[518];
                        pfi.LastPulseStart                  =  response[519];
                        pfi.BiasPower2Duration              =  response[520];
                        pfi.FirstPulseStart3TSpace3T        =  response[521];
                        pfi.FirstPulseStart4TSpace3T        =  response[522];
                        pfi.FirstPulseStart5TSpace3T        =  response[523];
                        pfi.FirstPulseStartSpace3T          =  response[524];
                        pfi.FirstPulseStart3TSpace4T        =  response[525];
                        pfi.FirstPulseStart4TSpace4T        =  response[526];
                        pfi.FirstPulseStart5TSpace4T        =  response[527];
                        pfi.FirstPulseStartSpace4T          =  response[528];
                        pfi.FirstPulseStart3TSpace5T        =  response[529];
                        pfi.FirstPulseStart4TSpace5T        =  response[530];
                        pfi.FirstPulseStart5TSpace5T        =  response[531];
                        pfi.FirstPulseStartSpace5T          =  response[532];
                        pfi.FirstPulseStart3TSpace          =  response[533];
                        pfi.FirstPulseStart4TSpace          =  response[534];
                        pfi.FirstPulseStart5TSpace          =  response[535];
                        pfi.FirstPulseStartSpace            =  response[536];
                        pfi.FirstPulse3TStartTSpace3T       =  response[537];
                        pfi.FirstPulse4TStartTSpace3T       =  response[538];
                        pfi.FirstPulse5TStartTSpace3T       =  response[539];
                        pfi.FirstPulseStartTSpace3T         =  response[540];
                        pfi.FirstPulse3TStartTSpace4T       =  response[541];
                        pfi.FirstPulse4TStartTSpace4T       =  response[542];
                        pfi.FirstPulse5TStartTSpace4T       =  response[543];
                        pfi.FirstPulseStartTSpace4T         =  response[544];
                        pfi.FirstPulse3TStartTSpace5T       =  response[545];
                        pfi.FirstPulse4TStartTSpace5T       =  response[546];
                        pfi.FirstPulse5TStartTSpace5T       =  response[547];
                        pfi.FirstPulseStartTSpace5T         =  response[548];
                        pfi.FirstPulse3TStartTSpace         =  response[549];
                        pfi.FirstPulse4TStartTSpace         =  response[550];
                        pfi.FirstPulse5TStartTSpace         =  response[551];
                        pfi.FirstPulseStartTSpace           =  response[552];
                        tmp                                 =  new byte[48];
                        Array.Copy(response, 553, tmp, 0, 48);
                        pfi.DiskManufacturer = StringHandlers.SpacePaddedToString(tmp);
                        tmp                  = new byte[16];
                        Array.Copy(response, 601, tmp, 0, 16);
                        pfi.DiskManufacturerSupplementary = StringHandlers.SpacePaddedToString(tmp);
                        pfi.WritePowerControlParams       = new byte[2];
                        pfi.WritePowerControlParams[0]    = response[617];
                        pfi.WritePowerControlParams[1]    = response[618];
                        pfi.PowerRatioLandThreshold       = response[619];
                        pfi.TargetAsymmetry               = response[620];
                        pfi.TemporaryPeakPower            = response[621];
                        pfi.TemporaryBiasPower1           = response[622];
                        pfi.TemporaryBiasPower2           = response[623];
                        pfi.TemporaryBiasPower3           = response[624];
                        pfi.PowerRatioGrooveThreshold     = response[625];
                        pfi.PowerRatioLandThreshold6T     = response[626];
                        pfi.PowerRatioGrooveThreshold6T   = response[627];

                        break;
                }

                break;

            // DVD-R and DVD-RW
            case DiskCategory.DVDR when pfi.PartVersion  < 6:
            case DiskCategory.DVDRW when pfi.PartVersion < 15:
                pfi.CurrentBorderOutSector =
                    (uint)((response[36] << 24) + (response[37] << 16) + (response[38] << 8) + response[39]);

                pfi.NextBorderInSector =
                    (uint)((response[40] << 24) + (response[41] << 16) + (response[42] << 8) + response[43]);

                break;

            // DVD+RW
            case DiskCategory.DVDPRW:
                pfi.RecordingVelocity    = response[36];
                pfi.ReadPowerMaxVelocity = response[37];
                pfi.PIndMaxVelocity      = response[38];
                pfi.PMaxVelocity         = response[39];
                pfi.E1MaxVelocity        = response[40];
                pfi.E2MaxVelocity        = response[41];
                pfi.YTargetMaxVelocity   = response[42];
                pfi.ReadPowerRefVelocity = response[43];
                pfi.PIndRefVelocity      = response[44];
                pfi.PRefVelocity         = response[45];
                pfi.E1RefVelocity        = response[46];
                pfi.E2RefVelocity        = response[47];
                pfi.YTargetRefVelocity   = response[48];
                pfi.ReadPowerMinVelocity = response[49];
                pfi.PIndMinVelocity      = response[50];
                pfi.PMinVelocity         = response[51];
                pfi.E1MinVelocity        = response[52];
                pfi.E2MinVelocity        = response[53];
                pfi.YTargetMinVelocity   = response[54];

                break;
        }

        // DVD+R, DVD+RW, DVD+R DL and DVD+RW DL
        if(pfi.DiskCategory is DiskCategory.DVDPR or DiskCategory.DVDPRW or DiskCategory.DVDPRDL
           or DiskCategory.DVDPRWDL)
        {
            pfi.VCPS                |= (response[20] & 0x40) == 0x40;
            pfi.ApplicationCode     =  response[21];
            pfi.ExtendedInformation =  response[22];
            tmp                     =  new byte[8];
            Array.Copy(response, 23, tmp, 0, 8);
            pfi.DiskManufacturerID = StringHandlers.CToString(tmp);
            tmp                    = new byte[3];
            Array.Copy(response, 31, tmp, 0, 3);
            pfi.MediaTypeID = StringHandlers.CToString(tmp);

            pfi.ProductRevision = pfi.DiskCategory == DiskCategory.DVDPRDL ? (byte)(response[34] & 0x3F) : response[34];

            pfi.PFIUsedInADIP = response[35];
        }

        switch(pfi.DiskCategory)
        {
            // DVD+RW
            case DiskCategory.DVDPRW when pfi.PartVersion == 2:
                pfi.TopFirstPulseDuration    = response[55];
                pfi.MultiPulseDuration       = response[56];
                pfi.FirstPulseLeadTime       = response[57];
                pfi.EraseLeadTimeRefVelocity = response[58];
                pfi.EraseLeadTimeUppVelocity = response[59];

                break;

            // DVD+R and DVD+R DL
            case DiskCategory.DVDPR:
            case DiskCategory.DVDPRDL:
                pfi.PrimaryVelocity                      = response[36];
                pfi.UpperVelocity                        = response[37];
                pfi.Wavelength                           = response[38];
                pfi.NormalizedPowerDependency            = response[39];
                pfi.MaximumPowerAtPrimaryVelocity        = response[40];
                pfi.PindAtPrimaryVelocity                = response[41];
                pfi.BtargetAtPrimaryVelocity             = response[42];
                pfi.MaximumPowerAtUpperVelocity          = response[43];
                pfi.PindAtUpperVelocity                  = response[44];
                pfi.BtargetAtUpperVelocity               = response[45];
                pfi.FirstPulseDuration4TPrimaryVelocity  = response[46];
                pfi.FirstPulseDuration3TPrimaryVelocity  = response[47];
                pfi.MultiPulseDurationPrimaryVelocity    = response[48];
                pfi.LastPulseDurationPrimaryVelocity     = response[49];
                pfi.FirstPulseLeadTime4TPrimaryVelocity  = response[50];
                pfi.FirstPulseLeadTime3TPrimaryVelocity  = response[51];
                pfi.FirstPulseLeadingEdgePrimaryVelocity = response[52];
                pfi.FirstPulseDuration4TUpperVelocity    = response[53];
                pfi.FirstPulseDuration3TUpperVelocity    = response[54];
                pfi.MultiPulseDurationUpperVelocity      = response[55];
                pfi.LastPulseDurationUpperVelocity       = response[56];
                pfi.FirstPulseLeadTime4TUpperVelocity    = response[57];
                pfi.FirstPulseLeadTime3TUpperVelocity    = response[58];
                pfi.FirstPulseLeadingEdgeUpperVelocity   = response[59];

                break;
        }

        switch(pfi.DiskCategory)
        {
            // DVD+R DL
            case DiskCategory.DVDPRDL:
                pfi.LayerStructure = (DVDLayerStructure)((response[34] & 0xC0) >> 6);

                break;

            // DVD+RW DL
            case DiskCategory.DVDPRWDL:
                pfi.BasicPrimaryVelocity        = response[36];
                pfi.MaxReadPowerPrimaryVelocity = response[37];
                pfi.PindPrimaryVelocity         = response[38];
                pfi.PPrimaryVelocity            = response[39];
                pfi.E1PrimaryVelocity           = response[40];
                pfi.E2PrimaryVelocity           = response[41];
                pfi.YtargetPrimaryVelocity      = response[42];
                pfi.BOptimumPrimaryVelocity     = response[43];
                pfi.TFirstPulseDuration         = response[46];
                pfi.TMultiPulseDuration         = response[47];
                pfi.FirstPulseLeadTimeAnyRun    = response[48];
                pfi.FirstPulseLeadTimeRun3T     = response[49];
                pfi.LastPulseLeadTimeAnyRun     = response[50];
                pfi.LastPulseLeadTime3T         = response[51];
                pfi.LastPulseLeadTime4T         = response[52];
                pfi.ErasePulseLeadTimeAny       = response[53];
                pfi.ErasePulseLeadTime3T        = response[54];
                pfi.ErasePulseLeadTime4T        = response[55];

                break;

            // DVD-R DL and DVD-RW DL
            case DiskCategory.DVDR when pfi.PartVersion  >= 6:
            case DiskCategory.DVDRW when pfi.PartVersion >= 15:
                pfi.MaxRecordingSpeed = (DVDRecordingSpeed)response[21];
                pfi.MinRecordingSpeed = (DVDRecordingSpeed)response[22];
                pfi.RecordingSpeed1   = (DVDRecordingSpeed)response[23];
                pfi.RecordingSpeed2   = (DVDRecordingSpeed)response[24];
                pfi.RecordingSpeed3   = (DVDRecordingSpeed)response[25];
                pfi.RecordingSpeed4   = (DVDRecordingSpeed)response[26];
                pfi.RecordingSpeed5   = (DVDRecordingSpeed)response[27];
                pfi.RecordingSpeed6   = (DVDRecordingSpeed)response[28];
                pfi.RecordingSpeed7   = (DVDRecordingSpeed)response[29];
                pfi.Class             = response[30];
                pfi.ExtendedVersion   = response[31];

                pfi.CurrentBorderOutSector =
                    (uint)((response[36] << 24) + (response[37] << 16) + (response[38] << 8) + response[39]);

                pfi.NextBorderInSector =
                    (uint)((response[40] << 24) + (response[41] << 16) + (response[42] << 8) + response[43]);

                pfi.PreRecordedControlDataInv |= (response[44]       & 0x01) == 0x01;
                pfi.PreRecordedLeadIn         |= (response[44]       & 0x02) == 0x02;
                pfi.PreRecordedLeadOut        |= (response[44]       & 0x08) == 0x08;
                pfi.ARCharLayer1              =  (byte)(response[45] & 0x0F);
                pfi.TrackPolarityLayer1       =  (byte)((response[45] & 0xF0) >> 4);

                break;
        }

        return pfi;
    }

    public static string Prettify(PhysicalFormatInformation? pfi)
    {
        if(pfi == null)
            return null;

        PhysicalFormatInformation decoded = pfi.Value;
        var                       sb      = new StringBuilder();

        string sizeString = decoded.DiscSize switch
        {
            DVDSize.Eighty    => Localization._80mm,
            DVDSize.OneTwenty => Localization._120mm,
            _                 => string.Format(Localization.unknown_size_identifier_0, decoded.DiscSize)
        };

        switch(decoded.DiskCategory)
        {
            case DiskCategory.DVDROM:
                sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "DVD-ROM", decoded.PartVersion).
                   AppendLine();

                switch(decoded.DiscSize)
                {
                    case DVDSize.OneTwenty when decoded.PartVersion == 1:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_267);

                        break;
                    case DVDSize.Eighty when decoded.PartVersion == 1:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_268);

                        break;
                }

                break;
            case DiskCategory.DVDRAM:
                sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "DVD-RAM", decoded.PartVersion).
                   AppendLine();

                switch(decoded.PartVersion)
                {
                    case 1:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_272);

                        break;
                    case 6:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_330);

                        break;
                }

                break;
            case DiskCategory.DVDR:
                if(decoded.PartVersion >= 6)
                    sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "DVD-R DL", decoded.PartVersion).
                       AppendLine();
                else
                    sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "DVD-R", decoded.PartVersion).
                       AppendLine();

                switch(decoded.PartVersion)
                {
                    case 1:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_279);

                        break;
                    case 5:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_359);

                        break;
                    case 6:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_382);

                        break;
                }

                break;
            case DiskCategory.DVDRW:
                if(decoded.PartVersion >= 15)
                    sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "DVD-RW DL", decoded.PartVersion).
                       AppendLine();
                else
                    sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "DVD-RW", decoded.PartVersion).
                       AppendLine();

                switch(decoded.PartVersion)
                {
                    case 2:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_338);

                        break;
                    case 3:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_384);

                        break;
                }

                break;
            case DiskCategory.UMD:
                if(decoded.DiscSize == DVDSize.OneTwenty)
                    sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, Localization._60mm, "UMD",
                                    decoded.PartVersion).AppendLine();
                else
                    sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, Localization.invalid_size, "UMD",
                                    decoded.PartVersion).AppendLine();

                switch(decoded.PartVersion)
                {
                    case 0:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_365);

                        break;
                }

                break;
            case DiskCategory.DVDPRW:
                sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "DVD+RW", decoded.PartVersion).
                   AppendLine();

                switch(decoded.PartVersion)
                {
                    case 1:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_274);

                        break;
                    case 2:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_337);

                        break;
                    case 3:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_371);

                        break;
                }

                break;
            case DiskCategory.DVDPR:
                sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "DVD+R", decoded.PartVersion).
                   AppendLine();

                switch(decoded.PartVersion)
                {
                    case 1:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_349);

                        break;
                }

                break;
            case DiskCategory.DVDPRWDL:
                sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "DVD+RW DL", decoded.PartVersion).
                   AppendLine();

                switch(decoded.PartVersion)
                {
                    case 1:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_374);

                        break;
                }

                break;
            case DiskCategory.DVDPRDL:
                sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "DVD+R DL", decoded.PartVersion).
                   AppendLine();

                switch(decoded.PartVersion)
                {
                    case 1:
                        sb.AppendLine(Localization.Disc_claims_conformation_to_ECMA_364);

                        break;
                }

                break;
            case DiskCategory.Nintendo:
                if(decoded.PartVersion == 15)
                    if(decoded.DiscSize == DVDSize.Eighty)
                        sb.AppendLine(Localization.Disc_is_a_Nintendo_Gamecube_Optical_Disc_GOD);
                    else if(decoded.DiscSize == DVDSize.OneTwenty)
                        sb.AppendLine(Localization.Disc_is_a_Nintendo_Wii_Optical_Disc_WOD);
                    else
                        goto default;
                else
                    goto default;

                break;
            case DiskCategory.HDDVDROM:
                sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "HD DVD-ROM", decoded.PartVersion).
                   AppendLine();

                break;
            case DiskCategory.HDDVDRAM:
                sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "HD DVD-RAM", decoded.PartVersion).
                   AppendLine();

                break;
            case DiskCategory.HDDVDR:
                sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "HD DVD-R", decoded.PartVersion).
                   AppendLine();

                break;
            case DiskCategory.HDDVDRW:
                sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, "HD DVD-RW", decoded.PartVersion).
                   AppendLine();

                break;
            default:
                sb.AppendFormat(Localization.Disc_is_a_0_1_version_2, sizeString, Localization.unknown_disc_type,
                                decoded.PartVersion).AppendLine();

                break;
        }

        if(decoded.RecordedBookType != decoded.DiskCategory)
        {
            switch(decoded.RecordedBookType)
            {
                case DiskCategory.DVDROM:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, "DVD-ROM").AppendLine();

                    break;
                case DiskCategory.DVDRAM:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, "DVD-RAM").AppendLine();

                    break;
                case DiskCategory.DVDR:
                    if(decoded.PartVersion >= 6)
                        sb.AppendFormat(Localization.Disc_book_type_is_0, "DVD-R DL").AppendLine();
                    else
                        sb.AppendFormat(Localization.Disc_book_type_is_0, "DVD-R").AppendLine();

                    break;
                case DiskCategory.DVDRW:
                    if(decoded.PartVersion >= 15)
                        sb.AppendFormat(Localization.Disc_book_type_is_0, "DVD-RW DL").AppendLine();
                    else
                        sb.AppendFormat(Localization.Disc_book_type_is_0, "DVD-RW").AppendLine();

                    break;
                case DiskCategory.UMD:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, "UMD").AppendLine();

                    break;
                case DiskCategory.DVDPRW:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, "DVD+RW").AppendLine();

                    break;
                case DiskCategory.DVDPR:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, "DVD+R").AppendLine();

                    break;
                case DiskCategory.DVDPRWDL:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, "DVD+RW DL").AppendLine();

                    break;
                case DiskCategory.DVDPRDL:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, "DVD+R DL").AppendLine();

                    break;
                case DiskCategory.HDDVDROM:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, "HD DVD-ROM").AppendLine();

                    break;
                case DiskCategory.HDDVDRAM:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, "HD DVD-RAM").AppendLine();

                    break;
                case DiskCategory.HDDVDR:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, "HD DVD-R").AppendLine();

                    break;
                case DiskCategory.HDDVDRW:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, "HD DVD-RW").AppendLine();

                    break;
                default:
                    sb.AppendFormat(Localization.Disc_book_type_is_0, Localization.unit_unknown).AppendLine();

                    break;
            }
        }

        switch(decoded.MaximumRate)
        {
            case MaximumRateField.TwoMbps:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_2_52_Mbit_sec);

                break;
            case MaximumRateField.FiveMbps:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_5_04_Mbit_sec);

                break;
            case MaximumRateField.TenMbps:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_10_08_Mbit_sec);

                break;
            case MaximumRateField.TwentyMbps:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_20_16_Mbit_sec);

                break;
            case MaximumRateField.ThirtyMbps:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_30_24_Mbit_sec);

                break;
            case MaximumRateField.Unspecified:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_unspecified);

                break;
            default:
                sb.AppendFormat(Localization.Disc_maximum_transfer_rate_is_specified_by_unknown_key_0,
                                decoded.MaximumRate).AppendLine();

                break;
        }

        sb.AppendFormat(Localization.Disc_has_0_layers, decoded.Layers + 1).AppendLine();

        switch(decoded.TrackPath)
        {
            case true when decoded.Layers == 1:
                sb.AppendLine(Localization.Layers_are_in_parallel_track_path);

                break;
            case false when decoded.Layers == 1:
                sb.AppendLine(Localization.Layers_are_in_opposite_track_path);

                break;
        }

        switch(decoded.LinearDensity)
        {
            case LinearDensityField.TwoSix:
                sb.AppendLine(Localization.Pitch_size_is_0_267_μm_bit);

                break;
            case LinearDensityField.TwoNine:
                sb.AppendLine(Localization.Pitch_size_is_0_147_μm_bit);

                break;
            case LinearDensityField.FourZero:
                sb.AppendLine(Localization.Pitch_size_is_between_0_409_μm_bit_and_0_435_μm_bit);

                break;
            case LinearDensityField.TwoEight:
                sb.AppendLine(Localization.Pitch_size_is_between_0_140_μm_bit_and_0_148_μm_bit);

                break;
            case LinearDensityField.OneFive:
                sb.AppendLine(Localization.Pitch_size_is_0_153_μm_bit);

                break;
            case LinearDensityField.OneThree:
                sb.AppendLine(Localization.Pitch_size_is_between_0_130_μm_bit_and_0_140_μm_bit);

                break;
            case LinearDensityField.ThreeFive:
                sb.AppendLine(Localization.Pitch_size_is_0_353_μm_bit);

                break;
            default:
                sb.AppendFormat(Localization.Unknown_pitch_size_key_0, decoded.LinearDensity).AppendLine();

                break;
        }

        switch(decoded.TrackDensity)
        {
            case TrackDensityField.Seven:
                sb.AppendLine(Localization.Track_size_is_0_74_μm);

                break;
            case TrackDensityField.Eight:
                sb.AppendLine(Localization.Track_size_is_0_80_μm);

                break;
            case TrackDensityField.Six:
                sb.AppendLine(Localization.Track_size_is_0_615_μm);

                break;
            case TrackDensityField.Four:
                sb.AppendLine(Localization.Track_size_is_0_40_μm);

                break;
            case TrackDensityField.Three:
                sb.AppendLine(Localization.Track_size_is_0_34_μm);

                break;
            default:
                sb.AppendFormat(Localization.Unknown_track_size_key__0_, decoded.LinearDensity).AppendLine();

                break;
        }

        if(decoded.DataAreaStartPSN > 0)
            if(decoded.DataAreaEndPSN > 0)
            {
                sb.AppendFormat(Localization.Data_area_starts_at_PSN_0, decoded.DataAreaStartPSN).AppendLine();
                sb.AppendFormat(Localization.Data_area_ends_at_PSN_0, decoded.DataAreaEndPSN).AppendLine();

                if(decoded is { Layers: 1, TrackPath: false })
                    sb.AppendFormat(Localization.Layer_zero_ends_at_PSN_0, decoded.Layer0EndPSN).AppendLine();
            }
            else
                sb.AppendLine(Localization.Disc_is_empty);
        else
            sb.AppendLine(Localization.Disc_is_empty);

        if(decoded.BCA)
            sb.AppendLine(Localization.Disc_has_a_burst_cutting_area);

        switch(decoded.DiskCategory)
        {
            case DiskCategory.UMD:
                sb.AppendFormat(Localization.Media_attribute_is_0, decoded.MediaAttribute).AppendLine();

                break;
            case DiskCategory.DVDRAM:
                switch(decoded.DiscType)
                {
                    case DVDRAMDiscType.Cased:
                        sb.AppendLine(Localization.Disc_shall_be_recorded_with_a_case);

                        break;
                    case DVDRAMDiscType.Uncased:
                        sb.AppendLine(Localization.Disc_can_be_recorded_with_or_without_a_case);

                        break;
                    default:
                        sb.AppendFormat(Localization.Unknown_DVD_RAM_case_type_key_0, decoded.DiscType).AppendLine();

                        break;
                }

                if(decoded.PartVersion == 6)
                {
                    sb.AppendFormat(Localization.Disc_manufacturer_is_0,
                                    ManufacturerFromDVDRAM(decoded.DiskManufacturer)).AppendLine();

                    sb.AppendFormat(Localization.Disc_manufacturer_supplementary_information_is_0,
                                    decoded.DiskManufacturerSupplementary).AppendLine();
                }

                break;
            case DiskCategory.DVDR when decoded.PartVersion  < 6:
            case DiskCategory.DVDRW when decoded.PartVersion < 15:
                sb.AppendFormat(Localization.Current_Border_Out_first_sector_is_PSN_0, decoded.CurrentBorderOutSector).
                   AppendLine();

                sb.AppendFormat(Localization.Next_Border_In_first_sector_is_PSN_0, decoded.NextBorderInSector).
                   AppendLine();

                break;
            case DiskCategory.DVDPR:
            case DiskCategory.DVDPRW:
            case DiskCategory.DVDPRDL:
            case DiskCategory.DVDPRWDL:
                if(decoded.VCPS)
                    sb.AppendLine(Localization.Disc_contains_extended_information_for_VCPS);

                sb.AppendFormat(Localization.Disc_application_code_is_0, decoded.ApplicationCode).AppendLine();

                sb.AppendFormat(Localization.Disc_manufacturer_is_0,
                                ManufacturerFromDVDPlusID(decoded.DiskManufacturerID)).AppendLine();

                sb.AppendFormat(Localization.Disc_media_type_is_0, decoded.MediaTypeID).AppendLine();
                sb.AppendFormat(Localization.Disc_product_revision_is_0, decoded.ProductRevision).AppendLine();

                break;
        }

        if((decoded.DiskCategory != DiskCategory.DVDR  || decoded.PartVersion < 6) &&
           (decoded.DiskCategory != DiskCategory.DVDRW || decoded.PartVersion < 15))
            return sb.ToString();

        sb.AppendFormat(Localization.Current_RMD_in_extra_Border_zone_starts_at_PSN_0,
                        decoded.CurrentRMDExtraBorderPSN).AppendLine();

        sb.AppendFormat(Localization.PFI_in_extra_Border_zone_starts_at_PSN_0, decoded.PFIExtraBorderPSN).AppendLine();

        if(!decoded.PreRecordedControlDataInv)
            sb.AppendLine(Localization.Control_Data_Zone_is_pre_recorded);

        if(decoded.PreRecordedLeadIn)
            sb.AppendLine(Localization.Lead_In_is_pre_recorded);

        if(decoded.PreRecordedLeadOut)
            sb.AppendLine(Localization.Lead_Out_is_pre_recorded);

        return sb.ToString();
    }

    public static string Prettify(byte[] response, MediaType mediaType) => Prettify(Decode(response, mediaType));

    public static string ManufacturerFromDVDRAM(string manufacturerId) => manufacturerId switch
    {
        _ => ManufacturerFromDVDPlusID(manufacturerId)
    };

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static string ManufacturerFromDVDPlusID(string manufacturerId)
    {
        string manufacturer = "";

        switch(manufacturerId)
        {
            case "CMC MAG":
                manufacturer = "CMC Magnetics Corporation";

                break;
            case "INFOME":
                manufacturer = "InfoMedia Inc.";

                break;
            case "RITEK":
                manufacturer = "Ritek Co.";

                break;
            case "RICOHJPN":
                manufacturer = "Ricoh Company, Ltd.";

                break;
            case "ISSM":
                manufacturer = "Info Source Digital Media (Zhongshan) Co., Ltd.";

                break;
            case "LD":
                manufacturer = "Lead Data Inc.";

                break;
            case "MAXELL":
                manufacturer = "Hitachi Maxell, Ltd.";

                break;
            case "MCC":
                manufacturer = "Mitsubishi Kagaku Media Co., LTD.";

                break;
            case "PRODISC":
                manufacturer = "Prodisc Technology Inc.";

                break;
            case "Philips":
            case "PHILIPS":

                manufacturer = "Philips Components";

                break;
            case "YUDEN000":
                manufacturer = "Taiyo Yuden Company Ltd.";

                break;
            case "AML":
                manufacturer = "Avic Umedisc HK Ltd.";

                break;
            case "DAXON":
                manufacturer = "Daxon Technology Inc.";

                break;
            case "FTI":
                manufacturer = "Falcon Technologies International L.L.C.";

                break;
            case "GSC503":
                manufacturer = "Gigastore Corporation";

                break;
            case "MBIPG101":
                manufacturer = "Moser Baer India Ltd.";

                break;
            case "OPTODISC":
                manufacturer = "OptoDisc Ltd.";

                break;
            case "SONY":
                manufacturer = "Sony Corporation";

                break;
            case "TDK":
                manufacturer = "TDK Corporation";

                break;
            case "SENTINEL":
                manufacturer = "Sentinel B.V.";

                break;
            case "BeAll000":
                manufacturer = "BeALL Developers, Inc.";

                break;
            case "MPOMEDIA":
                manufacturer = "MPO Disque Compact";

                break;
            case "IMC JPN":
                manufacturer = "Intermedia Co., Ltd.";

                break;
            case "INFODISC":
                manufacturer = "InfoDisc Technology Co., Ltd.";

                break;
            case "WFKA11":
                manufacturer = "Wealth Fair Investment Inc.";

                break;
            case "MAM":
                manufacturer = "Manufacturing Advanced Media Europe";

                break;
            case "VDSPMSAB":
                manufacturer = "Interaxia Digital Storage Materials AG";

                break;
            case "KIC00000":
                manufacturer = "Advanced Media Corporation";

                break;
            case "MJC":
                manufacturer = "Megan Media Holdings Berhad";

                break;
            case "MUST":
                manufacturer = "Must Technology Co., Ltd.";

                break;
            case "IS02":
                manufacturer = "Infosmart Technology Ltd.";

                break;
            case "DDDessau":
                manufacturer = "Digital Disc Dessau GmbH";

                break;
            case "SKYMEDIA":
                manufacturer = "Sky Media Manufacturing S.A.";

                break;
            case "MICRON":
                manufacturer = "Eastgate Technology Ltd.";

                break;
            case "VIVA":
                manufacturer = "Viva Optical Disc Manufacturing Ltd.";

                break;
            case "EMDPZ3":
                manufacturer = "E-TOP Mediatek Inc.";

                break;
            case "LGEP16":
                manufacturer = "LG Electronics Inc.";

                break;
            case "POS":
                manufacturer = "POSTECH Corporation";

                break;
            case "Dvsn+160":
                manufacturer = "Digital Storage Technology Co., Ltd.";

                break;
            case "ODMS":
                manufacturer = "VDL Optical Disc Manufacturing Systems";

                break;
        }

        return manufacturer != "" ? $"{manufacturer} (\"{manufacturerId}\")" : $"\"{manufacturerId}\"";
    }

    public struct PhysicalFormatInformation
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;

        #region PFI common to all
        /// <summary>Byte 4, bits 7 to 4 Disk category field</summary>
        public DiskCategory DiskCategory;
        /// <summary>Byte 4, bits 3 to 0 Media version</summary>
        public byte PartVersion;
        /// <summary>Byte 5, bits 7 to 4 120mm if 0, 80mm if 1. If UMD (60mm) 0 also. Reserved rest of values</summary>
        public DVDSize DiscSize;
        /// <summary>Byte 5, bits 3 to 0 Maximum data rate</summary>
        public MaximumRateField MaximumRate;
        /// <summary>Byte 6, bit 7 Reserved</summary>
        public bool Reserved3;
        /// <summary>Byte 6, bits 6 to 5 Number of layers</summary>
        public byte Layers;
        /// <summary>Byte 6, bit 4 Track path</summary>
        public bool TrackPath;
        /// <summary>Byte 6, bits 3 to 0 Layer type</summary>
        public LayerTypeFieldMask LayerType;
        /// <summary>Byte 7, bits 7 to 4 Linear density field</summary>
        public LinearDensityField LinearDensity;
        /// <summary>Byte 7, bits 3 to 0 Track density field</summary>
        public TrackDensityField TrackDensity;
        /// <summary>Bytes 8 to 11 PSN where Data Area starts</summary>
        public uint DataAreaStartPSN;
        /// <summary>Bytes 12 to 15 PSN where Data Area ends</summary>
        public uint DataAreaEndPSN;
        /// <summary>Bytes 16 to 19 PSN where Data Area ends in Layer 0</summary>
        public uint Layer0EndPSN;
        /// <summary>
        ///     Byte 20, bit 7 True if BCA exists. GC/Wii discs do not have this bit set, but there is a BCA, making it
        ///     unreadable in normal DVD drives
        /// </summary>
        public bool BCA;
        /// <summary>Byte 20, bits 6 to 0 Reserved</summary>
        public byte Reserved4;
        #endregion PFI common to all

        #region UMD PFI
        /// <summary>Bytes 21 to 22 UMD only, media attribute, application-defined, part of media specific in rest of discs</summary>
        public ushort MediaAttribute;
        #endregion UMD PFI

        #region DVD-RAM PFI
        /// <summary>Byte 36 Disc type, respecting case recordability</summary>
        public DVDRAMDiscType DiscType;
        #endregion DVD-RAM PFI

        #region DVD-RAM PFI, Version 0001b
        /// <summary>Byte 52 Byte 504 in Version 0110b Linear velocity, in tenths of m/s</summary>
        public byte Velocity;
        /// <summary>Byte 53 Byte 505 in Version 0110b Read power on disk surface, tenths of mW</summary>
        public byte ReadPower;
        /// <summary>Byte 54 Byte 507 in Version 0110b Peak power on disk surface for recording land tracks</summary>
        public byte PeakPower;
        /// <summary>Byte 55 Bias power on disk surface for recording land tracks</summary>
        public byte BiasPower;
        /// <summary>Byte 56 First pulse starting time for recording on land tracks, ns</summary>
        public byte FirstPulseStart;
        /// <summary>Byte 57 Byte 515 in Version 0110b First pulse ending time for recording on land tracks</summary>
        public byte FirstPulseEnd;
        /// <summary>Byte 58 Byte 518 in Version 0110b Multiple-pulse duration time for recording on land tracks</summary>
        public byte MultiplePulseDuration;
        /// <summary>Byte 59 Byte 519 in Version 0110b Last pulse starting time for recording on land tracks</summary>
        public byte LastPulseStart;
        /// <summary>Byte 60 Las pulse ending time for recording on land tracks</summary>
        public byte LastPulseEnd;
        /// <summary>Byte 61 Bias power duration for recording on land tracks</summary>
        public byte BiasPowerDuration;
        /// <summary>Byte 62 Byte 511 on Version 0110b Peak power for recording on groove tracks</summary>
        public byte PeakPowerGroove;
        /// <summary>Byte 63 Bias power for recording on groove tracks</summary>
        public byte BiasPowerGroove;
        /// <summary>Byte 64 First pulse starting time on groove tracks</summary>
        public byte FirstPulseStartGroove;
        /// <summary>Byte 65 First pulse ending time on groove tracks</summary>
        public byte FirstPulseEndGroove;
        /// <summary>Byte 66 Multiple-pulse duration time on groove tracks</summary>
        public byte MultiplePulseDurationGroove;
        /// <summary>Byte 67 Last pulse starting time on groove tracks</summary>
        public byte LastPulseStartGroove;
        /// <summary>Byte 68 Last pulse ending time on groove tracks</summary>
        public byte LastPulseEndGroove;
        /// <summary>Byte 69 Bias power duration for recording on groove tracks</summary>
        public byte BiasPowerDurationGroove;
        #endregion DVD-RAM PFI, Version 0001b

        #region DVD-R PFI, DVD-RW PFI
        /// <summary>Bytes 36 to 39 Sector number of the first sector of the current Border Out</summary>
        public uint CurrentBorderOutSector;
        /// <summary>Bytes 40 to 43 Sector number of the first sector of the next Border In</summary>
        public uint NextBorderInSector;
        #endregion DVD-R PFI, DVD-RW PFI

        #region DVD+RW PFI
        /// <summary>Byte 36 Linear velocities 0 = CLV from 4,90 m/s to 6,25 m/s 1 = CAV from 3,02 m/s to 7,35 m/s</summary>
        public byte RecordingVelocity;
        /// <summary>Byte 37 Maximum read power in milliwatts at maximum velocity mW = 20 * (value - 1)</summary>
        public byte ReadPowerMaxVelocity;
        /// <summary>Byte 38 Indicative value of Ptarget in mW at maximum velocity</summary>
        public byte PIndMaxVelocity;
        /// <summary>Byte 39 Peak power multiplication factor at maximum velocity</summary>
        public byte PMaxVelocity;
        /// <summary>Byte 40 Bias1/write power ration at maximum velocity</summary>
        public byte E1MaxVelocity;
        /// <summary>Byte 41 Bias2/write power ration at maximum velocity</summary>
        public byte E2MaxVelocity;
        /// <summary>Byte 42 Target value for γ, γtarget at the maximum velocity</summary>
        public byte YTargetMaxVelocity;
        /// <summary>Byte 43 Maximum read power in milliwatts at reference velocity (4,90 m/s) mW = 20 * (value - 1)</summary>
        public byte ReadPowerRefVelocity;
        /// <summary>Byte 44 Indicative value of Ptarget in mW at reference velocity (4,90 m/s)</summary>
        public byte PIndRefVelocity;
        /// <summary>Byte 45 Peak power multiplication factor at reference velocity (4,90 m/s)</summary>
        public byte PRefVelocity;
        /// <summary>Byte 46 Bias1/write power ration at reference velocity (4,90 m/s)</summary>
        public byte E1RefVelocity;
        /// <summary>Byte 47 Bias2/write power ration at reference velocity (4,90 m/s)</summary>
        public byte E2RefVelocity;
        /// <summary>Byte 48 Target value for γ, γtarget at the reference velocity (4,90 m/s)</summary>
        public byte YTargetRefVelocity;
        /// <summary>Byte 49 Maximum read power in milliwatts at minimum velocity mW = 20 * (value - 1)</summary>
        public byte ReadPowerMinVelocity;
        /// <summary>Byte 50 Indicative value of Ptarget in mW at minimum velocity</summary>
        public byte PIndMinVelocity;
        /// <summary>Byte 51 Peak power multiplication factor at minimum velocity</summary>
        public byte PMinVelocity;
        /// <summary>Byte 52 Bias1/write power ration at minimum velocity</summary>
        public byte E1MinVelocity;
        /// <summary>Byte 53 Bias2/write power ration at minimum velocity</summary>
        public byte E2MinVelocity;
        /// <summary>Byte 54 Target value for γ, γtarget at the minimum velocity</summary>
        public byte YTargetMinVelocity;
        #endregion DVD+RW PFI

        #region DVD-RAM PFI, version 0110b
        /// <summary>Byte 506, bit 7 Mode of adaptative write pulse control</summary>
        public bool AdaptativeWritePulseControlFlag;
        /// <summary>Byte 508 Bias power 1 on disk surface for recording land tracks</summary>
        public byte BiasPower1;
        /// <summary>Byte 509 Bias power 2 on disk surface for recording land tracks</summary>
        public byte BiasPower2;
        /// <summary>Byte 510 Bias power 3 on disk surface for recording land tracks</summary>
        public byte BiasPower3;
        /// <summary>Byte 512 Bias power 1 on disk surface for recording groove tracks</summary>
        public byte BiasPower1Groove;
        /// <summary>Byte 513 Bias power 2 on disk surface for recording groove tracks</summary>
        public byte BiasPower2Groove;
        /// <summary>Byte 514 Bias power 3 on disk surface for recording groove tracks</summary>
        public byte BiasPower3Groove;
        /// <summary>Byte 516 First pulse duration</summary>
        public byte FirstPulseDuration;
        /// <summary>Byte 520 Bias power 2 duration on land tracks at Velocity 1</summary>
        public byte BiasPower2Duration;
        /// <summary>Byte 521 First pulse start time, at Mark 3T and Leading Space 3T</summary>
        public byte FirstPulseStart3TSpace3T;
        /// <summary>Byte 522 First pulse start time, at Mark 4T and Leading Space 3T</summary>
        public byte FirstPulseStart4TSpace3T;
        /// <summary>Byte 523 First pulse start time, at Mark 5T and Leading Space 3T</summary>
        public byte FirstPulseStart5TSpace3T;
        /// <summary>Byte 524 First pulse start time, at Mark >5T and Leading Space 3T</summary>
        public byte FirstPulseStartSpace3T;
        /// <summary>Byte 525 First pulse start time, at Mark 3T and Leading Space 4T</summary>
        public byte FirstPulseStart3TSpace4T;
        /// <summary>Byte 526 First pulse start time, at Mark 4T and Leading Space 4T</summary>
        public byte FirstPulseStart4TSpace4T;
        /// <summary>Byte 527 First pulse start time, at Mark 5T and Leading Space 4T</summary>
        public byte FirstPulseStart5TSpace4T;
        /// <summary>Byte 528 First pulse start time, at Mark >5T and Leading Space 4T</summary>
        public byte FirstPulseStartSpace4T;
        /// <summary>Byte 529 First pulse start time, at Mark 3T and Leading Space 5T</summary>
        public byte FirstPulseStart3TSpace5T;
        /// <summary>Byte 530 First pulse start time, at Mark 4T and Leading Space 5T</summary>
        public byte FirstPulseStart4TSpace5T;
        /// <summary>Byte 531 First pulse start time, at Mark 5T and Leading Space 5T</summary>
        public byte FirstPulseStart5TSpace5T;
        /// <summary>Byte 532 First pulse start time, at Mark >5T and Leading Space 5T</summary>
        public byte FirstPulseStartSpace5T;
        /// <summary>Byte 533 First pulse start time, at Mark 3T and Leading Space >5T</summary>
        public byte FirstPulseStart3TSpace;
        /// <summary>Byte 534 First pulse start time, at Mark 4T and Leading Space >5T</summary>
        public byte FirstPulseStart4TSpace;
        /// <summary>Byte 535 First pulse start time, at Mark 5T and Leading Space >5T</summary>
        public byte FirstPulseStart5TSpace;
        /// <summary>Byte 536 First pulse start time, at Mark >5T and Leading Space >5T</summary>
        public byte FirstPulseStartSpace;
        /// <summary>Byte 537 First pulse start time, at Mark 3T and Trailing Space 3T</summary>
        public byte FirstPulse3TStartTSpace3T;
        /// <summary>Byte 538 First pulse start time, at Mark 4T and Trailing Space 3T</summary>
        public byte FirstPulse4TStartTSpace3T;
        /// <summary>Byte 539 First pulse start time, at Mark 5T and Trailing Space 3T</summary>
        public byte FirstPulse5TStartTSpace3T;
        /// <summary>Byte 540 First pulse start time, at Mark >5T and Trailing Space 3T</summary>
        public byte FirstPulseStartTSpace3T;
        /// <summary>Byte 541 First pulse start time, at Mark 3T and Trailing Space 4T</summary>
        public byte FirstPulse3TStartTSpace4T;
        /// <summary>Byte 542 First pulse start time, at Mark 4T and Trailing Space 4T</summary>
        public byte FirstPulse4TStartTSpace4T;
        /// <summary>Byte 543 First pulse start time, at Mark 5T and Trailing Space 4T</summary>
        public byte FirstPulse5TStartTSpace4T;
        /// <summary>Byte 544 First pulse start time, at Mark >5T and Trailing Space 4T</summary>
        public byte FirstPulseStartTSpace4T;
        /// <summary>Byte 545 First pulse start time, at Mark 3T and Trailing Space 5T</summary>
        public byte FirstPulse3TStartTSpace5T;
        /// <summary>Byte 546 First pulse start time, at Mark 4T and Trailing Space 5T</summary>
        public byte FirstPulse4TStartTSpace5T;
        /// <summary>Byte 547 First pulse start time, at Mark 5T and Trailing Space 5T</summary>
        public byte FirstPulse5TStartTSpace5T;
        /// <summary>Byte 548 First pulse start time, at Mark >5T and Trailing Space 5T</summary>
        public byte FirstPulseStartTSpace5T;
        /// <summary>Byte 549 First pulse start time, at Mark 3T and Trailing Space >5T</summary>
        public byte FirstPulse3TStartTSpace;
        /// <summary>Byte 550 First pulse start time, at Mark 4T and Trailing Space >5T</summary>
        public byte FirstPulse4TStartTSpace;
        /// <summary>Byte 551 First pulse start time, at Mark 5T and Trailing Space >5T</summary>
        public byte FirstPulse5TStartTSpace;
        /// <summary>Byte 552 First pulse start time, at Mark >5T and Trailing Space >5T</summary>
        public byte FirstPulseStartTSpace;
        /// <summary>Bytes 553 to 600 Disk manufacturer's name, space-padded</summary>
        public string DiskManufacturer;
        /// <summary>Bytes 601 to 616 Disk manufacturer's supplementary information</summary>
        public string DiskManufacturerSupplementary;
        /// <summary>Bytes 617 to 627 Write power control parameters</summary>
        public byte[] WritePowerControlParams;
        /// <summary>Byte 619 Ratio of peak power for land tracks to threshold peak power for land tracks</summary>
        public byte PowerRatioLandThreshold;
        /// <summary>Byte 620 Target asymmetry</summary>
        public byte TargetAsymmetry;
        /// <summary>Byte 621 Temporary peak power</summary>
        public byte TemporaryPeakPower;
        /// <summary>Byte 622 Temporary bias power 1</summary>
        public byte TemporaryBiasPower1;
        /// <summary>Byte 623 Temporary bias power 2</summary>
        public byte TemporaryBiasPower2;
        /// <summary>Byte 624 Temporary bias power 3</summary>
        public byte TemporaryBiasPower3;
        /// <summary>Byte 625 Ratio of peak power for groove tracks to threshold peak power for groove tracks</summary>
        public byte PowerRatioGrooveThreshold;
        /// <summary>Byte 626 Ratio of peak power for land tracks to threshold 6T peak power for land tracks</summary>
        public byte PowerRatioLandThreshold6T;
        /// <summary>Byte 627 Ratio of peak power for groove tracks to threshold 6T peak power for groove tracks</summary>
        public byte PowerRatioGrooveThreshold6T;
        #endregion DVD-RAM PFI, version 0110b

        #region DVD+RW PFI, DVD+R PFI, DVD+R DL PFI and DVD+RW DL PFI
        /// <summary>Byte 20, bit 6 If set indicates data zone contains extended information for VCPS</summary>
        public bool VCPS;
        /// <summary>Byte 21 Indicates restricted usage disk</summary>
        public byte ApplicationCode;
        /// <summary>Byte 22 Bitmap of extended information block presence</summary>
        public byte ExtendedInformation;
        /// <summary>Bytes 23 to 30 Disk manufacturer ID, null-padded</summary>
        public string DiskManufacturerID;
        /// <summary>Bytes 31 to 33 Media type ID, null-padded</summary>
        public string MediaTypeID;
        /// <summary>Byte 34 Product revision number</summary>
        public byte ProductRevision;
        /// <summary>Byte 35 Indicates how many bytes, up to 63, are used in ADIP's PFI</summary>
        public byte PFIUsedInADIP;
        #endregion DVD+RW PFI, DVD+R PFI, DVD+R DL PFI and DVD+RW DL PFI

        #region DVD+RW PFI, version 0010b
        /// <summary>Byte 55 Ttop first pulse duration</summary>
        public byte TopFirstPulseDuration;
        /// <summary>Byte 56 Tmp multi pulse duration</summary>
        public byte MultiPulseDuration;
        /// <summary>Byte 57 dTtop first pulse lead time</summary>
        public byte FirstPulseLeadTime;
        /// <summary>Byte 58 dTera erase lead time at reference velocity</summary>
        public byte EraseLeadTimeRefVelocity;
        /// <summary>Byte 59 dTera erase lead time at upper velocity</summary>
        public byte EraseLeadTimeUppVelocity;
        #endregion DVD+RW PFI, version 0010b

        #region DVD+R PFI version 0001b and DVD+R DL PFI version 0001b
        /// <summary>Byte 36 Primary recording velocity for the basic write strategy</summary>
        public byte PrimaryVelocity;
        /// <summary>Byte 37 Upper recording velocity for the basic write strategy</summary>
        public byte UpperVelocity;
        /// <summary>Byte 38 Wavelength λIND</summary>
        public byte Wavelength;
        /// <summary>Byte 39 Normalized write power dependency on wavelength (dP/dλ)/(PIND/λIND)</summary>
        public byte NormalizedPowerDependency;
        /// <summary>Byte 40 Maximum read power at primary velocity</summary>
        public byte MaximumPowerAtPrimaryVelocity;
        /// <summary>Byte 41 Pind at primary velocity</summary>
        public byte PindAtPrimaryVelocity;
        /// <summary>Byte 42 βtarget at primary velocity</summary>
        public byte BtargetAtPrimaryVelocity;
        /// <summary>Byte 43 Maximum read power at upper velocity</summary>
        public byte MaximumPowerAtUpperVelocity;
        /// <summary>Byte 44 Pind at primary velocity</summary>
        public byte PindAtUpperVelocity;
        /// <summary>Byte 45 βtarget at upper velocity</summary>
        public byte BtargetAtUpperVelocity;
        /// <summary>Byte 46 Ttop (≥4T) first pulse duration for cm∗ ≥4T at Primary velocity</summary>
        public byte FirstPulseDuration4TPrimaryVelocity;
        /// <summary>Byte 47 Ttop (=3T) first pulse duration for cm∗ =3T at Primary velocity</summary>
        public byte FirstPulseDuration3TPrimaryVelocity;
        /// <summary>Byte 48 Tmp multi pulse duration at Primary velocity</summary>
        public byte MultiPulseDurationPrimaryVelocity;
        /// <summary>Byte 49 Tlp last pulse duration at Primary velocity</summary>
        public byte LastPulseDurationPrimaryVelocity;
        /// <summary>Byte 50 dTtop (≥4T) first pulse lead time for cm∗ ≥4T at Primary velocity</summary>
        public byte FirstPulseLeadTime4TPrimaryVelocity;
        /// <summary>Byte 51 dTtop (=3T) first pulse lead time for cm∗ =3T at Primary velocity</summary>
        public byte FirstPulseLeadTime3TPrimaryVelocity;
        /// <summary>Byte 52 dTle first pulse leading edge shift for ps∗ =3T at Primary velocity</summary>
        public byte FirstPulseLeadingEdgePrimaryVelocity;
        /// <summary>Byte 53 Ttop (≥4T) first pulse duration for cm∗ ≥4T at Upper velocity</summary>
        public byte FirstPulseDuration4TUpperVelocity;
        /// <summary>Byte 54 Ttop (=3T) first pulse duration for cm∗ =3T at Upper velocity</summary>
        public byte FirstPulseDuration3TUpperVelocity;
        /// <summary>Byte 55 Tmp multi pulse duration at Upper velocity</summary>
        public byte MultiPulseDurationUpperVelocity;
        /// <summary>Byte 56 Tlp last pulse duration at Upper velocity</summary>
        public byte LastPulseDurationUpperVelocity;
        /// <summary>Byte 57 dTtop (≥4T) first pulse lead time for cm∗ ≥4T at Upper velocity</summary>
        public byte FirstPulseLeadTime4TUpperVelocity;
        /// <summary>Byte 58 dTtop (=3T) first pulse lead time for cm∗ =3T at Upper velocity</summary>
        public byte FirstPulseLeadTime3TUpperVelocity;
        /// <summary>Byte 59 dTle first pulse leading edge shift for ps∗ =3T at Upper velocity</summary>
        public byte FirstPulseLeadingEdgeUpperVelocity;
        #endregion DVD+R PFI version 0001b and DVD+R DL PFI version 0001b

        #region DVD+R DL PFI version 0001b
        /// <summary>Byte 34, bits 7 to 6</summary>
        public DVDLayerStructure LayerStructure;
        #endregion DVD+R DL PFI version 0001b

        #region DVD+RW DL PFI
        /// <summary>Byte 36 Primary recording velocity for the basic write strategy</summary>
        public byte BasicPrimaryVelocity;
        /// <summary>Byte 37 Maximum read power at Primary velocity</summary>
        public byte MaxReadPowerPrimaryVelocity;
        /// <summary>Byte 38 PIND at Primary velocity</summary>
        public byte PindPrimaryVelocity;
        /// <summary>Byte 39 ρ at Primary velocity</summary>
        public byte PPrimaryVelocity;
        /// <summary>Byte 40 ε1 at Primary velocity</summary>
        public byte E1PrimaryVelocity;
        /// <summary>Byte 41 ε2 at Primary velocity</summary>
        public byte E2PrimaryVelocity;
        /// <summary>Byte 42 γtarget at Primary velocity</summary>
        public byte YtargetPrimaryVelocity;
        /// <summary>Byte 43 β optimum at Primary velocity</summary>
        public byte BOptimumPrimaryVelocity;
        /// <summary>Byte 46 Ttop first pulse duration</summary>
        public byte TFirstPulseDuration;
        /// <summary>Byte 47 Tmp multi pulse duration</summary>
        public byte TMultiPulseDuration;
        /// <summary>Byte 48 dTtop first pulse lead/lag time for any runlength ≥ 4T</summary>
        public byte FirstPulseLeadTimeAnyRun;
        /// <summary>Byte 49 dTtop,3 first pulse lead/lag time for runlengths = 3T</summary>
        public byte FirstPulseLeadTimeRun3T;
        /// <summary>Byte 50 dTlp last pulse lead/lag time for any runlength ≥ 5T</summary>
        public byte LastPulseLeadTimeAnyRun;
        /// <summary>Byte 51 dTlp,3 last pulse lead/lag time for runlengths = 3T</summary>
        public byte LastPulseLeadTime3T;
        /// <summary>Byte 52 dTlp,4 last pulse lead/lag time for runlengths = 4T</summary>
        public byte LastPulseLeadTime4T;
        /// <summary>Byte 53 dTera erase lead/lag time when preceding mark length ≥ 5T</summary>
        public byte ErasePulseLeadTimeAny;
        /// <summary>Byte 54 dTera,3 erase lead/lag time when preceding mark length = 3T</summary>
        public byte ErasePulseLeadTime3T;
        /// <summary>Byte 55 dTera,4 erase lead/lag time when preceding mark length = 4T</summary>
        public byte ErasePulseLeadTime4T;
        #endregion DVD+RW DL PFI

        #region DVD-R DL PFI and DVD-RW DL PFI
        /// <summary>Byte 21 Maximum recording speed</summary>
        public DVDRecordingSpeed MaxRecordingSpeed;
        /// <summary>Byte 22 Minimum recording speed</summary>
        public DVDRecordingSpeed MinRecordingSpeed;
        /// <summary>Byte 23 Another recording speed</summary>
        public DVDRecordingSpeed RecordingSpeed1;
        /// <summary>Byte 24 Another recording speed</summary>
        public DVDRecordingSpeed RecordingSpeed2;
        /// <summary>Byte 25 Another recording speed</summary>
        public DVDRecordingSpeed RecordingSpeed3;
        /// <summary>Byte 26 Another recording speed</summary>
        public DVDRecordingSpeed RecordingSpeed4;
        /// <summary>Byte 27 Another recording speed</summary>
        public DVDRecordingSpeed RecordingSpeed5;
        /// <summary>Byte 28 Another recording speed</summary>
        public DVDRecordingSpeed RecordingSpeed6;
        /// <summary>Byte 29 Another recording speed</summary>
        public DVDRecordingSpeed RecordingSpeed7;
        /// <summary>Byte 30 Class</summary>
        public byte Class;
        /// <summary>Byte 31 Extended version. 0x30 = ECMA-382, 0x20 = ECMA-384</summary>
        public byte ExtendedVersion;
        /// <summary>Byte 36 Start sector number of current RMD in Extra Border Zone</summary>
        public uint CurrentRMDExtraBorderPSN;
        /// <summary>Byte 40 Start sector number of Physical Format Information blocks in Extra Border Zone</summary>
        public uint PFIExtraBorderPSN;
        /// <summary>Byte 44, bit 0 If NOT set, Control Data Zone is pre-recorded</summary>
        public bool PreRecordedControlDataInv;
        /// <summary>Byte 44 bit 1 Lead-in Zone is pre-recorded</summary>
        public bool PreRecordedLeadIn;
        /// <summary>Byte 44 bit 3 Lead-out Zone is pre-recorded</summary>
        public bool PreRecordedLeadOut;
        /// <summary>Byte 45 bits 0 to 3 AR characteristic of LPP on Layer 1</summary>
        public byte ARCharLayer1;
        /// <summary>Byte 45 bits 4 to 7 Tracking polarity on Layer 1</summary>
        public byte TrackPolarityLayer1;
        #endregion DVD-R DL PFI and DVD-RW DL PFI

        public DiskCategory RecordedBookType;
    }
}