// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ErrorNumber.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common types.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines enumerations of error numbers.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.CommonTypes.Enums;

/// <summary>Enumerates error codes. Negative for UNIX error number equivalents, positive for Aaru error numbers.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public enum ErrorNumber
{
    /// <summary>Operation not permitted</summary>
    NotPermitted = -1,
    /// <summary>No such file or directory</summary>
    NoSuchFile = -2,
    /// <summary>No such process</summary>
    NoSuchProcess = -3,
    /// <summary>System call interrupted</summary>
    InterruptedSyscall = -4,
    /// <summary>I/O error</summary>
    InOutError = -5,
    /// <summary>No such device or address</summary>
    NoSuchDeviceOrAddress = -6,
    /// <summary>The argument list is too long</summary>
    ArgumentListTooLong = -7,
    /// <summary>Error loading the executable format</summary>
    ExecutableFormatError = -8,
    /// <summary>Bad file descriptor</summary>
    BadFileNumber = -9,
    /// <summary>No child process</summary>
    NoChildProcess = -10,
    /// <summary>Try again</summary>
    TryAgain = -11,
    /// <summary>Process ran out of memory</summary>
    OutOfMemory = -12,
    /// <summary>Access denied</summary>
    AccessDenied = -13,
    /// <summary>Bad address</summary>
    BadAddress = -14,
    /// <summary>File is not a block device</summary>
    NotABlockDevice = -15,
    /// <summary>Busy, cannot complete</summary>
    Busy = -16,
    /// <summary>File already exists</summary>
    FileExists = -17,
    /// <summary>Tried to create a link that crosses devices</summary>
    CrossDeviceLink = -18,
    /// <summary>No such device</summary>
    NoSuchDevice = -19,
    /// <summary>Is not a directory (e.g.: trying to ReadDir() a file)</summary>
    NotDirectory = -20,
    /// <summary>Is a directory (e.g.: trying to Read() a dir)</summary>
    IsDirectory = -21,
    /// <summary>Invalid argument</summary>
    InvalidArgument = -22,
    /// <summary>File table overflow</summary>
    FileTableOverflow = -23,
    /// <summary>Too many open files</summary>
    TooManyOpenFiles = -24,
    /// <summary>Destination is not a TTY</summary>
    NotTypewriter = -25,
    /// <summary>Text file busy</summary>
    TextFileBusy = -26,
    /// <summary>File is too large</summary>
    FileTooLarge = -27,
    /// <summary>No space left on volume</summary>
    NoSpaceLeft = -28,
    /// <summary>Illegal seek requested</summary>
    IllegalSeek = -29,
    /// <summary>Tried a write operation on a read-only device</summary>
    ReadOnly = -30,
    /// <summary>Too many links</summary>
    TooManyLinks = -31,
    /// <summary>Broken pipe</summary>
    BrokenPipe = -32,
    /// <summary>Out of domain</summary>
    OutOfDomain = -33,
    /// <summary>Out of range</summary>
    OutOfRange = -34,
    /// <summary>Operation would incur in a deadlock</summary>
    DeadlockWouldOccur = -35,
    /// <summary>Name is too long</summary>
    NameTooLong = -36,
    /// <summary>There are no available locks</summary>
    NoLocksAvailable = -37,
    /// <summary>Not implemented</summary>
    NotImplemented = -38,
    /// <summary>There is no data available</summary>
    NoData = -61,
    /// <summary>Link is severed</summary>
    SeveredLink = -67,
    /// <summary>There is no such attribute</summary>
    NoSuchExtendedAttribute = NoData,
    /// <summary>Not supported</summary>
    NotSupported = -252,
    /// <summary>Operation not permitted</summary>
    EPERM = NotPermitted,
    /// <summary>No such file or directory</summary>
    ENOENT = NoSuchFile,
    /// <summary>No such process</summary>
    ESRCH = NoSuchProcess,
    /// <summary>System call interrupted</summary>
    EINTR = InterruptedSyscall,
    /// <summary>I/O error</summary>
    EIO = InOutError,
    /// <summary>No such device or address</summary>
    ENXIO = NoSuchDeviceOrAddress,
    /// <summary>The argument list is too long</summary>
    E2BIG = ArgumentListTooLong,
    /// <summary>Error loading the executable format</summary>
    ENOEXEC = ExecutableFormatError,
    /// <summary>Bad file descriptor</summary>
    EBADF = BadFileNumber,
    /// <summary>No child process</summary>
    ECHILD = NoChildProcess,
    /// <summary>Try again</summary>
    EAGAIN = TryAgain,
    /// <summary>Process ran out of memory</summary>
    ENOMEM = OutOfMemory,
    /// <summary>Access denied</summary>
    EACCES = AccessDenied,
    /// <summary>Bad address</summary>
    EFAULT = BadAddress,
    /// <summary>File is not a block device</summary>
    ENOTBLK = NotABlockDevice,
    /// <summary>Busy, cannot complete</summary>
    EBUSY = Busy,
    /// <summary>File already exists</summary>
    EEXIST = FileExists,
    /// <summary>Tried to create a link that crosses devices</summary>
    EXDEV = CrossDeviceLink,
    /// <summary>No such device</summary>
    ENODEV = NoSuchDevice,
    /// <summary>Is not a directory (e.g.: trying to ReadDir() a file)</summary>
    ENOTDIR = NotDirectory,
    /// <summary>Is a directory (e.g.: trying to Read() a dir)</summary>
    EISDIR = IsDirectory,
    /// <summary>Invalid argument</summary>
    EINVAL = InvalidArgument,
    /// <summary>File table overflow</summary>
    ENFILE = FileTableOverflow,
    /// <summary>Too many open files</summary>
    EMFILE = TooManyOpenFiles,
    /// <summary>Destination is not a TTY</summary>
    ENOTTY = NotTypewriter,
    /// <summary>Text file busy</summary>
    ETXTBSY = TextFileBusy,
    /// <summary>File is too large</summary>
    EFBIG = FileTooLarge,
    /// <summary>No space left on volume</summary>
    ENOSPC = NoSpaceLeft,
    /// <summary>Illegal seek requested</summary>
    ESPIPE = IllegalSeek,
    /// <summary>Tried a write operation on a read-only device</summary>
    EROFS = ReadOnly,
    /// <summary>Too many links</summary>
    EMLINK = TooManyLinks,
    /// <summary>Broken pipe</summary>
    EPIPE = BrokenPipe,
    /// <summary>Out of domain</summary>
    EDOM = OutOfDomain,
    /// <summary>Out of range</summary>
    ERANGE = OutOfRange,
    /// <summary>Operation would incur in a deadlock</summary>
    EDEADLK = DeadlockWouldOccur,
    /// <summary>Name is too long</summary>
    ENAMETOOLONG = NameTooLong,
    /// <summary>There are no available locks</summary>
    ENOLCK = NoLocksAvailable,
    /// <summary>Not implemented</summary>
    ENOSYS = NotImplemented,
    /// <summary>Link is severed</summary>
    ENOLINK = SeveredLink,
    /// <summary>Not supported</summary>
    ENOTSUP = NotSupported,
    /// <summary>Directory is not empty</summary>
    DirectoryNotEmpty = -39,
    /// <summary>Too many symbolic links encountered</summary>
    TooManySymbolicLinks = -40,
    /// <summary>Directory is not empty</summary>
    ENOTEMPTY = DirectoryNotEmpty,
    /// <summary>Too many symbolic links encountered</summary>
    ELOOP = TooManySymbolicLinks,
    /// <summary>There is no such attribute</summary>
    ENOATTR = NoSuchExtendedAttribute,
    /// <summary>There is no data available</summary>
    ENODATA = NoData,
    /// <summary>No error</summary>
    NoError = 0,
    /// <summary>User requested help to be shown</summary>
    HelpRequested = 1,
    /// <summary>Command found nothing</summary>
    NothingFound = 2,
    /// <summary>Media has been already dumped completely</summary>
    AlreadyDumped = 3,
    /// <summary>Image and its sectors cannot be verified</summary>
    NotVerifiable = 4,
    /// <summary>There are bad sectors and image cannot be verified</summary>
    BadSectorsImageNotVerified = 5,
    /// <summary>All sectors are good and image cannot be verified</summary>
    CorrectSectorsImageNotVerified = 6,
    /// <summary>Image is bad and sectors cannot be verified</summary>
    BadImageSectorsNotVerified = 7,
    /// <summary>Image is bad and there are bad sectors</summary>
    BadImageBadSectors = 8,
    /// <summary>All sectors are good and image is bad</summary>
    CorrectSectorsBadImage = 9,
    /// <summary>Image is good and sectors cannot be verified</summary>
    CorrectImageSectorsNotVerified = 10,
    /// <summary>Image is good and there are bad sectors</summary>
    CorrectImageBadSectors = 11,
    /// <summary>Exception has been raised</summary>
    UnexpectedException = 12,
    /// <summary>The number of arguments is not as expected</summary>
    UnexpectedArgumentCount = 13,
    /// <summary>A required argument is not present</summary>
    MissingArgument = 14,
    /// <summary>The specified file cannot be opened</summary>
    CannotOpenFile = 15,
    /// <summary>The specified encoding cannot be found</summary>
    EncodingUnknown = 16,
    /// <summary>The image format has not been recognized</summary>
    UnrecognizedFormat = 17,
    /// <summary>The image format failed to open</summary>
    CannotOpenFormat = 18,
    /// <summary>The specified metadata sidecar does not have the correct format</summary>
    InvalidSidecar = 19,
    /// <summary>The specified resume map does not have the correct format</summary>
    InvalidResume = 20,
    /// <summary>The specified image format cannot be found</summary>
    FormatNotFound = 21,
    /// <summary>More than one format found for the specified search criteria</summary>
    TooManyFormats = 22,
    /// <summary>The specified format does not support the specified media</summary>
    UnsupportedMedia = 23,
    /// <summary>Data will be lost writing the specified format</summary>
    DataWillBeLost = 24,
    /// <summary>Cannot create destination format</summary>
    CannotCreateFormat = 25,
    /// <summary>Error writing data</summary>
    WriteError = 26,
    /// <summary>Cannot open device</summary>
    CannotOpenDevice = 27,
    /// <summary>Cannot remove the existing database</summary>
    CannotRemoveDatabase = 28,
    /// <summary>Specified sector could not be found</summary>
    SectorNotFound = 29,
    /// <summary>Image or device has not been opened</summary>
    NotOpened = 30
}