// ReSharper disable InconsistentNaming

namespace Aaru.Filesystems;

/// <summary>Extended attribute name constants used across filesystems</summary>
static class Xattrs
{
    /// <summary>Extended attribute name for Acorn RISC OS filetype</summary>
    public const string XATTR_ACORN_RISCOS_FILETYPE = "riscos.type";

    /// <summary>Extended attribute name for Acorn RISC OS full 32-bit file attributes (ADFS-G/big directories)</summary>
    public const string XATTR_ACORN_RISCOS_ATTRIBUTES = "riscos.attr";

    /// <summary>Extended attribute name for Acorn RISC OS load address</summary>
    public const string XATTR_ACORN_RISCOS_LOAD_ADDR = "riscos.loadaddr";

    /// <summary>Extended attribute name for Acorn RISC OS exec address</summary>
    public const string XATTR_ACORN_RISCOS_EXEC_ADDR = "riscos.execaddr";

    /// <summary>Extended attribute name for the Amiga file comment</summary>
    public const string XATTR_AMIGA_COMMENTS = "amiga.comments";

    /// <summary>
    ///     Extended attribute name for the 32 Amiga protection bits, as the big endian 32-bit value AmigaOS uses
    /// </summary>
    public const string XATTR_AMIGA_PROTECTION = "amiga.protection";

    /// <summary>Extended attribute name for the POSIX access ACL, in the binary format of Linux</summary>
    public const string XATTR_POSIX_ACL_ACCESS = "system.posix_acl_access";

    /// <summary>Extended attribute name for the POSIX default ACL of a directory, in the binary format of Linux</summary>
    public const string XATTR_POSIX_ACL_DEFAULT = "system.posix_acl_default";

    /// <summary>Extended attribute name for Apple DOS track/sector list</summary>
    public const string XATTR_APPLE_DOS_TRACK_SECTOR_LIST = "com.apple.dos.tracksectorlist";

    /// <summary>Extended attribute name for Apple DOS file type</summary>
    public const string XATTR_APPLE_DOS_TYPE = "com.apple.dos.type";

    /// <summary>Extended attribute name for Apple Finder information</summary>
    public const string XATTR_APPLE_FINDER_INFO = "com.apple.FinderInfo";

    /// <summary>Extended attribute name for HFS creator code</summary>
    public const string XATTR_APPLE_HFS_CREATOR = "hfs.creator";

    /// <summary>Extended attribute name for HFS type code (OSType)</summary>
    public const string XATTR_APPLE_HFS_OSTYPE = "hfs.type";

    /// <summary>Extended attribute name for Macintosh custom icon</summary>
    public const string XATTR_APPLE_ICON = "com.apple.Macintosh.Icon";

    /// <summary>Extended attribute name for Apple Lisa label</summary>
    public const string XATTR_APPLE_LISA_LABEL = "com.apple.lisa.label";

    /// <summary>Extended attribute name for Apple Lisa password</summary>
    public const string XATTR_APPLE_LISA_PASSWORD = "com.apple.lisa.password";

    /// <summary>Extended attribute name for Apple Lisa serial number</summary>
    public const string XATTR_APPLE_LISA_SERIAL = "com.apple.lisa.serial";

    /// <summary>Extended attribute name for Apple Lisa tags</summary>
    public const string XATTR_APPLE_LISA_TAGS = "com.apple.lisa.tags";

    /// <summary>Extended attribute name for ProDOS file type</summary>
    public const string XATTR_APPLE_PRODOS_TYPE = "prodos.type";

    /// <summary>Extended attribute name for ProDOS auxiliary type</summary>
    public const string XATTR_APPLE_PRODOS_AUX_TYPE = "prodos.aux_type";

    /// <summary>Extended attribute name for Macintosh resource fork</summary>
    public const string XATTR_APPLE_RESOURCE_FORK = "com.apple.ResourceFork";

    /// <summary>Extended attribute name for BeOS old filesystem file type (predates MIME type)</summary>
    public const string XATTR_BE_OFS_FILETYPE = "com.be.filetype";

    /// <summary>Extended attribute name for ECMA-167 alternate permissions</summary>
    public const string XATTR_ECMA_ALTERNATE_PERMISSIONS = "ch.ecma.alternate_permissions";

    /// <summary>Extended attribute name for ECMA-167 character set information</summary>
    public const string XATTR_ECMA_CHARSET_INFO = "ch.ecma.charset_info";

    /// <summary>Extended attribute name for ECMA-167 device specification</summary>
    public const string XATTR_ECMA_DEVICE_SPECIFICATION = "ch.ecma.device_specification";

    /// <summary>Extended attribute name for ECMA-167 file timestamps</summary>
    public const string XATTR_ECMA_FILE_TIMES = "ch.ecma.file_times";

    /// <summary>Extended attribute name for ECMA-167 information timestamps</summary>
    public const string XATTR_ECMA_INFO_TIMES = "ch.ecma.info_times";

    /// <summary>Extended attribute name for IBM OS/400 directory information</summary>
    public const string XATTR_IBM_OS400_DIR_INFO = "com.ibm.os400.dirinfo";

    /// <summary>Extended attribute name for ISO 9660 associated file</summary>
    public const string XATTR_ISO9660_ASSOCIATED_FILE = "org.iso.9660.AssociatedFile";

    /// <summary>Extended attribute name for ISO 9660 extended attributes</summary>
    public const string XATTR_ISO9660_EA = "org.iso.9660.ea";

    /// <summary>Extended attribute name for ISO 9660 Mode 2 subheader</summary>
    public const string XATTR_ISO9660_MODE2_SUBHEADER = "org.iso.mode2.subheader";

    /// <summary>Extended attribute name for ISO 9660 Mode 2 subheader copy</summary>
    public const string XATTR_ISO9660_MODE2_SUBHEADER_COPY = "org.iso.mode2.subheader.copy";

    /// <summary>Extended attribute name for UDF DVD CGMS copy protection information</summary>
    public const string XATTR_UDF_DVD_CGMS = "org.osta.udf.dvd_cgms_info";

    /// <summary>Extended attribute name for UDF free extended attribute space</summary>
    public const string XATTR_UDF_FREE_EA_SPACE = "org.osta.udf.free_ea_space";

    /// <summary>Extended attribute name for VMS/ODS Access Control List</summary>
    public const string XATTR_VMS_ACL = "com.vms.acl";

    /// <summary>Extended attribute name for VMS/ODS File Attributes (FAT) record</summary>
    public const string XATTR_VMS_FAT = "com.vms.fat";

    /// <summary>Extended attribute name for VMS/ODS file organization</summary>
    public const string XATTR_VMS_ORGANIZATION = "com.vms.organization";

    /// <summary>Extended attribute name for OS/2 Workplace Shell class information</summary>
    public const string XATTR_WORKPLACE_CLASSINFO = "com.ibm.os2.classinfo";
}