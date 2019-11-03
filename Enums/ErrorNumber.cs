// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

namespace DiscImageChef.CommonTypes.Enums
{
    /// <summary>Enumerates error codes. Positive for warnings or informative codes, negative for errors.</summary>
    public enum ErrorNumber
    {
        /// <summary>No error</summary>
        NoError = 0, /// <summary>User requested help to be shown</summary>
        HelpRequested = 1, /// <summary>Command found nothing</summary>
        NothingFound = 2, /// <summary>Media has been already dumped completely</summary>
        AlreadyDumped = 3, /// <summary>Image and its sectors cannot be verified</summary>
        NotVerificable = 4, /// <summary>There are bad sectors and image cannot be verified</summary>
        BadSectorsImageNotVerified = 5, /// <summary>All sectors are good and image cannot be verified</summary>
        CorrectSectorsImageNotVerified = 6, /// <summary>Image is bad and sectors cannot be verified</summary>
        BadImageSectorsNotVerified = 7, /// <summary>Image is bad and there are bad sectors</summary>
        BadImageBadSectors = 8, /// <summary>All sectors are good and image is bad</summary>
        CorrectSectorsBadImage = 9, /// <summary>Image is good and sectors cannot be verified</summary>
        CorrectImageSectorsNotVerified = 10, /// <summary>Image is good and there are bad sectors</summary>
        CorrectImageBadSectors = 11, /// <summary>Exception has been raised</summary>
        UnexpectedException = -1, /// <summary>The number of arguments is not as expected</summary>
        UnexpectedArgumentCount = -2, /// <summary>A required argument is not present</summary>
        MissingArgument = -3, /// <summary>A specified argument contains an invalid value</summary>
        InvalidArgument = -4, /// <summary>The specified file cannot be found</summary>
        FileNotFound = -5, /// <summary>The specified file cannot be opened</summary>
        CannotOpenFile = -6, /// <summary>The specified encoding cannot be found</summary>
        EncodingUnknown = -7, /// <summary>The image format has not been recognized</summary>
        UnrecognizedFormat = -8, /// <summary>The image format failed to open</summary>
        CannotOpenFormat = -9, /// <summary>The specified metadata sidecar does not have the correct format</summary>
        InvalidSidecar = -10, /// <summary>The specified resume map does not have the correct format</summary>
        InvalidResume = -11, /// <summary>The specified destination file/folder already exists</summary>
        DestinationExists = -12, /// <summary>The specified image format cannot be found</summary>
        FormatNotFound = -13, /// <summary>More than one format found for the specified search criteria</summary>
        TooManyFormats = -14, /// <summary>The specified format does not support the specified media</summary>
        UnsupportedMedia = -15, /// <summary>Data will be lost writing the specified format</summary>
        DataWillBeLost = -16, /// <summary>Cannot create destination format</summary>
        CannotCreateFormat = -17, /// <summary>Error writing data</summary>
        WriteError = -18, /// <summary>Argument expected a directory, but found a file</summary>
        ExpectedDirectory = -19, /// <summary>Argument expected a file, but found a directory</summary>
        ExpectedFile = -20, /// <summary>Cannot open device</summary>
        CannotOpenDevice = -21, /// <summary>The specified operation requires administrative privileges</summary>
        NotEnoughPermissions = -22
    }
}