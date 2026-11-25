namespace Aaru.Images;

public sealed partial class AaruFormat
{
#region Nested type: BlockType

    /// <summary>List of known blocks types (libaaruformat is the reference, not this)</summary>
    enum BlockType : uint
    {
        /// <summary>Block containing data</summary>
        DataBlock                    = 0x4B4C4244,
        /// <summary>Block containing a deduplication table</summary>
        DeDuplicationTable           = 0x2A544444,
        /// <summary>Block containing the index</summary>
        Index                        = 0x58444E49,
        /// <summary>Block containing the index</summary>
        Index2                       = 0x32584449,
        /// <summary>Block containing logical geometry</summary>
        GeometryBlock                = 0x4D4F4547,
        /// <summary>Block containing metadata</summary>
        MetadataBlock                = 0x4154454D,
        /// <summary>Block containing optical disc tracks</summary>
        TracksBlock                  = 0x534B5254,
        /// <summary>Block containing CICM XML metadata</summary>
        CicmBlock                    = 0x4D434943,
        /// <summary>Block containing contents checksums</summary>
        ChecksumBlock                = 0x4D534B43,
        /// <summary>Block containing data position measurements</summary>
        DataPositionMeasurementBlock = 0x2A4D5044,
        /// <summary>Block containing a snapshot index</summary>
        SnapshotBlock                = 0x50414E53,
        /// <summary>Block containing how to locate the parent image</summary>
        ParentBlock                  = 0x544E5250,
        /// <summary>Block containing an array of hardware used to create the image</summary>
        DumpHardwareBlock            = 0x2A504D44,
        /// <summary>Block containing list of files for a tape image</summary>
        TapeFileBlock                = 0x454C4654,
        /// <summary>Block containing list of partitions for a tape image</summary>
        TapePartitionBlock           = 0x54425054,
        /// <summary>Block containing list of indexes for Compact Disc tracks</summary>
        CompactDiscIndexesBlock      = 0x58494443,
        /// <summary>Block containing JSON version of Aaru Metadata</summary>
        AaruMetadataJsonBlock        = 0x444D534A,
        /// <summary>Block containing list of flux captures</summary>
        FluxCapturesBlock            = 0x58554C46
    }

#endregion

#region Nested type: Status

    /// <summary>
    ///     <para>Ok = 0</para>
    /// </summary>
    enum Status
    {
        /// <summary>
        ///     Ok.
        /// </summary>
        Ok                     = 0,
        /// <summary>
        ///     Sector has not been dumped.
        ///     &lt;remarks&gt;AARUF_STATUS_SECTOR_NOT_DUMPED&lt;/remarks&gt;
        /// </summary>
        SectorNotDumped        = 1,
        /// <summary>
        ///     Input file/stream failed magic or structural validation.
        ///     &lt;remarks&gt;AARUF_ERROR_NOT_AARUFORMAT&lt;/remarks&gt;
        /// </summary>
        NotAaruFormat          = -1,
        /// <summary>
        ///     File size insufficient for mandatory header / structures.
        ///     &lt;remarks&gt;AARUF_ERROR_FILE_TOO_SMALL&lt;/remarks&gt;
        /// </summary>
        FileTooSmall           = -2,
        /// <summary>
        ///     Image uses a newer incompatible on-disk version.
        ///     &lt;remarks&gt;AARUF_ERROR_INCOMPATIBLE_VERSION&lt;/remarks&gt;
        /// </summary>
        IncompatibleVersion    = -3,
        /// <summary>
        ///     Index block unreadable / truncated / bad identifier.
        ///     &lt;remarks&gt;AARUF_ERROR_CANNOT_READ_INDEX&lt;/remarks&gt;
        /// </summary>
        CannotReadIndex        = -4,
        /// <summary>
        ///     Requested logical sector outside media bounds.
        ///     &lt;remarks&gt;AARUF_ERROR_SECTOR_OUT_OF_BOUNDS&lt;/remarks&gt;
        /// </summary>
        SectorOutOfBounds      = -5,
        /// <summary>
        ///     Failed to read container header.
        ///     &lt;remarks&gt;AARUF_ERROR_CANNOT_READ_HEADER&lt;/remarks&gt;
        /// </summary>
        CannotReadHeader       = -6,
        /// <summary>
        ///     Generic block read failure (seek/read error).
        ///     &lt;remarks&gt;AARUF_ERROR_CANNOT_READ_BLOCK&lt;/remarks&gt;
        /// </summary>
        CannotReadBlock        = -7,
        /// <summary>
        ///     Block marked with unsupported compression algorithm.
        ///     &lt;remarks&gt;AARUF_ERROR_UNSUPPORTED_COMPRESSION&lt;/remarks&gt;
        /// </summary>
        UnsupportedCompression = -8,
        /// <summary>
        ///     Memory allocation failure (critical).
        ///     &lt;remarks&gt;AARUF_ERROR_NOT_ENOUGH_MEMORY&lt;/remarks&gt;
        /// </summary>
        NotEnoughMemory        = -9,
        /// <summary>
        ///     Caller-supplied buffer insufficient for data.
        ///     &lt;remarks&gt;AARUF_ERROR_BUFFER_TOO_SMALL&lt;/remarks&gt;
        /// </summary>
        BufferTooSmall         = -10,
        /// <summary>
        ///     Requested media tag absent.
        ///     &lt;remarks&gt;AARUF_ERROR_MEDIA_TAG_NOT_PRESENT&lt;/remarks&gt;
        /// </summary>
        MediaTagNotPresent     = -11,
        /// <summary>
        ///     Operation incompatible with image media type.
        ///     &lt;remarks&gt;AARUF_ERROR_INCORRECT_MEDIA_TYPE&lt;/remarks&gt;
        /// </summary>
        IncorrectMediaType     = -12,
        /// <summary>
        ///     Referenced track number not present.
        ///     &lt;remarks&gt;AARUF_ERROR_TRACK_NOT_FOUND&lt;/remarks&gt;
        /// </summary>
        TrackNotFound          = -13,
        /// <summary>
        ///     Internal logic assertion hit unexpected path.
        ///     &lt;remarks&gt;AARUF_ERROR_REACHED_UNREACHABLE_CODE&lt;/remarks&gt;
        /// </summary>
        ReachedUnreachableCode = -14,
        /// <summary>
        ///     Track metadata internally inconsistent or malformed.
        ///     &lt;remarks&gt;AARUF_ERROR_INVALID_TRACK_FORMAT&lt;/remarks&gt;
        /// </summary>
        InvalidTrackFormat     = -15,
        /// <summary>
        ///     Requested sector tag (e.g. subchannel/prefix) not stored.
        ///     &lt;remarks&gt;AARUF_ERROR_SECTOR_TAG_NOT_PRESENT&lt;/remarks&gt;
        /// </summary>
        SectorTagNotPresent    = -16,
        /// <summary>
        ///     Decompression routine failed or size mismatch.
        ///     &lt;remarks&gt;AARUF_ERROR_CANNOT_DECOMPRESS_BLOCK&lt;/remarks&gt;
        /// </summary>
        CannotDecompressBlock  = -17,
        /// <summary>
        ///     CRC64 mismatch indicating corruption.
        ///     &lt;remarks&gt;AARUF_ERROR_INVALID_BLOCK_CRC&lt;/remarks&gt;
        /// </summary>
        InvalidBlockCrc        = -18,
        /// <summary>
        ///     Output file could not be created / opened for write.
        ///     &lt;remarks&gt;AARUF_ERROR_CANNOT_CREATE_FILE&lt;/remarks&gt;
        /// </summary>
        CannotCreateFile       = -19,
        /// <summary>
        ///     Application name field length invalid (sanity limit).
        ///     &lt;remarks&gt;AARUF_ERROR_INVALID_APP_NAME_LENGTH&lt;/remarks&gt;
        /// </summary>
        InvalidAppNameLength   = -20,
        /// <summary>
        ///     Failure writing container header.
        ///     &lt;remarks&gt;AARUF_ERROR_CANNOT_WRITE_HEADER&lt;/remarks&gt;
        /// </summary>
        CannotWriteHeader      = -21,
        /// <summary>
        ///     Operation requires write mode but context is read-only.
        ///     &lt;remarks&gt;AARUF_READ_ONLY&lt;/remarks&gt;
        /// </summary>
        ReadOnly               = -22,
        /// <summary>
        ///     Failure writing block header.
        ///     &lt;remarks&gt;AARUF_ERROR_CANNOT_WRITE_BLOCK_HEADER&lt;/remarks&gt;
        /// </summary>
        CannotWriteBlockHeader = -23,
        /// <summary>
        ///     Failure writing block payload.
        ///     &lt;remarks&gt;AARUF_ERROR_CANNOT_WRITE_BLOCK_DATA&lt;/remarks&gt;
        /// </summary>
        CannotWriteBlockData   = -24,
        /// <summary>
        ///     Failed to encode/store a DDT entry (overflow or IO).
        ///     &lt;remarks&gt;AARUF_ERROR_CANNOT_SET_DDT_ENTRY&lt;/remarks&gt;
        /// </summary>
        CannotSetDdtEntry      = -25,
        /// <summary>
        ///     Data size does not match expected size.
        ///     &lt;remarks&gt;AARUF_ERROR_INCORRECT_DATA_SIZE&lt;/remarks&gt;
        /// </summary>
        IncorrectDataSize      = -26,
        /// <summary>
        ///     Invalid or unsupported media or sector tag format.
        ///     &lt;remarks&gt;AARUF_ERROR_INVALID_TAG&lt;/remarks&gt;
        /// </summary>
        InvalidTag             = -27,
        /// <summary>
        ///     Requested tape file number not present in image.
        ///     &lt;remarks&gt;AARUF_ERROR_TAPE_FILE_NOT_FOUND&lt;/remarks&gt;
        /// </summary>
        TapeFileNotFound       = -28,
        /// <summary>
        ///     Requested tape partition not present in image.
        ///     &lt;remarks&gt;AARUF_ERROR_TAPE_PARTITION_NOT_FOUND&lt;/remarks&gt;
        /// </summary>
        TapePartitionNotFound  = -29,
        /// <summary>
        ///     Requested metadata not present in image.
        ///     &lt;remarks&gt;AARUF_ERROR_METADATA_NOT_PRESENT&lt;/remarks&gt;
        /// </summary>
        MetadataNotPresent     = -30,
        ///     Sector length is too big.
        ///     &lt;remarks&gt;AARUF_ERROR_INVALID_SECTOR_LENGTH&lt;/remarks&gt;
        /// </summary>
        InvalidSectorLength    = -31,
        /// <summary>
        ///     Requested flux data not present in image.
        ///     &lt;remarks&gt;AARUF_ERROR_FLUX_DATA_NOT_FOUND&lt;/remarks&gt;
        /// </summary>
        FluxDataNotFound       = -32,
        /// <summary>
        ///     Image requires features not supported by this library.
        ///     &lt;remarks&gt;AARUF_ERROR_INCOMPATIBLE_FEATURES&lt;/remarks&gt;
        /// </summary>
        IncompatibleFeatures   = -33,
        /// <summary>
        ///     AES sector encryption failed.
        ///     &lt;remarks&gt;AARUF_ERROR_CANNOT_ENCRYPT_SECTOR&lt;/remarks&gt;
        /// </summary>
        CannotEncryptSector    = -34,
        /// <summary>
        ///     AES sector decryption failed.
        ///     &lt;remarks&gt;AARUF_ERROR_CANNOT_DECRYPT_SECTOR&lt;/remarks&gt;
        /// </summary>
        CannotDecryptSector    = -35,
        /// <summary>
        ///     Required encryption key not present in media tags.
        ///     &lt;remarks&gt;AARUF_ERROR_MISSING_ENCRYPTION_KEY&lt;/remarks&gt;
        /// </summary>
        MissingEncryptionKey   = -36,
        /// <summary>
        ///     Image has no user-data DDT (e.g. flux-only image); sector data is unavailable.
        ///     &lt;remarks&gt;AARUF_ERROR_USER_DATA_NOT_PRESENT&lt;/remarks&gt;
        /// </summary>
        UserDataNotPresent     = -37,
        /// <summary>
        ///     Requested twin instance does not exist (or would leave a gap).
        ///     &lt;remarks&gt;AARUF_ERROR_TWIN_INDEX_OUT_OF_RANGE&lt;/remarks&gt;
        /// </summary>
        TwinIndexOutOfRange    = -38,
    }

#endregion
}