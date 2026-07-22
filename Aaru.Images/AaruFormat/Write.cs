using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Aaru.Images;

public sealed partial class AaruFormat
{
    // AARU_EXPORT int32_t AARU_CALL aaruf_write_sector(void *context, uint64_t sector_address, bool negative,
    // const uint8_t *data, uint8_t sector_status, uint32_t length)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_write_sector", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_write_sector(IntPtr                             context, ulong sectorAddress,
                                                     [MarshalAs(UnmanagedType.I1)] bool negative, [In] byte[] data,
                                                     SectorStatus                       sectorStatus, uint length);

    // AARU_EXPORT int32_t AARU_CALL aaruf_write_sector_long(void *context, uint64_t sector_address, bool negative,
    // const uint8_t *data, uint8_t sector_status, uint32_t length)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_write_sector_long", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_write_sector_long(IntPtr context, ulong sectorAddress,
                                                          [MarshalAs(UnmanagedType.I1)] bool negative, [In] byte[] data,
                                                          SectorStatus sectorStatus, uint length);

    // AARU_EXPORT int32_t AARU_CALL aaruf_write_media_tag(void *context, const uint8_t *data, const int32_t type,
    // const uint32_t length)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_write_media_tag", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_write_media_tag(IntPtr context, [In] byte[] data, MediaTagType type,
                                                        uint   length);

    // AARU_EXPORT int32_t AARU_CALL aaruf_write_sector_tag(void *context, const uint64_t sector_address, const bool negative,
    // const uint8_t *data, const size_t length, const int32_t tag)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_write_sector_tag", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_write_sector_tag(IntPtr context, ulong sectorAddress,
                                                         [MarshalAs(UnmanagedType.I1)] bool negative, [In] byte[] data,
                                                         nuint length, SectorTagType tag);

    // AARU_EXPORT void AARU_CALL *aaruf_create(const char *filepath, const uint32_t media_type, const uint32_t sector_size,
    // const uint64_t user_sectors, const uint64_t negative_sectors,
    // const uint64_t overflow_sectors, const char *options,
    // const uint8_t *application_name, const uint8_t application_name_length,
    // const uint8_t application_major_version,
    // const uint8_t application_minor_version, const bool is_tape)
    [LibraryImport("libaaruformat",
                   EntryPoint = "aaruf_create",
                   SetLastError = true,
                   StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial IntPtr aaruf_create(string filepath, MediaType mediaType, uint sectorSize, ulong userSectors,
                                               ulong negativeSectors, ulong overflowSectors, string options,
                                               string applicationName, byte applicationNameLength,
                                               byte applicationMajorVersion, byte applicationMinorVersion,
                                               [MarshalAs(UnmanagedType.I1)] bool isTape);

#region IWritableOpticalImage Members

    /// <inheritdoc />
    public bool WriteDPM()
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSector(byte[] data, ulong sectorAddress, bool negative, SectorStatus sectorStatus)
    {
        Status res = aaruf_write_sector(_context, sectorAddress, negative, data, sectorStatus, (uint)data.Length);

        if(res == Status.Ok) return true;

        ErrorMessage = StatusToErrorMessage(res);

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress, bool negative, SectorStatus sectorStatus)
    {
        Status res = aaruf_write_sector_long(_context, sectorAddress, negative, data, sectorStatus, (uint)data.Length);

        if(res == Status.Ok) return true;

        ErrorMessage = StatusToErrorMessage(res);

        return false;
    }

    /// <inheritdoc />
    public bool WriteMediaTag(byte[] data, MediaTagType tag)
    {
        Status res = aaruf_write_media_tag(_context, data, tag, (uint)data.Length);

        if(res == Status.Ok) return true;

        ErrorMessage = StatusToErrorMessage(res);

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, bool negative, SectorTagType tag)
    {
        switch(tag)
        {
            // Due to how we cache the tracks, we need to handle ISRC tags ourselves
            case SectorTagType.CdTrackIsrc:
                _trackIsrcs                     ??= [];
                _trackIsrcs[(int)sectorAddress] =   Encoding.UTF8.GetString(data).TrimEnd('\0');

                return true;

            // Due to how we cache the tracks, we need to handle Track Flags ourselves
            case SectorTagType.CdTrackFlags:
            {
                _trackFlags ??= [];

                if(data.Length <= 0) return false;

                _trackFlags[(int)sectorAddress] = data[0];

                return true;
            }
        }

        Status res = aaruf_write_sector_tag(_context, sectorAddress, negative, data, (nuint)data.Length, tag);

        if(res == Status.Ok) return true;

        ErrorMessage = StatusToErrorMessage(res);

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, bool negative, uint length, SectorStatus[] sectorStatus)
    {
        var sectorSize = (uint)(data.Length / length);

        for(uint i = 0; i < length; i++)
        {
            var sectorData = new byte[sectorSize];
            Array.Copy(data, i * sectorSize, sectorData, 0, sectorSize);

            if(!WriteSector(sectorData, sectorAddress + i, negative, sectorStatus[i])) return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[]         data, ulong sectorAddress, bool negative, uint length,
                                 SectorStatus[] sectorStatus)
    {
        var sectorSize = (uint)(data.Length / length);

        for(uint i = 0; i < length; i++)
        {
            var sectorData = new byte[sectorSize];
            Array.Copy(data, i * sectorSize, sectorData, 0, sectorSize);

            if(!WriteSectorLong(sectorData, sectorAddress + i, negative, sectorStatus[i])) return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, bool negative, uint length, SectorTagType tag)
    {
        var sectorSize = (uint)(data.Length / length);

        for(uint i = 0; i < length; i++)
        {
            var sectorData = new byte[sectorSize];
            Array.Copy(data, i * sectorSize, sectorData, 0, sectorSize);

            if(!WriteSectorTag(sectorData, sectorAddress + i, negative, tag)) return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool Create(string path,            MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint   negativeSectors, uint      overflowSectors, uint sectorSize)
    {
        // Convert options dictionary to string format
        string optionsString = options is { Count: > 0 }
                                   ? string.Join(";", options.Select(static kvp => $"{kvp.Key}={kvp.Value}"))
                                   : null;

        // Create new image
        if(!File.Exists(path))
        {
            // Get application major and minor version
            Version version      = typeof(AaruFormat).Assembly.GetName().Version;
            var     majorVersion = (byte)(version?.Major ?? 0);
            var     minorVersion = (byte)(version?.Minor ?? 0);


            const string applicationName = "Aaru";

            // Create the image context
            _context = aaruf_create(path,
                                    mediaType,
                                    sectorSize,
                                    sectors,
                                    negativeSectors,
                                    overflowSectors,
                                    optionsString,
                                    applicationName,
                                    (byte)applicationName.Length,
                                    majorVersion,
                                    minorVersion,
                                    IsTape);

            if(_context != IntPtr.Zero) return true;

            int errno  = Marshal.GetLastWin32Error();
            var status = (Status)errno;
            ErrorMessage = StatusToErrorMessage(status);

            return false;
        }

        _context = aaruf_open(path, true, optionsString);

        if(_context == IntPtr.Zero)
        {
            int errno = Marshal.GetLastWin32Error();

            AaruLogging.Debug(MODULE_NAME,
                              "Failed to open AaruFormat image {0}, libaaruformat returned error number {1}",
                              path,
                              errno);

            return false;
        }

        AaruFormatImageInfo imageInfo = new();

        Status ret = aaruf_get_image_info(_context, ref imageInfo);

        if(ret != Status.Ok)
        {
            ErrorMessage = StatusToErrorMessage(ret);

            return false;
        }

        _imageInfo.Application          = StringHandlers.CToString(imageInfo.Application,        Encoding.UTF8);
        _imageInfo.Version              = StringHandlers.CToString(imageInfo.Version,            Encoding.UTF8);
        _imageInfo.ApplicationVersion   = StringHandlers.CToString(imageInfo.ApplicationVersion, Encoding.UTF8);
        _imageInfo.CreationTime         = DateTime.FromFileTimeUtc(imageInfo.CreationTime);
        _imageInfo.HasPartitions        = imageInfo.HasPartitions;
        _imageInfo.HasSessions          = imageInfo.HasSessions;
        _imageInfo.ImageSize            = imageInfo.ImageSize;
        _imageInfo.MediaType            = imageInfo.MediaType;
        _imageInfo.LastModificationTime = DateTime.FromFileTimeUtc(imageInfo.LastModificationTime);
        _imageInfo.SectorSize           = imageInfo.SectorSize;
        _imageInfo.Sectors              = imageInfo.Sectors;
        _imageInfo.MetadataMediaType    = imageInfo.MetadataMediaType;

        nuint sizet_length = 0;

        ret = aaruf_get_readable_sector_tags(_context, null, ref sizet_length);

        if(ret != Status.BufferTooSmall)
        {
            ErrorMessage = StatusToErrorMessage(ret);

            return false;
        }

        var sectorTagsBuffer = new byte[sizet_length];
        ret = aaruf_get_readable_sector_tags(_context, sectorTagsBuffer, ref sizet_length);

        if(ret != Status.Ok)
        {
            ErrorMessage = StatusToErrorMessage(ret);

            return false;
        }

        // Convert array of booleans to List of enums
        for(nuint i = 0; i < sizet_length; i++)
        {
            if(sectorTagsBuffer[i] != 0) _imageInfo.ReadableSectorTags.Add((SectorTagType)i);
        }

        sizet_length = 0;
        ret          = aaruf_get_readable_media_tags(_context, null, ref sizet_length);

        if(ret != Status.BufferTooSmall)
        {
            ErrorMessage = StatusToErrorMessage(ret);

            return false;
        }

        var mediaTagsBuffer = new byte[sizet_length];
        ret = aaruf_get_readable_media_tags(_context, mediaTagsBuffer, ref sizet_length);

        if(ret != Status.Ok)
        {
            ErrorMessage = StatusToErrorMessage(ret);

            return false;
        }

        // Convert array of booleans to List of enums
        for(nuint i = 0; i < sizet_length; i++)
        {
            if(mediaTagsBuffer[i] != 0) _imageInfo.ReadableMediaTags.Add((MediaTagType)i);
        }

        ret = aaruf_get_media_sequence(_context, out int sequence, out int lastSequence);

        if(ret == Status.Ok)
        {
            _imageInfo.LastMediaSequence = lastSequence;
            _imageInfo.MediaSequence     = sequence;
        }

        var length = 0;
        ret = aaruf_get_creator(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_creator(_context, buffer, ref length);
            if(ret == Status.Ok) _imageInfo.Creator = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_creator(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_creator(_context, buffer, ref length);
            if(ret == Status.Ok) _imageInfo.Creator = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_comments(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_comments(_context, buffer, ref length);
            if(ret == Status.Ok) _imageInfo.Comments = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_media_title(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_media_title(_context, buffer, ref length);
            if(ret == Status.Ok) _imageInfo.MediaTitle = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_media_manufacturer(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_media_manufacturer(_context, buffer, ref length);

            if(ret == Status.Ok)
                _imageInfo.MediaManufacturer = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_media_model(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_media_model(_context, buffer, ref length);
            if(ret == Status.Ok) _imageInfo.MediaModel = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_media_serial_number(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_media_serial_number(_context, buffer, ref length);

            if(ret == Status.Ok)
                _imageInfo.MediaSerialNumber = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_media_barcode(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_media_barcode(_context, buffer, ref length);
            if(ret == Status.Ok) _imageInfo.MediaBarcode = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_media_part_number(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_media_part_number(_context, buffer, ref length);
            if(ret == Status.Ok) _imageInfo.MediaPartNumber = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_drive_manufacturer(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_drive_manufacturer(_context, buffer, ref length);

            if(ret == Status.Ok)
                _imageInfo.DriveManufacturer = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_drive_model(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_drive_model(_context, buffer, ref length);
            if(ret == Status.Ok) _imageInfo.DriveModel = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_drive_serial_number(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_drive_serial_number(_context, buffer, ref length);

            if(ret == Status.Ok)
                _imageInfo.DriveSerialNumber = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        length = 0;
        ret    = aaruf_get_drive_firmware_revision(_context, null, ref length);

        if(ret == Status.BufferTooSmall)
        {
            var buffer = new byte[length];
            ret = aaruf_get_drive_firmware_revision(_context, buffer, ref length);

            if(ret == Status.Ok)
                _imageInfo.DriveFirmwareRevision = StringHandlers.CToString(buffer, Encoding.Unicode, true);
        }

        ret = aaruf_get_geometry(_context, out uint cylinders, out uint heads, out uint sectorsPerTrack);

        if(ret == Status.Ok)
        {
            _imageInfo.Cylinders       = cylinders;
            _imageInfo.Heads           = heads;
            _imageInfo.SectorsPerTrack = sectorsPerTrack;
        }

        SetMetadataFromTags();

        uint infoNegativeSectors = 0;

        ret = aaruf_get_negative_sectors(_context, ref infoNegativeSectors);
        if(ret == Status.Ok) _imageInfo.NegativeSectors = infoNegativeSectors;

        uint infoOverflowSectors = 0;

        ret = aaruf_get_overflow_sectors(_context, ref infoOverflowSectors);
        if(ret == Status.Ok) _imageInfo.OverflowSectors = infoOverflowSectors;

        return true;
    }

#endregion
}