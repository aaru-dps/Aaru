// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IFilter.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface for a Filter.
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

using System;
using System.IO;
using Aaru.CommonTypes.Enums;

namespace Aaru.CommonTypes.Interfaces
{
    /// <summary>
    ///     Defines a filter, that is, a transformation of the data from a file, like, for example, a compressor (e.g.
    ///     GZIP), or a container (e.g. AppleDouble)
    /// </summary>
    public interface IFilter
    {
        /// <summary>Descriptive name of the plugin</summary>
        string Name { get; }
        /// <summary>Unique UUID of the plugin</summary>
        Guid Id { get; }
        /// <summary>Plugin author</summary>
        string Author { get; }

        /// <summary>
        ///     Gets the path used to open this filter.<br /> UNIX: /path/to/archive.zip/path/to/file.bin =&gt;
        ///     /path/to/archive.zip/path/to/file.bin <br /> Windows: C:\path\to\archive.zip\path\to\file.bin =&gt;
        ///     C:\path\to\archive.zip\path\to\file.bin
        /// </summary>
        /// <returns>Path used to open this filter.</returns>
        string BasePath { get; }

        /// <summary>Gets creation time of file referenced by this filter.</summary>
        /// <returns>The creation time.</returns>
        DateTime CreationTime { get; }

        /// <summary>Gets length of this filter's data fork.</summary>
        /// <returns>The data fork length.</returns>
        long DataForkLength { get; }

        /// <summary>
        ///     Gets the filename for the file referenced by this filter.<br /> UNIX: /path/to/archive.zip/path/to/file.bin =
        ///     &gt; file.bin <br /> Windows: C:\path\to\archive.zip\path\to\file.bin =&gt; file.bin
        /// </summary>
        /// <returns>The filename.</returns>
        string Filename { get; }

        /// <summary>Gets last write time of file referenced by this filter.</summary>
        /// <returns>The last write time.</returns>
        DateTime LastWriteTime { get; }

        /// <summary>Gets length of file referenced by ths filter.</summary>
        /// <returns>The length.</returns>
        long Length { get; }

        /// <summary>
        ///     Gets full path to file referenced by this filter. If it's an archive, it's the path inside the archive.<br />
        ///     UNIX: /path/to/archive.zip/path/to/file.bin =&gt; /path/to/file.bin <br /> Windows:
        ///     C:\path\to\archive.zip\path\to\file.bin =&gt; \path\to\file.bin
        /// </summary>
        /// <returns>The path.</returns>
        string Path { get; }

        /// <summary>
        ///     Gets path to parent folder to the file referenced by this filter. If it's an archive, it's the full path to
        ///     the archive itself.<br /> UNIX: /path/to/archive.zip/path/to/file.bin =&gt; /path/to/archive.zip <br /> Windows:
        ///     C:\path\to\archive.zip\path\to\file.bin =&gt; C:\path\to\archive.zip
        /// </summary>
        /// <returns>The parent folder.</returns>
        string ParentFolder { get; }

        /// <summary>Gets length of this filter's resource fork.</summary>
        /// <returns>The resource fork length.</returns>
        long ResourceForkLength { get; }

        /// <summary>Returns true if the file referenced by this filter has a resource fork</summary>
        bool HasResourceFork { get; }

        /// <summary>Closes all opened streams.</summary>
        void Close();

        /// <summary>Gets a stream to access the data fork contents.</summary>
        /// <returns>The data fork stream.</returns>
        Stream GetDataForkStream();

        /// <summary>Gets a stream to access the resource fork contents.</summary>
        /// <returns>The resource fork stream.</returns>
        Stream GetResourceForkStream();

        /// <summary>Identifies if the specified path contains data recognizable by this filter instance</summary>
        /// <param name="path">Path.</param>
        bool Identify(string path);

        /// <summary>Identifies if the specified stream contains data recognizable by this filter instance</summary>
        /// <param name="stream">Stream.</param>
        bool Identify(Stream stream);

        /// <summary>Identifies if the specified buffer contains data recognizable by this filter instance</summary>
        /// <param name="buffer">Buffer.</param>
        bool Identify(byte[] buffer);

        /// <summary>Opens the specified path with this filter instance</summary>
        /// <param name="path">Path.</param>
        ErrorNumber Open(string path);

        /// <summary>Opens the specified stream with this filter instance</summary>
        /// <param name="stream">Stream.</param>
        ErrorNumber Open(Stream stream);

        /// <summary>Opens the specified buffer with this filter instance</summary>
        /// <param name="buffer">Buffer.</param>
        ErrorNumber Open(byte[] buffer);
    }
}