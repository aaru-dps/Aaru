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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

namespace Aaru.CommonTypes.Enums
{
    /// <summary>Enumerates error codes. Negative for UNIX error number equivalents, positive for Aaru error numbers.</summary>
    public enum ErrorNumber
    {
        NotPermitted = -1,
        /// <summary>No such file or directory</summary>
        NoSuchFile = -2, NoSuchProcess = -3, InterruptedSyscall = -4,
        /// <summary>I/O error</summary>
        InOutError = -5, NoSuchDeviceOrAddress = -6, ArgumentListTooLong = -7, ExecutableFormatError = -8,
        BadFileNumber                          = -9, NoChildProcess      = -10, TryAgain             = -11,
        OutOfMemory                            = -12,
        /// <summary>Access denied</summary>
        AccessDenied = -13, BadAddress = -14, NotABlockDevice = -15,
        /// <summary>Busy, cannot complete</summary>
        Busy = -16, FileExists = -17, CrossDeviceLink = -18,
        /// <summary>No such device</summary>
        NoSuchDevice = -19,
        /// <summary>Is not a directory (e.g.: trying to ReadDir() a file)</summary>
        NotDirectory = -20,
        /// <summary>Is a directory (e.g.: trying to Read() a dir)</summary>
        IsDirectory = -21,
        /// <summary>Invalid argument</summary>
        InvalidArgument = -22, FileTableOverflow = -23, TooManyOpenFiles = -24, NotTypewriter = -25,
        TextFileBusy                             = -26,
        /// <summary>File is too large</summary>
        FileTooLarge = -27, NoSpaceLeft = -28, IllegalSeek        = -29, ReadOnly    = -30,
        TooManyLinks                    = -31, BrokenPipe         = -32, OutOfDomain = -33,
        OutOfRange                      = -34, DeadlockWouldOccur = -35,
        /// <summary>Name is too long</summary>
        NameTooLong = -36, NoLocksAvailable = -37,
        /// <summary>Not implemented</summary>
        NotImplemented = -38,
        /// <summary>There is no data available</summary>
        NoData = -61,
        /// <summary>Link is severed</summary>
        SeveredLink = -67,
        /// <summary>There is no such attribute</summary>
        NoSuchExtendedAttribute = NoData,
        /// <summary>Not supported</summary>
        NotSupported = -252, EPERM = NotPermitted,
        /// <summary>No such file or directory</summary>
        ENOENT = NoSuchFile, ESRCH = NoSuchProcess, EINTR = InterruptedSyscall,
        /// <summary>I/O error</summary>
        EIO = InOutError, ENXIO = NoSuchDeviceOrAddress, E2BIG = ArgumentListTooLong, ENOEXEC = ExecutableFormatError,
        EBADF                   = BadFileNumber, ECHILD        = NoChildProcess, EAGAIN       = TryAgain,
        ENOMEM                  = OutOfMemory,
        /// <summary>Access denied</summary>
        EACCES = AccessDenied, EFAULT = BadAddress, ENOTBLK = NotABlockDevice,
        /// <summary>Busy, cannot complete</summary>
        EBUSY = Busy, EEXIST = FileExists, EXDEV = CrossDeviceLink,
        /// <summary>No such device</summary>
        ENODEV = NoSuchDevice,
        /// <summary>Is not a directory (e.g.: trying to ReadDir() a file)</summary>
        ENOTDIR = NotDirectory,
        /// <summary>Is a directory (e.g.: trying to Read() a dir)</summary>
        EISDIR = IsDirectory,
        /// <summary>Invalid argument</summary>
        EINVAL = InvalidArgument, ENFILE = FileTableOverflow, EMFILE = TooManyOpenFiles, ENOTTY = NotTypewriter,
        ETXTBSY                          = TextFileBusy,
        /// <summary>File is too large</summary>
        EFBIG = FileTooLarge, ENOSPC = NoSpaceLeft, ESPIPE = IllegalSeek, EROFS = ReadOnly,
        EMLINK                       = TooManyLinks, EPIPE = BrokenPipe, EDOM   = OutOfDomain,
        ERANGE                       = OutOfRange, EDEADLK = DeadlockWouldOccur,
        /// <summary>Name is too long</summary>
        ENAMETOOLONG = NameTooLong, ENOLCK = NoLocksAvailable,
        /// <summary>Not implemented</summary>
        ENOSYS = NotImplemented,
        /// <summary>Link is severed</summary>
        ENOLINK = SeveredLink,
        /// <summary>Not supported</summary>
        ENOTSUP = NotSupported, DirectoryNotEmpty = -39, TooManySymbolicLinks = -40, ENOTEMPTY = DirectoryNotEmpty,
        ELOOP                                     = TooManySymbolicLinks,
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
        SectorNotFound=29
    }
}