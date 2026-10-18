using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SecureDigital;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class AaruFormat
{
    /// <summary>
    ///     Converts an AaruFormat.Status to Aaru.CommonTypes.Enums.ErrorNumber.
    /// </summary>
    /// <param name="status">The AaruFormat status to convert</param>
    /// <returns>The corresponding ErrorNumber</returns>
    static ErrorNumber StatusToErrorNumber(Status status)
    {
        return status switch
               {
                   Status.Ok                     => ErrorNumber.NoError,
                   Status.NotAaruFormat          => ErrorNumber.InvalidArgument,
                   Status.FileTooSmall           => ErrorNumber.InvalidArgument,
                   Status.IncompatibleVersion    => ErrorNumber.NotSupported,
                   Status.CannotReadIndex        => ErrorNumber.InOutError,
                   Status.SectorOutOfBounds      => ErrorNumber.OutOfRange,
                   Status.CannotReadHeader       => ErrorNumber.InOutError,
                   Status.CannotReadBlock        => ErrorNumber.InOutError,
                   Status.UnsupportedCompression => ErrorNumber.NotSupported,
                   Status.NotEnoughMemory        => ErrorNumber.OutOfMemory,
                   Status.BufferTooSmall         => ErrorNumber.InvalidArgument,
                   Status.MediaTagNotPresent     => ErrorNumber.NoData,
                   Status.IncorrectMediaType     => ErrorNumber.InvalidArgument,
                   Status.TrackNotFound          => ErrorNumber.SectorNotFound,
                   Status.ReachedUnreachableCode => ErrorNumber.InvalidArgument,
                   Status.InvalidTrackFormat     => ErrorNumber.InvalidArgument,
                   Status.SectorTagNotPresent    => ErrorNumber.NoData,
                   Status.CannotDecompressBlock  => ErrorNumber.InOutError,
                   Status.InvalidBlockCrc        => ErrorNumber.InOutError,
                   Status.CannotCreateFile       => ErrorNumber.InvalidArgument,
                   Status.InvalidAppNameLength   => ErrorNumber.InvalidArgument,
                   Status.CannotWriteHeader      => ErrorNumber.InOutError,
                   Status.ReadOnly               => ErrorNumber.ReadOnly,
                   Status.CannotWriteBlockHeader => ErrorNumber.InOutError,
                   Status.CannotWriteBlockData   => ErrorNumber.InOutError,
                   Status.CannotSetDdtEntry      => ErrorNumber.InOutError,
                   Status.IncorrectDataSize      => ErrorNumber.InvalidArgument,
                   Status.InvalidTag             => ErrorNumber.InvalidArgument,
                   Status.TapeFileNotFound       => ErrorNumber.NoData,
                   Status.TapePartitionNotFound  => ErrorNumber.NoData,
                   Status.MetadataNotPresent     => ErrorNumber.NoData,
                   Status.InvalidSectorLength    => ErrorNumber.InvalidArgument,
                   _                             => ErrorNumber.InvalidArgument
               };
    }

    /// <summary>
    ///     Converts an AaruFormat.Status to a descriptive error message.
    /// </summary>
    /// <param name="status">The AaruFormat status to convert</param>
    /// <returns>A descriptive error message string</returns>
    static string StatusToErrorMessage(Status status)
    {
        return status switch
               {
                   Status.Ok                     => "Operation completed successfully.",
                   Status.NotAaruFormat          => "Input file or stream failed magic or structural validation.",
                   Status.FileTooSmall           => "File size is insufficient for mandatory header or structures.",
                   Status.IncompatibleVersion    => "Image uses a newer incompatible on-disk version.",
                   Status.CannotReadIndex        => "Index block is unreadable, truncated, or has bad identifier.",
                   Status.SectorOutOfBounds      => "Requested logical sector is outside media bounds.",
                   Status.CannotReadHeader       => "Failed to read container header.",
                   Status.CannotReadBlock        => "Generic block read failure (seek or read error).",
                   Status.UnsupportedCompression => "Block is marked with unsupported compression algorithm.",
                   Status.NotEnoughMemory        => "Memory allocation failure.",
                   Status.BufferTooSmall         => "Caller-supplied buffer is insufficient for data.",
                   Status.MediaTagNotPresent     => "Requested media tag is absent.",
                   Status.IncorrectMediaType     => "Operation is incompatible with image media type.",
                   Status.TrackNotFound          => "Referenced track number is not present.",
                   Status.ReachedUnreachableCode => "Internal logic assertion hit unexpected path.",
                   Status.InvalidTrackFormat     => "Track metadata is internally inconsistent or malformed.",
                   Status.SectorTagNotPresent    => "Requested sector tag (e.g., subchannel or prefix) is not stored.",
                   Status.CannotDecompressBlock  => "Decompression routine failed or size mismatch occurred.",
                   Status.InvalidBlockCrc        => "CRC64 mismatch indicating corruption.",
                   Status.CannotCreateFile       => "Output file could not be created or opened for write.",
                   Status.InvalidAppNameLength   => "Application name field length is invalid (sanity limit exceeded).",
                   Status.CannotWriteHeader      => "Failure writing container header.",
                   Status.ReadOnly               => "Operation requires write mode but context is read-only.",
                   Status.CannotWriteBlockHeader => "Failure writing block header.",
                   Status.CannotWriteBlockData   => "Failure writing block payload.",
                   Status.CannotSetDdtEntry      => "Failed to encode or store a DDT entry (overflow or I/O error).",
                   Status.IncorrectDataSize      => "Data size does not match expected size.",
                   Status.InvalidTag             => "Invalid or unsupported media or sector tag format.",
                   Status.TapeFileNotFound       => "Requested tape file number is not present in image.",
                   Status.TapePartitionNotFound  => "Requested tape partition is not present in image.",
                   Status.MetadataNotPresent     => "Requested metadata is not present in image.",
                   Status.InvalidSectorLength    => "Requested sector length is too big.",
                   Status.FluxDataNotFound       => "Requested flux data is not present in image.",
                   _                             => "Unknown error occurred."
               };
    }

    /// <summary>Checks for media tags that may contain metadata and sets it up if not already set</summary>
    void SetMetadataFromTags()
    {
        // Search for SecureDigital CID
        uint   length = 0;
        Status res    = aaruf_read_media_tag(_context, null, MediaTagType.SD_CID, ref length);

        if(res == Status.BufferTooSmall)
        {
            var sdCid = new byte[length];

            res = aaruf_read_media_tag(_context, sdCid, MediaTagType.SD_CID, ref length);

            if(res == Status.Ok)
            {
                CID decoded = Decoders.SecureDigital.Decoders.DecodeCID(sdCid);

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveManufacturer))
                    _imageInfo.DriveManufacturer = VendorString.Prettify(decoded.Manufacturer);

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel)) _imageInfo.DriveModel = decoded.ProductName;

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveFirmwareRevision))
                {
                    _imageInfo.DriveFirmwareRevision =
                        $"{(decoded.ProductRevision & 0xF0) >> 4:X2}.{decoded.ProductRevision & 0x0F:X2}";
                }

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveSerialNumber))
                    _imageInfo.DriveSerialNumber = $"{decoded.ProductSerialNumber}";
            }
        }

        // Search for MultiMediaCard CID
        length = 0;
        res    = aaruf_read_media_tag(_context, null, MediaTagType.SD_CID, ref length);

        if(res == Status.BufferTooSmall)
        {
            var mmcCid = new byte[length];

            res = aaruf_read_media_tag(_context, mmcCid, MediaTagType.SD_CID, ref length);

            if(res == Status.Ok)
            {
                Decoders.MMC.CID decoded = Decoders.MMC.Decoders.DecodeCID(mmcCid);

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveManufacturer))
                    _imageInfo.DriveManufacturer = Decoders.MMC.VendorString.Prettify(decoded.Manufacturer);

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel)) _imageInfo.DriveModel = decoded.ProductName;

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveFirmwareRevision))
                {
                    _imageInfo.DriveFirmwareRevision =
                        $"{(decoded.ProductRevision & 0xF0) >> 4:X2}.{decoded.ProductRevision & 0x0F:X2}";
                }

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveSerialNumber))
                    _imageInfo.DriveSerialNumber = $"{decoded.ProductSerialNumber}";
            }
        }

        // Search for SCSI INQUIRY
        length = 0;
        res    = aaruf_read_media_tag(_context, null, MediaTagType.SCSI_INQUIRY, ref length);

        if(res == Status.BufferTooSmall)
        {
            var scsiInquiry = new byte[length];

            res = aaruf_read_media_tag(_context, scsiInquiry, MediaTagType.SD_CID, ref length);

            if(res == Status.Ok)
            {
                Inquiry? nullableInquiry = Inquiry.Decode(scsiInquiry);

                if(nullableInquiry.HasValue)
                {
                    Inquiry inquiry = nullableInquiry.Value;

                    if(string.IsNullOrWhiteSpace(_imageInfo.DriveManufacturer))
                        _imageInfo.DriveManufacturer = StringHandlers.CToString(inquiry.VendorIdentification)?.Trim();

                    if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel))
                        _imageInfo.DriveModel = StringHandlers.CToString(inquiry.ProductIdentification)?.Trim();

                    if(string.IsNullOrWhiteSpace(_imageInfo.DriveFirmwareRevision))
                    {
                        _imageInfo.DriveFirmwareRevision =
                            StringHandlers.CToString(inquiry.ProductRevisionLevel)?.Trim();
                    }
                }
            }
        }

        // Search for ATA IDENTIFY
        length = 0;
        res    = aaruf_read_media_tag(_context, null, MediaTagType.ATA_IDENTIFY, ref length);

        if(res == Status.BufferTooSmall)
        {
            var ataIdentify = new byte[length];

            res = aaruf_read_media_tag(_context, ataIdentify, MediaTagType.ATA_IDENTIFY, ref length);

            if(res == Status.Ok)
            {
                Identify.IdentifyDevice? nullableIdentify =
                    CommonTypes.Structs.Devices.ATA.Identify.Decode(ataIdentify);

                if(!nullableIdentify.HasValue) return;

                Identify.IdentifyDevice identify = nullableIdentify.Value;

                string[] separated = identify.Model.Split(' ');

                if(separated.Length == 1)
                {
                    if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel))
                        _imageInfo.DriveModel = separated[0];
                    else
                    {
                        if(string.IsNullOrWhiteSpace(_imageInfo.DriveManufacturer))
                            _imageInfo.DriveManufacturer = separated[0];

                        if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel)) _imageInfo.DriveModel = separated[^1];
                    }
                }

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveFirmwareRevision))
                    _imageInfo.DriveFirmwareRevision = identify.FirmwareRevision;

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveSerialNumber))
                    _imageInfo.DriveSerialNumber = identify.SerialNumber;
            }
        }

        // Search for ATAPI IDENTIFY
        length = 0;
        res    = aaruf_read_media_tag(_context, null, MediaTagType.ATAPI_IDENTIFY, ref length);

        if(res != Status.BufferTooSmall) return;

        var atapiIdentify = new byte[length];

        res = aaruf_read_media_tag(_context, atapiIdentify, MediaTagType.ATAPI_IDENTIFY, ref length);

        if(res != Status.Ok) return;

        {
            Identify.IdentifyDevice? nullableIdentify = CommonTypes.Structs.Devices.ATA.Identify.Decode(atapiIdentify);

            if(!nullableIdentify.HasValue) return;

            Identify.IdentifyDevice identify = nullableIdentify.Value;

            string[] separated = identify.Model.Split(' ');

            if(separated.Length == 1)
            {
                if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel))
                    _imageInfo.DriveModel = separated[0];
                else
                {
                    if(string.IsNullOrWhiteSpace(_imageInfo.DriveManufacturer))
                        _imageInfo.DriveManufacturer = separated[0];

                    if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel)) _imageInfo.DriveModel = separated[^1];
                }
            }

            if(string.IsNullOrWhiteSpace(_imageInfo.DriveFirmwareRevision))
                _imageInfo.DriveFirmwareRevision = identify.FirmwareRevision;

            if(string.IsNullOrWhiteSpace(_imageInfo.DriveSerialNumber))
                _imageInfo.DriveSerialNumber = identify.SerialNumber;
        }
    }
}