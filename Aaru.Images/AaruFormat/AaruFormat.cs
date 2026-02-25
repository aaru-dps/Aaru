using System;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

/// <inheritdoc cref="Aaru.CommonTypes.Interfaces.IWritableOpticalImage" />
/// <summary>Implements reading and writing AaruFormat media images</summary>
public sealed partial class AaruFormat : IWritableOpticalImage, IVerifiableImage, IWritableTapeImage, IDisposable
{
    const string MODULE_NAME = "Aaru Format plugin";
    IntPtr       _context;
    ImageInfo    _imageInfo;

    public AaruFormat() => _imageInfo = new ImageInfo
    {
        ReadableSectorTags    = [],
        ReadableMediaTags     = [],
        HasPartitions         = false,
        HasSessions           = false,
        Version               = null,
        Application           = "Aaru",
        ApplicationVersion    = null,
        Creator               = null,
        Comments              = null,
        MediaManufacturer     = null,
        MediaModel            = null,
        MediaSerialNumber     = null,
        MediaBarcode          = null,
        MediaPartNumber       = null,
        MediaSequence         = 0,
        LastMediaSequence     = 0,
        DriveManufacturer     = null,
        DriveModel            = null,
        DriveSerialNumber     = null,
        DriveFirmwareRevision = null
    };

    public uint    HeldDpmStartSector     { get; set; }
    public uint    HeldDpmResolution      { get; set; }
    public uint    HeldNumberOfDpmEntries { get; set; }
    public ulong[] HeldDpm                { get; set; }
}