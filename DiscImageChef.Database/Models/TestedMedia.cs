// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : TestedMedia.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for tested media reports.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Database.Models
{
    public class TestedMedia : BaseEntity
    {
        public ulong?  Blocks                           { get; set; }
        public uint?   BlockSize                        { get; set; }
        public bool?   CanReadAACS                      { get; set; }
        public bool?   CanReadADIP                      { get; set; }
        public bool?   CanReadATIP                      { get; set; }
        public bool?   CanReadBCA                       { get; set; }
        public bool?   CanReadC2Pointers                { get; set; }
        public bool?   CanReadCMI                       { get; set; }
        public bool?   CanReadCorrectedSubchannel       { get; set; }
        public bool?   CanReadCorrectedSubchannelWithC2 { get; set; }
        public bool?   CanReadDCB                       { get; set; }
        public bool?   CanReadDDS                       { get; set; }
        public bool?   CanReadDMI                       { get; set; }
        public bool?   CanReadDiscInformation           { get; set; }
        public bool?   CanReadFullTOC                   { get; set; }
        public bool?   CanReadHDCMI                     { get; set; }
        public bool?   CanReadLayerCapacity             { get; set; }
        public bool?   CanReadLeadIn                    { get; set; }
        public bool?   CanReadLeadInPostgap             { get; set; }
        public bool?   CanReadLeadOut                   { get; set; }
        public bool?   CanReadMediaID                   { get; set; }
        public bool?   CanReadMediaSerial               { get; set; }
        public bool?   CanReadPAC                       { get; set; }
        public bool?   CanReadPFI                       { get; set; }
        public bool?   CanReadPMA                       { get; set; }
        public bool?   CanReadPQSubchannel              { get; set; }
        public bool?   CanReadPQSubchannelWithC2        { get; set; }
        public bool?   CanReadPRI                       { get; set; }
        public bool?   CanReadRWSubchannel              { get; set; }
        public bool?   CanReadRWSubchannelWithC2        { get; set; }
        public bool?   CanReadRecordablePFI             { get; set; }
        public bool?   CanReadSpareAreaInformation      { get; set; }
        public bool?   CanReadTOC                       { get; set; }
        public byte?   Density                          { get; set; }
        public uint?   LongBlockSize                    { get; set; }
        public string  Manufacturer                     { get; set; }
        public bool    MediaIsRecognized                { get; set; }
        public byte?   MediumType                       { get; set; }
        public string  MediumTypeName                   { get; set; }
        public string  Model                            { get; set; }
        public bool?   SupportsHLDTSTReadRawDVD         { get; set; }
        public bool?   SupportsNECReadCDDA              { get; set; }
        public bool?   SupportsPioneerReadCDDA          { get; set; }
        public bool?   SupportsPioneerReadCDDAMSF       { get; set; }
        public bool?   SupportsPlextorReadCDDA          { get; set; }
        public bool?   SupportsPlextorReadRawDVD        { get; set; }
        public bool?   SupportsRead10                   { get; set; }
        public bool?   SupportsRead12                   { get; set; }
        public bool?   SupportsRead16                   { get; set; }
        public bool?   SupportsRead                     { get; set; }
        public bool?   SupportsReadCapacity16           { get; set; }
        public bool?   SupportsReadCapacity             { get; set; }
        public bool?   SupportsReadCd                   { get; set; }
        public bool?   SupportsReadCdMsf                { get; set; }
        public bool?   SupportsReadCdRaw                { get; set; }
        public bool?   SupportsReadCdMsfRaw             { get; set; }
        public bool?   SupportsReadLong16               { get; set; }
        public bool?   SupportsReadLong                 { get; set; }
        public byte[]  ModeSense6Data                   { get; set; }
        public byte[]  ModeSense10Data                  { get; set; }
        public CHS     CHS                              { get; set; }
        public CHS     CurrentCHS                       { get; set; }
        public uint?   LBASectors                       { get; set; }
        public ulong?  LBA48Sectors                     { get; set; }
        public ushort? LogicalAlignment                 { get; set; }
        public ushort? NominalRotationRate              { get; set; }
        public uint?   PhysicalBlockSize                { get; set; }
        public bool?   SolidStateDevice                 { get; set; }
        public ushort? UnformattedBPT                   { get; set; }
        public ushort? UnformattedBPS                   { get; set; }
        public bool?   SupportsReadDmaLba               { get; set; }
        public bool?   SupportsReadDmaRetryLba          { get; set; }
        public bool?   SupportsReadLba                  { get; set; }
        public bool?   SupportsReadRetryLba             { get; set; }
        public bool?   SupportsReadLongLba              { get; set; }
        public bool?   SupportsReadLongRetryLba         { get; set; }
        public bool?   SupportsSeekLba                  { get; set; }
        public bool?   SupportsReadDmaLba48             { get; set; }
        public bool?   SupportsReadLba48                { get; set; }
        public bool?   SupportsReadDma                  { get; set; }
        public bool?   SupportsReadDmaRetry             { get; set; }
        public bool?   SupportsReadRetry                { get; set; }
        public bool?   SupportsReadLongRetry            { get; set; }
        public bool?   SupportsSeek                     { get; set; }

        public static TestedMedia MapTestedMedia(testedMediaType oldMedia)
        {
            if(oldMedia == null) return null;

            TestedMedia newMedia                                               = new TestedMedia();
            if(oldMedia.BlocksSpecified) newMedia.Blocks                       = oldMedia.Blocks;
            if(oldMedia.BlockSizeSpecified) newMedia.BlockSize                 = oldMedia.BlockSize;
            if(oldMedia.CanReadAACSSpecified) newMedia.CanReadAACS             = oldMedia.CanReadAACS;
            if(oldMedia.CanReadADIPSpecified) newMedia.CanReadADIP             = oldMedia.CanReadADIP;
            if(oldMedia.CanReadATIPSpecified) newMedia.CanReadATIP             = oldMedia.CanReadATIP;
            if(oldMedia.CanReadBCASpecified) newMedia.CanReadBCA               = oldMedia.CanReadBCA;
            if(oldMedia.CanReadC2PointersSpecified) newMedia.CanReadC2Pointers = oldMedia.CanReadC2Pointers;
            if(oldMedia.CanReadCMISpecified) newMedia.CanReadCMI               = oldMedia.CanReadCMI;
            if(oldMedia.CanReadCorrectedSubchannelSpecified)
                newMedia.CanReadCorrectedSubchannel = oldMedia.CanReadCorrectedSubchannel;
            if(oldMedia.CanReadCorrectedSubchannelWithC2Specified)
                newMedia.CanReadCorrectedSubchannelWithC2 = oldMedia.CanReadCorrectedSubchannelWithC2;
            if(oldMedia.CanReadDCBSpecified) newMedia.CanReadDCB = oldMedia.CanReadDCB;
            if(oldMedia.CanReadDDSSpecified) newMedia.CanReadDDS = oldMedia.CanReadDDS;
            if(oldMedia.CanReadDMISpecified) newMedia.CanReadDMI = oldMedia.CanReadDMI;
            if(oldMedia.CanReadDiscInformationSpecified)
                newMedia.CanReadDiscInformation = oldMedia.CanReadDiscInformation;
            if(oldMedia.CanReadFullTOCSpecified) newMedia.CanReadFullTOC             = oldMedia.CanReadFullTOC;
            if(oldMedia.CanReadHDCMISpecified) newMedia.CanReadHDCMI                 = oldMedia.CanReadHDCMI;
            if(oldMedia.CanReadLayerCapacitySpecified) newMedia.CanReadLayerCapacity = oldMedia.CanReadLayerCapacity;
            if(oldMedia.CanReadLeadInSpecified) newMedia.CanReadLeadInPostgap        = oldMedia.CanReadLeadIn;
            if(oldMedia.CanReadLeadOutSpecified) newMedia.CanReadLeadOut             = oldMedia.CanReadLeadOut;
            if(oldMedia.CanReadMediaIDSpecified) newMedia.CanReadMediaID             = oldMedia.CanReadMediaID;
            if(oldMedia.CanReadMediaSerialSpecified) newMedia.CanReadMediaSerial     = oldMedia.CanReadMediaSerial;
            if(oldMedia.CanReadPACSpecified) newMedia.CanReadPAC                     = oldMedia.CanReadPAC;
            if(oldMedia.CanReadPFISpecified) newMedia.CanReadPFI                     = oldMedia.CanReadPFI;
            if(oldMedia.CanReadPMASpecified) newMedia.CanReadPMA                     = oldMedia.CanReadPMA;
            if(oldMedia.CanReadPQSubchannelSpecified) newMedia.CanReadPQSubchannel   = oldMedia.CanReadPQSubchannel;
            if(oldMedia.CanReadPQSubchannelWithC2Specified)
                newMedia.CanReadPQSubchannelWithC2 = oldMedia.CanReadPQSubchannelWithC2;
            if(oldMedia.CanReadPRISpecified) newMedia.CanReadPRI                   = oldMedia.CanReadPRI;
            if(oldMedia.CanReadRWSubchannelSpecified) newMedia.CanReadRWSubchannel = oldMedia.CanReadRWSubchannel;
            if(oldMedia.CanReadRWSubchannelWithC2Specified)
                newMedia.CanReadRWSubchannelWithC2 = oldMedia.CanReadRWSubchannelWithC2;
            if(oldMedia.CanReadRecordablePFISpecified) newMedia.CanReadRecordablePFI = oldMedia.CanReadRecordablePFI;
            if(oldMedia.CanReadSpareAreaInformationSpecified)
                newMedia.CanReadSpareAreaInformation = oldMedia.CanReadSpareAreaInformation;
            if(oldMedia.CanReadTOCSpecified) newMedia.CanReadTOC       = oldMedia.CanReadTOC;
            if(oldMedia.DensitySpecified) newMedia.Density             = oldMedia.Density;
            if(oldMedia.LongBlockSizeSpecified) newMedia.LongBlockSize = oldMedia.LongBlockSize;
            if(oldMedia.ManufacturerSpecified) newMedia.Manufacturer   = oldMedia.Manufacturer;
            newMedia.MediaIsRecognized = oldMedia.MediaIsRecognized;
            if(oldMedia.MediumTypeSpecified) newMedia.MediumType = oldMedia.MediumType;
            newMedia.MediumTypeName = oldMedia.MediumTypeName;
            if(oldMedia.ModelSpecified) newMedia.Model = oldMedia.Model;
            if(oldMedia.SupportsHLDTSTReadRawDVDSpecified)
                newMedia.SupportsHLDTSTReadRawDVD = oldMedia.SupportsHLDTSTReadRawDVD;
            if(oldMedia.SupportsNECReadCDDASpecified) newMedia.SupportsNECReadCDDA = oldMedia.SupportsNECReadCDDA;
            if(oldMedia.SupportsPioneerReadCDDASpecified)
                newMedia.SupportsPioneerReadCDDA = oldMedia.SupportsPioneerReadCDDA;
            if(oldMedia.SupportsPioneerReadCDDAMSFSpecified)
                newMedia.SupportsPioneerReadCDDAMSF = oldMedia.SupportsPioneerReadCDDAMSF;
            if(oldMedia.SupportsPlextorReadCDDASpecified)
                newMedia.SupportsPlextorReadCDDA = oldMedia.SupportsPlextorReadCDDA;
            if(oldMedia.SupportsPlextorReadRawDVDSpecified)
                newMedia.SupportsPlextorReadRawDVD = oldMedia.SupportsPlextorReadRawDVD;
            if(oldMedia.SupportsRead10Specified) newMedia.SupportsRead10 = oldMedia.SupportsRead10;
            if(oldMedia.SupportsRead12Specified) newMedia.SupportsRead12 = oldMedia.SupportsRead12;
            if(oldMedia.SupportsRead16Specified) newMedia.SupportsRead16 = oldMedia.SupportsRead16;
            if(oldMedia.SupportsReadSpecified) newMedia.SupportsRead     = oldMedia.SupportsRead;
            if(oldMedia.SupportsReadCapacity16Specified)
                newMedia.SupportsReadCapacity16 = oldMedia.SupportsReadCapacity16;
            if(oldMedia.SupportsReadCapacitySpecified) newMedia.SupportsReadCapacity = oldMedia.SupportsReadCapacity;
            if(oldMedia.SupportsReadCdSpecified) newMedia.SupportsReadCd             = oldMedia.SupportsReadCd;
            if(oldMedia.SupportsReadCdMsfSpecified) newMedia.SupportsReadCdMsf       = oldMedia.SupportsReadCdMsf;
            if(oldMedia.SupportsReadCdRawSpecified) newMedia.SupportsReadCdRaw       = oldMedia.SupportsReadCdRaw;
            if(oldMedia.SupportsReadCdMsfRawSpecified) newMedia.SupportsReadCdMsfRaw = oldMedia.SupportsReadCdMsfRaw;
            if(oldMedia.SupportsReadLong16Specified) newMedia.SupportsReadLong16     = oldMedia.SupportsReadLong16;
            if(oldMedia.SupportsReadLongSpecified) newMedia.SupportsReadLong         = oldMedia.SupportsReadLong;
            newMedia.ModeSense6Data  = oldMedia.ModeSense6Data;
            newMedia.ModeSense10Data = oldMedia.ModeSense10Data;
            if(oldMedia.LBASectorsSpecified) newMedia.LBASectors                   = oldMedia.LBASectors;
            if(oldMedia.LBA48SectorsSpecified) newMedia.LBA48Sectors               = oldMedia.LBA48Sectors;
            if(oldMedia.LogicalAlignmentSpecified) newMedia.LogicalAlignment       = oldMedia.LogicalAlignment;
            if(oldMedia.NominalRotationRateSpecified) newMedia.NominalRotationRate = oldMedia.NominalRotationRate;
            if(oldMedia.PhysicalBlockSizeSpecified) newMedia.PhysicalBlockSize     = oldMedia.PhysicalBlockSize;
            if(oldMedia.SolidStateDeviceSpecified) newMedia.SolidStateDevice       = oldMedia.SolidStateDevice;
            if(oldMedia.UnformattedBPTSpecified) newMedia.UnformattedBPT           = oldMedia.UnformattedBPT;
            if(oldMedia.UnformattedBPSSpecified) newMedia.UnformattedBPS           = oldMedia.UnformattedBPS;
            if(oldMedia.SupportsReadDmaLbaSpecified) newMedia.SupportsReadDmaLba   = oldMedia.SupportsReadDmaLba;
            if(oldMedia.SupportsReadDmaRetryLbaSpecified)
                newMedia.SupportsReadDmaRetryLba = oldMedia.SupportsReadDmaRetryLba;
            if(oldMedia.SupportsReadLbaSpecified) newMedia.SupportsReadLba           = oldMedia.SupportsReadLba;
            if(oldMedia.SupportsReadRetryLbaSpecified) newMedia.SupportsReadRetryLba = oldMedia.SupportsReadRetryLba;
            if(oldMedia.SupportsReadLongLbaSpecified) newMedia.SupportsReadLongLba   = oldMedia.SupportsReadLongLba;
            if(oldMedia.SupportsReadLongRetryLbaSpecified)
                newMedia.SupportsReadLongRetryLba = oldMedia.SupportsReadLongRetryLba;
            if(oldMedia.SupportsSeekLbaSpecified) newMedia.SupportsSeekLba             = oldMedia.SupportsSeekLba;
            if(oldMedia.SupportsReadDmaLba48Specified) newMedia.SupportsReadDmaLba48   = oldMedia.SupportsReadDmaLba48;
            if(oldMedia.SupportsReadLba48Specified) newMedia.SupportsReadLba48         = oldMedia.SupportsReadLba48;
            if(oldMedia.SupportsReadDmaSpecified) newMedia.SupportsReadDma             = oldMedia.SupportsReadDma;
            if(oldMedia.SupportsReadDmaRetrySpecified) newMedia.SupportsReadDmaRetry   = oldMedia.SupportsReadDmaRetry;
            if(oldMedia.SupportsReadRetrySpecified) newMedia.SupportsReadRetry         = oldMedia.SupportsReadRetry;
            if(oldMedia.SupportsReadLongRetrySpecified) newMedia.SupportsReadLongRetry = oldMedia.SupportsReadLongRetry;
            if(oldMedia.SupportsSeekSpecified) newMedia.SupportsSeek                   = oldMedia.SupportsSeek;

            if(oldMedia.CHS != null)
                newMedia.CHS = new CHS
                {
                    Cylinders = oldMedia.CHS.Cylinders,
                    Heads     = oldMedia.CHS.Heads,
                    Sectors   = oldMedia.CHS.Sectors
                };
            if(oldMedia.CurrentCHS != null)
                newMedia.CurrentCHS = new CHS
                {
                    Cylinders = oldMedia.CurrentCHS.Cylinders,
                    Heads     = oldMedia.CurrentCHS.Heads,
                    Sectors   = oldMedia.CurrentCHS.Sectors
                };

            return newMedia;
        }
    }
}